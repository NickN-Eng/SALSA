using Grasshopper.Kernel;
using System;
using System.Data;
using System.Data.SQLite;


namespace SALSA.GH
{
    /// <summary>
    /// Execute a SQLite query that returns rows, such as SELECT.
    /// </summary>
    public class SqliteQuery : SqliteDBFilepathBaseComponent
    {
        public SqliteQuery()
            : base("Sqlite Query", "Query", "Execute a SQLite query that returns rows, such as SELECT.")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Execute", "Exec", "Set to true to execute the query", GH_ParamAccess.item);
            pManager.AddTextParameter("File Path", "Path", "Path to the SQLite file", GH_ParamAccess.item);
            pManager.AddTextParameter("Query Text", "Query", "SQL query text to execute", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("out", "out", "Result message of the execution", GH_ParamAccess.item);
            var param = new DataTable_Param();
            pManager.AddParameter(param, "DataTable", "Table", "The result of the SQL query as a DataTable", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!GetData(DA, 0, out bool active) || !active) return;

            if (!GetFilepathAndValidate(DA, out string filePath)) return;

            if (!GetData(DA, 2, out string queryText)) return;

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
                    DA.SetData(0, "Query executed successfully.");
                    DA.SetData(1, new DataTable_Goo(dataTable));
                }
                catch (Exception ex)
                {
                    DA.SetData(0, $"Error: {ex.Message}");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("ad15f9d2-9bf9-4f16-8390-b75a3b896252"); }
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


