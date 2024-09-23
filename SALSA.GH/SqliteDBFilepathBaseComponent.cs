using Grasshopper.Kernel;
using System;
using System.IO;
using System.Linq;

namespace SALSA.GH
{
    public abstract class SqliteDBFilepathBaseComponent : SqliteBaseComponent
    {
        protected SqliteDBFilepathBaseComponent(string name, string nickname, string description) : base(name, nickname, description, SALSAConstants.Subcategory_Sqlite)
        {
        }

        protected SqliteDBFilepathBaseComponent()
        {
        }



        protected virtual int FilepathIndex { get; set; } = 1;

        /// <summary>
        /// If there is only one file path input, this will store the file path for later use.
        /// </summary>
        private bool _getFilepathOnce;
        private string _filePath;

        protected override void BeforeSolveInstance()
        {
            base.BeforeSolveInstance();

            var fpaths = Params.Input[FilepathIndex].VolatileData.AllData(true);
            _getFilepathOnce = fpaths.Count() == 1;
            if(_getFilepathOnce)
            {
                _filePath = fpaths.First().ToString();
            }
            else
            {
                _filePath = null;
            }

        }

        protected bool GetFilepathAndValidate(IGH_DataAccess DA, out string filePath/*, bool requireFileExistence = true*/)
        {
            //If the file path is already set, return it
            //_getFilepathOnce is true if there is only one file path input
            if (_getFilepathOnce && !string.IsNullOrEmpty(_filePath))
            {
                filePath = _filePath;
                return true;
            }

            filePath = string.Empty;
            if (!DA.GetData(FilepathIndex, ref filePath)) return false;

            return ValidateImproveFilepath(ref filePath);
        }

        protected bool ValidateImproveFilepath(ref string filePath)
        {
            // Check if the file path is null or empty
            if (string.IsNullOrEmpty(filePath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error: File path is null or empty.");
                return false;
            }

            // Ensure the file path has a .db extension
            if (!filePath.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".db";
            }

            //If the file exists, no further validation is needed
            if (File.Exists(filePath))
            {
                return true;
            }

            // Check if the file path contains directory separators (slashes)
            var somepath = Path.GetDirectoryName(filePath);

            if (Directory.Exists(somepath))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(somepath))
            {
                var x = 1; //Breakpoint
            }
            //if (filePath.Contains("\\") || filePath.Contains("/"))
            //{
            //    //Path does not exist
            //    var y = 1; //Breakpoint
            //    return false; //Create a directory if it does not exist????
            //}
            //Beyond this point, the filepath needs a directory -get it from the save file...

            //this check is not needed
            if (!filePath.Contains("\\") && !filePath.Contains("/"))
            {
                string grasshopperDocPath = OnPingDocument().FilePath; // Get the Grasshopper document path
                if (!string.IsNullOrEmpty(grasshopperDocPath))
                {
                    // Extract directory path
                    string directoryPathGH = Path.GetDirectoryName(grasshopperDocPath);
                    filePath = Path.Combine(directoryPathGH, filePath);
                    return true;
                }

                // If it's just a filename, get the current Grasshopper document folder
                string rhinoSavePath = Rhino.RhinoDoc.ActiveDoc.Path; // Get the active Rhino document path
                if (!string.IsNullOrEmpty(rhinoSavePath))
                {
                    string directoryPathRH = Path.GetDirectoryName(rhinoSavePath);
                    filePath = Path.Combine(directoryPathRH, filePath);
                    return false;
                }

                // Extract directory path
                string directoryPath = Path.GetDirectoryName(rhinoSavePath);
                filePath = Path.Combine(directoryPath, filePath);


                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid folderpath. Attempting to create at current rhino OR gh save location but there was no save path.");

            }

            // Check if the file exists
            if (/*requireFileExistence &&*/ !File.Exists(filePath))
            {
                //RaiseError(DA, $"Error: File does not exist at path '{filePath}'.");
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Error: File does not exist at path '{filePath}, we will attempt to create one.");
                //return false;
            }

            return true;
        }
    }
}
