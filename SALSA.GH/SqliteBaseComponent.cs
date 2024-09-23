using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SALSA.GH
{

    public abstract class SqliteBaseComponent : GH_Component
    {
        protected SqliteBaseComponent(string name, string nickname, string description, string subcategory) : base(name, nickname, description, SALSAConstants.Category, subcategory)
        {
        }

        protected SqliteBaseComponent()
        {
        }

        /// <summary>
        /// Index of the output message parameter. If this is set to -1, no message will be output.
        /// </summary>
        protected virtual int OutMessageIndex { get; set; } = 0;

        public bool GetData<T>(IGH_DataAccess DA, int index, out T value, bool optional = false)
        {
            value = default(T);
            var success = DA.GetData(index, ref value);
            if (!optional && !success)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not get data from parameter: " + Params.Input[index].Name + ".");
            return true;
        }

        public bool GetDataList<T>(IGH_DataAccess DA, int index, out List<T> value, bool optional = false)
        {
            value = new List<T>();
            var success = DA.GetDataList(index, value);
            if (!optional && !success)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not get data from parameter: " + Params.Input[index].Name + ".");
            return true;
        }

        public void RaiseError(IGH_DataAccess DA, string message)
        {
            if(OutMessageIndex > 0) DA.SetData(OutMessageIndex, message);
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
        }

        public void RaiseWarning(IGH_DataAccess DA, string message)
        {
            if (OutMessageIndex > 0) DA.SetData(OutMessageIndex, message);
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
        }

        public void RaiseRemark(IGH_DataAccess DA, string message)
        {
            if (OutMessageIndex > 0) DA.SetData(OutMessageIndex, message);
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, message);
        }
    }
}
