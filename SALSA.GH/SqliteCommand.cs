using Grasshopper.Kernel;
using System;
using System.Data.SQLite;

namespace SALSA.GH
{
    /// <summary>
    /// Execute an sqlite command, which does not return rows. Commands like INSERT, UPDATE, DELETE, or CREATE TABLE.
    /// </summary>
    public class SqliteCommand : SqliteDBFilepathBaseComponent
    {
        public SqliteCommand()
          : base("Sqlite Command", "Command",
            "Execute an sqlite command, which does not return rows. Commands like INSERT, UPDATE, DELETE, or CREATE TABLE.")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Execute", "Exec", "Set to true to execute the command", GH_ParamAccess.item);
            pManager.AddTextParameter("File Path", "Path", "Path to the SQLite file", GH_ParamAccess.item);
            pManager.AddTextParameter("Command Text", "Cmd", "SQL command text to execute", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("out", "out", "Result message of the execution", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!GetData(DA, 0, out bool active) || !active) return;

            if (!GetFilepathAndValidate(DA, out string filePath)) return;

            if (!GetData(DA, 2, out string commandText)) return;

            // Connection to SQLite database
            string connectionString = $"Data Source={filePath}";
            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Execute the provided command
                    var command = connection.CreateCommand();
                    command.CommandText = commandText;
                    int result = command.ExecuteNonQuery();

                    DA.SetData(0, $"Successful! Rows affected: {result}.");

                }
                catch (Exception ex)
                {
                    RaiseWarning(DA, $"Error: {ex.Message}");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("6f0bed7b-e47e-4e1e-98a6-4517d52e64d8"); }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add an icon here
                return null;
            }
        }
    }
}
