using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System.Windows.Forms;


namespace SALSA.GH
{

    public enum ComponentState
    {
        AutoGenerate,
        ManualEdit,
        Locked
    }

    public class ReadDataTableComponent : SqliteBaseComponent, IGH_VariableParameterComponent
    {
        public ReadDataTableComponent()
          : base("Read DataTable", "ReadDT",
              "Reads a DataTable and outputs its columns",
              SALSAConstants.Subcategory_DataTable)
        {
            State = ComponentState.AutoGenerate;
        }

        public override Guid ComponentGuid => new Guid("5f1a7466-7da4-4b02-9d0d-9852360ef3ba");

        protected override System.Drawing.Bitmap Icon => null; // Add an icon if needed



        private ComponentState State
        {
            get => state;
            set
            {
                state = value;
                Message = value.ToString().Replace('_', ' ');
            }
        }

        private ComponentState state;

        private List<string> columnNames = null;
        private bool[] paramUsed;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new DataTable_Param(), "DataTable", "DT", "Input DataTable", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // Output parameters are managed dynamically
        }

        protected override void BeforeSolveInstance()
        {
            base.BeforeSolveInstance();

            if (State == ComponentState.AutoGenerate)
            {
                AutoGenerateOutputs();
            }

            paramUsed = new bool[Params.Output.Count];
        }

        private void AutoGenerateOutputs()
        {
            // Get the DataTable from the input
            DataTable dataTable = null;
            if (Params.Input[0].VolatileDataCount > 0)
            {
                var dataTableGoo = Params.Input[0].VolatileData.AllData(true).FirstOrDefault() as DataTable_Goo;
                dataTable = dataTableGoo?.Value;
            }

            if (dataTable == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No DataTable provided.");
                return;
            }

            // Get column names
            columnNames = dataTable.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToList();

            // Adjust output parameters
            int requiredOutputs = columnNames.Count;
            int currentOutputs = Params.Output.Count;

            if (requiredOutputs != currentOutputs)
            {
                RecordUndoEvent("Adjusting Outputs");

                if (currentOutputs < requiredOutputs)
                {
                    for (int i = currentOutputs; i < requiredOutputs; i++)
                    {
                        var newParam = CreateParameter(GH_ParameterSide.Output, i);
                        Params.RegisterOutputParam(newParam);
                    }
                }
                else if (currentOutputs > requiredOutputs)
                {
                    for (int i = currentOutputs - 1; i >= requiredOutputs; i--)
                    {
                        Params.UnregisterOutputParameter(Params.Output[i]);
                    }
                }

                Params.OnParametersChanged();
                ExpireSolution(true);
            }

            // Update parameter names and nicknames
            for (int i = 0; i < Params.Output.Count; i++)
            {
                var param = Params.Output[i];
                param.Name = columnNames[i];
                param.NickName = columnNames[i];
                param.Description = $"Data from column: {columnNames[i]}";
                param.Access = GH_ParamAccess.list;
                param.MutableNickName = State == ComponentState.ManualEdit;
            }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DataTable_Goo dataTableGoo = null;
            if (!DA.GetData(0, ref dataTableGoo) || dataTableGoo == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No DataTable provided.");
                return;
            }

            DataTable dataTable = dataTableGoo.Value;

            if (dataTable == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "DataTable is null.");
                return;
            }

            for (int i = 0; i < Params.Output.Count; i++)
            {
                var param = Params.Output[i];
                var columnName = param.NickName;

                if (!dataTable.Columns.Contains(columnName))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Column '{columnName}' does not exist in the DataTable.");
                    continue;
                }

                var columnData = dataTable.Columns[columnName];
                List<object> values = new List<object>();

                foreach (DataRow row in dataTable.Rows)
                {
                    values.Add(row[columnData]);
                }

                paramUsed[i] = true;
                DA.SetDataList(i, values);
            }
        }

        protected override void AfterSolveInstance()
        {
            base.AfterSolveInstance();

            // Warn about unused output parameters
            if (paramUsed != null && paramUsed.Any(u => !u))
            {
                var unusedParams = Params.Output.Where((param, index) => !paramUsed[index]);
                string unusedColumns = string.Join(", ", unusedParams.Select(p => p.NickName));
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Unused output parameters: {unusedColumns}");
            }
        }

        #region Variable Parameter Implementation

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Output && State == ComponentState.ManualEdit;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Output && State == ComponentState.ManualEdit;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            if (side != GH_ParameterSide.Output)
                return null;

            var param = new Param_GenericObject();
            param.Name = "Column" + index;
            param.NickName = "Column" + index;
            param.Description = "DataTable Column";
            param.Access = GH_ParamAccess.list;
            param.MutableNickName = true;
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Output && State == ComponentState.ManualEdit;
        }

        public void VariableParameterMaintenance()
        {
            if (State == ComponentState.Locked || State == ComponentState.ManualEdit)
            {
                // Ensure parameters have correct properties
                foreach (var param in Params.Output)
                {
                    param.MutableNickName = State == ComponentState.ManualEdit;
                    param.Access = GH_ParamAccess.list;
                }
            }
            else if (columnNames != null)
            {
                // Update parameter names in auto-generate mode
                for (int i = 0; i < Params.Output.Count; i++)
                {
                    var dcParam = Params.Output[i] as DataColumnParameter;
                    if (dcParam == null)
                    {
                        throw new Exception("Something has gone wrong with the type of param. Expected DataColumnParameter but got something else.");
                    }
                    dcParam.SetName(columnNames[i]);
                    Params.Output[i].Access = GH_ParamAccess.list;
                    Params.Output[i].MutableNickName = false;
                }
            }
        }

        #endregion

        #region Menu Items

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem autoGenerateItem = new ToolStripMenuItem("Auto Generate Outputs", null, AutoGenerate_Click)
            {
                Checked = State == ComponentState.AutoGenerate,
                Enabled = State != ComponentState.AutoGenerate
            };
            menu.Items.Add(autoGenerateItem);

            ToolStripMenuItem manualEditItem = new ToolStripMenuItem("Manual Edit Outputs", null, ManualEdit_Click)
            {
                Checked = State == ComponentState.ManualEdit,
                Enabled = State != ComponentState.ManualEdit
            };
            menu.Items.Add(manualEditItem);

            ToolStripMenuItem lockItem = new ToolStripMenuItem("Lock Outputs", null, Lock_Click)
            {
                Checked = State == ComponentState.Locked,
                Enabled = State != ComponentState.Locked
            };
            menu.Items.Add(lockItem);
        }

        private void AutoGenerate_Click(object sender, EventArgs e)
        {
            RecordUndoEvent("Switch to Auto Generate");
            State = ComponentState.AutoGenerate;
            VariableParameterMaintenance();
            ExpireSolution(true);
        }

        private void ManualEdit_Click(object sender, EventArgs e)
        {
            RecordUndoEvent("Switch to Manual Edit");
            State = ComponentState.ManualEdit;
            VariableParameterMaintenance();
            ExpireSolution(true);
        }

        private void Lock_Click(object sender, EventArgs e)
        {
            RecordUndoEvent("Lock Outputs");
            State = ComponentState.Locked;
            VariableParameterMaintenance();
            ExpireSolution(true);
        }

        #endregion

        #region Serialization

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetInt32("State", (int)State);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("State"))
            {
                int stateInt = reader.GetInt32("State");
                State = (ComponentState)stateInt;
            }
            return base.Read(reader);
        }

        #endregion
    }

}
