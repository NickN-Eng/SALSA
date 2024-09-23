using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SALSA.GH
{
    /// <summary>
    /// Execute a SQLite SELECT query on a specified table.
    /// </summary>
    public class SqliteSelectQuery : SqliteDBFilepathBaseComponent
    {
        public SqliteSelectQuery()
            : base("Sqlite Select Query", "SelectQuery", "Execute a SQLite SELECT query on a specified table.")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Execute", "Exec", "Set to true to execute the query", GH_ParamAccess.item);
            pManager.AddTextParameter("File Path", "Path", "Path to the SQLite file", GH_ParamAccess.item);
            pManager.AddTextParameter("Table Name", "Table", "Name of the table to query into", GH_ParamAccess.item);
            int colIndex = pManager.AddTextParameter("Columns", "Columns", "Columns to return", GH_ParamAccess.list); // Optional
            pManager[colIndex].Optional = true;
            int whereIndex = pManager.AddTextParameter("Where", "Where", "Optional criterion for filtering the database.", GH_ParamAccess.item); // Optional
            pManager[whereIndex].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("out", "out", "Result message of the execution", GH_ParamAccess.item);
            var param = new DataTable_Param();
            pManager.AddParameter(param, "DataTable", "DT", "The result of the SQL query as a DataTable", GH_ParamAccess.item);
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
            if (!GetDataList(DA, 3, out List<string> columns, optional: true) || columns.Count == 0)
            {
                columns.Add("*"); // If no columns are specified, use '*'
            }

            // Get the where clause (optional)
            string whereClause = "";
            GetData(DA, 4, out string whereInput, optional: true);
            if (!string.IsNullOrEmpty(whereInput))
            {
                whereClause = $" WHERE {whereInput}";
            }

            // Construct the SQL query
            string columnList = string.Join(", ", columns);
            string queryText = $"SELECT {columnList} FROM {tableName}{whereClause}";

            // Execute the query
            string connectionString = $"Data Source={filePath}";
            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = queryText;
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Set the output data
                    DA.SetData(0, $"Query executed successfully.{Environment.NewLine}SQL query:{Environment.NewLine}{queryText}");
                    DA.SetData(1, new DataTable_Goo(dataTable));
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
            get { return new Guid("b2c7585d-d4c7-4d9f-8e5f-39cdd7b63778"); }
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
