﻿using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SALSA.GH
{
    /// <summary>
    /// Component to perform a batch insert into an SQLite table with variable columns.
    /// </summary>
    public class SqliteBatchInsert : SqliteDBFilepathBaseComponent, IGH_VariableParameterComponent
    {
        public SqliteBatchInsert()
          : base("Sqlite Batch Insert", "BatchInsert",
            "Performs a batch insert into an SQLite table with variable columns.")
        {
            State = ComponentState.AutoGenerate;
        }

        private ComponentState state;
        private ComponentState State
        {
            get => state;
            set
            {
                state = value;
                Message = value.ToString().Replace('_', ' ');
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Execute", "Exec", "Set to true to execute the command", GH_ParamAccess.item);
            pManager.AddTextParameter("File Path", "Path", "Path to the SQLite file", GH_ParamAccess.item);
            pManager.AddTextParameter("Table Name", "Table", "Name of the table to insert into", GH_ParamAccess.item);
            // Column parameters are managed dynamically
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("out", "out", "Result message of the execution", GH_ParamAccess.item);
        }

        protected override void BeforeSolveInstance()
        {
            base.BeforeSolveInstance();

            if (State == ComponentState.AutoGenerate)
            {
                AutoGenerateInputs();
            }
        }

        //Caches to prevent unnecessary re-computation of AutoGenerate every time values remain the same.
        private string cachedInputFilepath; //Note this filepath is BEFORE validation and improvement. 
        private string cachedTableName;
        private void ClearAutogeneratedCache()
        {
            cachedInputFilepath = null;
            cachedTableName = null;
        }


        private void AutoGenerateInputs()
        {
            // Get the File Path and Table Name from the inputs
            string filePath = null;
            string tableName = null;

            if (Params.Input[1].VolatileDataCount > 0)
            {
                GH_String dbFilePath = Params.Input[1].VolatileData.AllData(true).FirstOrDefault() as GH_String;
                if (dbFilePath != null)
                {
                    filePath = dbFilePath.Value;
                }
            }

            if (Params.Input[2].VolatileDataCount > 0)
            {
                GH_String ghTableName = Params.Input[2].VolatileData.AllData(true).FirstOrDefault() as GH_String;
                if (ghTableName != null)
                {
                    tableName = ghTableName.Value;
                }
            }

            if(cachedInputFilepath != null && cachedTableName != null && filePath == cachedInputFilepath && tableName == cachedTableName)
            {
                //No need to recompute
                return;
            }

            //Cache the values to avoid unnecessary re-computation
            cachedInputFilepath = filePath;
            cachedTableName = tableName;

            if (!ValidateImproveFilepath(ref filePath)) return;

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(tableName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File Path and Table Name must be provided for AutoGenerate.");
                return;
            }

            // Connect to the database and get the column names and types
            List<ColumnInfo> columns = GetTableColumns(filePath, tableName, out string getTableMessage);
            if (columns == null || columns.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Failed to retrieve columns from table '{tableName}' due to error: {getTableMessage}.");
                return;
            }

            // Adjust input parameters
            int requiredInputs = 3 + columns.Count; // 3 fixed inputs + columns
            int currentInputs = Params.Input.Count;

            if (requiredInputs != currentInputs)
            {
                RecordUndoEvent("Adjusting Inputs");

                if (currentInputs < requiredInputs)
                {
                    for (int i = currentInputs; i < requiredInputs; i++)
                    {
                        var newParam = CreateParameter(GH_ParameterSide.Input, i);
                        if (newParam != null)
                            Params.RegisterInputParam(newParam);
                    }
                }
                else if (currentInputs > requiredInputs)
                {
                    for (int i = currentInputs - 1; i >= requiredInputs; i--)
                    {
                        Params.UnregisterInputParameter(Params.Input[i]);
                    }
                }

                Params.OnParametersChanged();
                ExpireSolution(true);
                //return; // Need to expire and recompute
            }

            // Update parameter names, nicknames, descriptions, and data types
            for (int i = 3; i < Params.Input.Count; i++)
            {
                var dcParam = Params.Input[i] as DataColumnParameter;
                if (dcParam != null)
                {
                    int colIndex = i - 3;
                    var columnInfo = columns[colIndex];

                    dcParam.ColumnDataType = columnInfo.DataType;
                    dcParam.SetName(columnInfo.Name);
                    dcParam.Access = GH_ParamAccess.list;
                    dcParam.MutableNickName = false;
                }
            }
        }

        private List<ColumnInfo> GetTableColumns(string filePath, string tableName, out string message)
        {
            string connectionString = $"Data Source={filePath}";
            List<ColumnInfo> columns = new List<ColumnInfo>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = $"PRAGMA table_info({tableName});";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string columnName = reader["name"].ToString();
                                string columnTypeStr = reader["type"].ToString();
                                SqlDataType columnType = MapSQLiteTypeToSqlDataType(columnTypeStr);

                                columns.Add(new ColumnInfo { Name = columnName, DataType = columnType });
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    message = ex.Message;
                    return null;
                }
                finally
                {
                    connection.Close();
                }
            }

            message = null;
            return columns;
        }

        private SqlDataType MapSQLiteTypeToSqlDataType(string sqliteType)
        {
            // Simplified mapping, adjust as needed
            sqliteType = sqliteType.ToUpperInvariant();

            if (sqliteType.Contains("INT"))
                return SqlDataType.Integer;
            else if (sqliteType.Contains("CHAR") || sqliteType.Contains("CLOB") || sqliteType.Contains("TEXT"))
                return SqlDataType.Text;
            else if (sqliteType.Contains("BLOB"))
                return SqlDataType.Text; // Or handle BLOB differently
            else if (sqliteType.Contains("REAL") || sqliteType.Contains("FLOA") || sqliteType.Contains("DOUB"))
                return SqlDataType.Double;
            else if (sqliteType.Contains("BOOL"))
                return SqlDataType.Boolean;
            else
                return SqlDataType.Text; // Default
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!GetData(DA, 0, out bool active) || !active)
            {
                DA.SetData(0, "Set Execute to true to run.");
                return;
            }

            if (!GetFilepathAndValidate(DA, out string filePath)) return;

            if (!GetData(DA, 2, out string tableName)) return;

            // Collect column data
            int columnCount = Params.Input.Count - 3;
            List<DataColumnParameter> columnParams = new List<DataColumnParameter>();

            for (int i = 3; i < Params.Input.Count; i++)
            {
                var param = Params.Input[i] as DataColumnParameter;
                if (param != null)
                    columnParams.Add(param);
                else
                {
                    RaiseWarning(DA, $"Input parameter at index {i} is not a DataColumnParameter.");
                    return;
                }
            }

            // Check if columns exist in table and types match
            List<ColumnInfo> tableColumns = GetTableColumns(filePath, tableName, out string getTableErrorMessage);
            if (tableColumns == null || tableColumns.Count == 0)
            {
                RaiseWarning(DA, $"Failed to retrieve columns from table '{tableName}' due to error: {getTableErrorMessage}");
                return;
            }

            // Build a mapping of column names to types
            Dictionary<string, SqlDataType> tableColumnDict = tableColumns.ToDictionary(c => c.Name, c => c.DataType);

            // Check that the input columns exist in the table and types match
            foreach (var colParam in columnParams)
            {
                if (!tableColumnDict.TryGetValue(colParam.Name, out SqlDataType tableDataType))
                {
                    RaiseWarning(DA, $"Column '{colParam.Name}' does not exist in table '{tableName}'.");
                    return;
                }
                if (tableDataType != colParam.ColumnDataType)
                {
                    RaiseWarning(DA, $"Type mismatch for column '{colParam.Name}'. Expected '{tableDataType}', got '{colParam.ColumnDataType}'.");
                    return;
                }
            }

            // Get data from the input parameters
            int rowCount = -1;
            List<List<object>> columnData = new List<List<object>>();

            for (int i = 0; i < columnParams.Count; i++)
            {
                var colParam = columnParams[i];
                List<object> data = new List<object>();
                if (!DA.GetDataList(i + 3, data))
                {
                    RaiseWarning(DA, $"Failed to get data for column '{colParam.Name}'.");
                    return;
                }

                if (rowCount == -1)
                    rowCount = data.Count;
                else if (data.Count != rowCount)
                {
                    RaiseWarning(DA, $"Data count mismatch in column '{colParam.Name}'. Expected {rowCount}, got {data.Count}.");
                    return;
                }

                columnData.Add(data);
            }

            // Build and execute the INSERT INTO statements
            string connectionString = $"Data Source={filePath}";
            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        for (int row = 0; row < rowCount; row++)
                        {
                            var command = connection.CreateCommand();
                            List<string> columnNames = new List<string>();
                            List<string> parameterNames = new List<string>();
                            for (int col = 0; col < columnParams.Count; col++)
                            {
                                var colParam = columnParams[col];
                                columnNames.Add(colParam.Name);
                                parameterNames.Add($"@param{col}");

                                var value = columnData[col][row];

                                command.Parameters.AddWithValue($"@param{col}", value);
                            }

                            string columnsPart = string.Join(", ", columnNames.Select(c => $"[{c}]"));
                            string valuesPart = string.Join(", ", parameterNames);

                            command.CommandText = $"INSERT INTO [{tableName}] ({columnsPart}) VALUES ({valuesPart});";

                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }

                    DA.SetData(0, $"Successfully inserted {rowCount} rows into '{tableName}'.");
                }
                catch (Exception ex)
                {
                    RaiseWarning(DA, $"Error executing insert: {ex.Message}");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && State == ComponentState.ManualEdit && index >= 3;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && State == ComponentState.ManualEdit && index >= 3;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
            {
                var dcParam = new DataColumnParameter();
                dcParam.SetName("Column " + (index - 3));
                dcParam.Access = GH_ParamAccess.list;
                dcParam.MutableNickName = true; // By default, set to true
                dcParam.Optional = true;
                return dcParam;
            }
            return null;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && State == ComponentState.ManualEdit && index >= 3;
        }

        public void VariableParameterMaintenance()
        {
            if (State == ComponentState.Locked || State == ComponentState.ManualEdit)
            {
                // Ensure parameters have correct properties
                for (int i = 3; i < Params.Input.Count; i++)
                {
                    var param = Params.Input[i];
                    if (param is DataColumnParameter colParam)
                    {
                        colParam.MutableNickName = State == ComponentState.ManualEdit;
                        colParam.Access = GH_ParamAccess.list;
                    }
                }
            }
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem autoGenerateItem = new ToolStripMenuItem("Auto Generate Inputs", null, AutoGenerate_Click)
            {
                Checked = State == ComponentState.AutoGenerate,
                Enabled = State != ComponentState.AutoGenerate
            };
            menu.Items.Add(autoGenerateItem);

            ToolStripMenuItem manualEditItem = new ToolStripMenuItem("Manual Edit Inputs", null, ManualEdit_Click)
            {
                Checked = State == ComponentState.ManualEdit,
                Enabled = State != ComponentState.ManualEdit
            };
            menu.Items.Add(manualEditItem);

            ToolStripMenuItem lockItem = new ToolStripMenuItem("Lock Inputs", null, Lock_Click)
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
            AutoGenerateInputs();
            VariableParameterMaintenance();
            ExpireSolution(true);
        }

        private void ManualEdit_Click(object sender, EventArgs e)
        {
            RecordUndoEvent("Switch to Manual Edit");
            State = ComponentState.ManualEdit;
            ClearAutogeneratedCache();
            VariableParameterMaintenance();
            ExpireSolution(true);
        }

        private void Lock_Click(object sender, EventArgs e)
        {
            RecordUndoEvent("Lock Inputs");
            State = ComponentState.Locked;
            ClearAutogeneratedCache();
            VariableParameterMaintenance();
            ExpireSolution(true);
        }

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
            
            ClearAutogeneratedCache();
            return base.Read(reader);
        }



        public override Guid ComponentGuid => new Guid("6f0bed7b-e47e-4e1e-98a6-4517d52e64d9");

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add an icon here
                return null;
            }
        }

        private class ColumnInfo
        {
            public string Name { get; set; }
            public SqlDataType DataType { get; set; }
        }
    }
}
