using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System.Windows.Forms;

namespace SALSA.GH
{
    /// <summary>
    /// A parameter for a create DataTable componnent which allows a user to specify the name and data type of a column.
    /// </summary>
    public class DataColumnParameter : GH_Param<IGH_Goo>
    {
        private SqlDataType columnDataType;
        public SqlDataType ColumnDataType
        {
            get { return columnDataType; }
            set
            {
                columnDataType = value;
                UpdateDescription();
                OnObjectChanged(GH_ObjectEventType.Options);
            }
        }

        public DataColumnParameter() : this("ColumnX")
        {
        }

        public DataColumnParameter(string Name) : base(Name, Name, null, SALSAConstants.Category, SALSAConstants.Subcategory_DataTable, GH_ParamAccess.list)
        {
            columnDataType = SqlDataType.Text;
            UpdateDescription();
        }


        public override Guid ComponentGuid => new Guid("e3e45f9d-2d3a-4e0b-8b77-6c50b81eae54");

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            ToolStripMenuItem dataTypeMenu = GH_DocumentObject.Menu_AppendItem(menu, "Data Type");
            GH_DocumentObject.Menu_AppendSeparator(menu);

            // Add menu items for data types
            GH_DocumentObject.Menu_AppendItem(menu, "Text", DataTypeMenuClicked, true, columnDataType == SqlDataType.Text).Tag = SqlDataType.Text;
            GH_DocumentObject.Menu_AppendItem(menu, "Boolean", DataTypeMenuClicked, true, columnDataType == SqlDataType.Boolean).Tag = SqlDataType.Boolean;
            GH_DocumentObject.Menu_AppendItem(menu, "Integer", DataTypeMenuClicked, true, columnDataType == SqlDataType.Integer).Tag = SqlDataType.Integer;
            GH_DocumentObject.Menu_AppendItem(menu, "Double", DataTypeMenuClicked, true, columnDataType == SqlDataType.Double).Tag = SqlDataType.Double;
        }

        private void DataTypeMenuClicked(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is SqlDataType dt)
            {
                RecordUndoEvent("Set Data Type");
                ColumnDataType = dt;
                ExpireSolution(true);
            }
        }

        public void SetName(string name)
        {
            Name = name;
            NickName = name;
            UpdateDescription();
        }

        public void UpdateDescription()
        {
            Description = $"Column named \"{Name}\" of type {columnDataType.ToString()}";
        }

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetInt32("ColumnDataType", (int)columnDataType);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("ColumnDataType"))
            {
                columnDataType = (SqlDataType)reader.GetInt32("ColumnDataType");
            }
            return base.Read(reader);
        }
    }
}
