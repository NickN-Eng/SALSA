using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SALSA.GH
{
    /// <summary>
    /// Create a table in the SQLite database.
    /// </summary>
    public class SqliteCreateTable : SqliteDBFilepathBaseComponent
    {
        public SqliteCreateTable()
            : base("Sqlite Create Table", "CreateTable", "Create a table in the SQLite database.")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Execute", "Exec", "Set to true to execute the command", GH_ParamAccess.item);
            pManager.AddTextParameter("File Path", "Path", "Path to the SQLite file", GH_ParamAccess.item);
            pManager.AddTextParameter("Table Name", "Table", "Name of the table to create", GH_ParamAccess.item);
            pManager.AddTextParameter("Columns", "Columns", "List of column names for the new table", GH_ParamAccess.list); // Optional
            pManager[3].Optional = true;
            pManager.AddTextParameter("DataTypes", "Types", "List of data types corresponding to each column. Accepts a variety of keywords such as text/string, number/double, int etc...", GH_ParamAccess.list); // Optional
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("out", "out", "Result message of the execution", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get the 'Execute' input
            if (!GetData(DA, 0, out bool execute) || !execute) return;

            // Get the SQLite file path and validate
            if (!GetFilepathAndValidate(DA, out string filePath)) return;

            // Get the table name
            if (!GetData(DA, 2, out string tableName) || string.IsNullOrEmpty(tableName))
            {
                DA.SetData(0, "Error: Table name is required.");
                return;
            }

            // Get the columns (optional)
            GetDataList(DA, 3, out List<string> columns);

            // Get the data types (optional)
            GetDataList(DA, 4, out List<string> dataTypes);

            // Validate columns and data types
            if (columns.Count == 0 || dataTypes.Count == 0 || columns.Count != dataTypes.Count)
            {
                DA.SetData(0, "Error: Columns and data types must be provided and have the same number of entries.");
                return;
            }

            // Construct the SQL query for creating the table
            List<string> columnDefinitions = new List<string>();
            for (int i = 0; i < columns.Count; i++)
            {
                if(!SqlDataTypeExtensions.TryParseFromNaturalLanguage(dataTypes[i], out SqlDataType dataType))
                {
                    RaiseWarning(DA, $"Error: Invalid data type '{dataTypes[i]}' for column '{columns[i]}'.");
                    return;
                }

                columnDefinitions.Add($"{columns[i]} {dataType.ToSqliteKeyword()}");
            }

            string columnDefinitionsText = string.Join(", ", columnDefinitions);
            string queryText = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnDefinitionsText});";

            // Execute the query
            string connectionString = $"Data Source={filePath}";
            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = queryText;
                    command.ExecuteNonQuery();

                    // Set the output data
                    DA.SetData(0, $"Table created successfully.{Environment.NewLine}SQL query:{Environment.NewLine}{queryText}");
                }
                catch (Exception ex)
                {
                    DA.SetData(0, $"Error:{Environment.NewLine}{ex.Message}{Environment.NewLine}SQL query:{Environment.NewLine}{queryText}");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("f1b1c48c-3c4a-4d59-bb39-3db232d367dd"); }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null; // Add an icon here if needed
            }
        }
    }
    

}
