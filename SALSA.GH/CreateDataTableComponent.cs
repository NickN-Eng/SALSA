using System;
using System.Collections.Generic;
using System.Data;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace SALSA.GH
{
    public class CreateDataTableComponent : SqliteBaseComponent, IGH_VariableParameterComponent
    {
        public CreateDataTableComponent()
          : base("Create DataTable", "DataTable",
              "Creates a DataTable with specified columns and data",
              SALSAConstants.Subcategory_DataTable)
        {
        }

        public override Guid ComponentGuid => new Guid("0c19834a-eb91-45e3-b406-625ea0ba212e");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Table Name", "Name", "Name of the DataTable", GH_ParamAccess.item, "MyDataTable");
            // Variable parameters will be managed in VariableParameterMaintenance
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new DataTable_Param(), "DataTable", "DT", "Output DataTable", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!GetData(DA, 0, out string tableName)) return;

            DataTable dataTable = new DataTable(tableName);
            int columnCount = Params.Input.Count - 1;
            if (columnCount == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No columns defined.");
                return;
            }

            int rowCount = -1;
            List<List<object>> columnsData = new List<List<object>>();

            for (int i = 1; i < Params.Input.Count; i++)
            {
                var param = Params.Input[i] as DataColumnParameter;
                if (param == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid parameter type");
                    return;
                }

                List<IGH_Goo> dataList = new List<IGH_Goo>();
                if (!DA.GetDataList(i, dataList))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Failed to get data from parameter {param.Name}");
                    return;
                }

                // Convert data to the specified data type
                List<object> convertedData = new List<object>();
                foreach (var dataItem in dataList)
                {
                    object convertedValue = param.ColumnDataType.ConvertValue(dataItem);
                    convertedData.Add(convertedValue);
                }
                columnsData.Add(convertedData);

                // Create a DataColumn in the DataTable
                DataColumn dataColumn = new DataColumn(param.NickName)
                {
                    DataType = param.ColumnDataType.GetDataType()
                };
                dataTable.Columns.Add(dataColumn);

                // Check if all columns have the same number of rows
                if (rowCount == -1)
                {
                    rowCount = convertedData.Count; //on the first column, set the row count
                }
                else if (convertedData.Count != rowCount)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "All columns must have the same number of rows");
                    return;
                }
            }

            // Populate the DataTable rows
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                DataRow dataRow = dataTable.NewRow();
                for (int colIndex = 0; colIndex < columnsData.Count; colIndex++)
                {
                    dataRow[colIndex] = columnsData[colIndex][rowIndex];
                }
                dataTable.Rows.Add(dataRow);
            }

            // Output the DataTable_Goo
            DataTable_Goo dataTableGoo = new DataTable_Goo(dataTable);
            DA.SetData(0, dataTableGoo);
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && index != 0;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && index > 0;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
            {
                var param = new DataColumnParameter($"Column{index}");
                return param;
            }
            return null;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && index > 0;
        }

        public void VariableParameterMaintenance()
        {
            for (int i = 1; i < Params.Input.Count; i++)
            {
                var param = Params.Input[i];
                if (string.IsNullOrEmpty(param.Name))
                {
                    var dcParam = Params.Output[i] as DataColumnParameter;
                    if (dcParam == null)
                    {
                        throw new Exception("Something has gone wrong with the type of param. Expected DataColumnParameter but got something else.");
                    }
                    dcParam.SetName($"Column{i}");
                    param.Access = GH_ParamAccess.list;
                }
            }
        }
    }
}
