using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using SALSA.WPF;

namespace SALSA.GH
{
    /// <summary>
    /// Parameter for System.Data.DataTable objects.
    /// </summary>
    public class DataTable_Param : GH_Param<DataTable_Goo>
    {
        public DataTable_Param() : base(
            "DataTable", "DT",
            "A parameter for System.Data.DataTable objects.",
            SALSAConstants.Category, SALSAConstants.Subcategory_DataTable,
            GH_ParamAccess.item)
        { }

        public DataTable_Param(string name, string nickname, string description, GH_ParamAccess access)
            : base(name, nickname, description, "Params", "Data", access)
        { }

        public override Guid ComponentGuid => new Guid("0d7ceaaa-917c-42da-82d5-31fb9f75477a");

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            // Add a separator for clarity
            //menu.Items.Add(new ToolStripSeparator());

            // Create the "Preview DataTable" menu item
            var previewItem = new ToolStripMenuItem("Preview DataTable");
            previewItem.Click += PreviewItem_Click;

            // Add the menu item to the menu
            menu.Items.Add(previewItem);
        }

        // Event handler for the "Preview DataTable" menu item
        private void PreviewItem_Click(object sender, EventArgs e)
        {
            // Define the refreshDataFunction
            Func<List<GHDataTable>> refreshDataFunction = () =>
            {
                var dataTables = new List<GHDataTable>();

                // Iterate through all data in the parameter
                IList<Grasshopper.Kernel.Data.GH_Path> allPaths = this.VolatileData.Paths;
                IList<System.Collections.IList> allStr = this.VolatileData.StructureProxy;

                var vData = this.VolatileData;
                foreach (Grasshopper.Kernel.Data.GH_Path path in vData.Paths)
                {
                    int i = 0;
                    foreach (var item in vData.get_Branch(path))
                    {
                        if(item is DataTable_Goo goo)
                        {
                            // Duplicate the DataTable to avoid affecting Grasshopper's data
                            DataTable duplicatedTable = goo.Value.Copy(); // Assuming GH_Goo<DataTable> has a Copy method
                            string pathStr = $"{path} [{i}]";

                            dataTables.Add(new GHDataTable(duplicatedTable, pathStr));
                        }

                        i++;
                    }
                }

                return dataTables;
            };

            // Instantiate the preview window with the refresh function
            var previewWindow = new GHDataTablePreview(refreshDataFunction);

            // Show the window (non-modal)
            previewWindow.Show();
        }


    }

}
