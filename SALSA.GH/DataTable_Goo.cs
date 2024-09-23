using System;
using System.Data;
using System.IO;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace SALSA.GH
{
    /// <summary>
    /// A Grasshopper Goo wrapper for DataTable objects.
    /// </summary>
    public class DataTable_Goo : GH_Goo<DataTable>
    {
        public DataTable_Goo()
        {
        }

        public DataTable_Goo(DataTable internal_data) : base(internal_data)
        {
        }

        public DataTable_Goo(GH_Goo<DataTable> other) : base(other)
        {
        }

        public override bool IsValid => Value != null;

        public override string TypeName => "DataTable";

        public override string TypeDescription => "A DataTable object from System.Data";

        public override bool CastFrom(object source)
        {
            if (source == null) return false;
            if (source is DataTable dt)
            {
                this.Value = dt;
                return true;
            }
            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (typeof(Q).IsAssignableFrom(typeof(DataTable)))
            {
                if (m_value == null)
                {
                    return false;
                }
                else
                {
                    target = (Q)(object)m_value;
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new DataTable_Goo(m_value.Copy());
        }

        public override object ScriptVariable()
        {
            return m_value.Copy();
        }

        public override string ToString()
        {
            string tableName = string.IsNullOrEmpty(m_value.TableName) ? "Unnamed" : m_value.TableName;
            return $"DataTable: {tableName}, Rows: {m_value.Rows.Count}, Columns: {m_value.Columns.Count}";
        }

        public override bool Write(GH_IWriter writer)
        {
            try
            {
                using (StringWriter sw = new StringWriter())
                {
                    m_value.WriteXml(sw, XmlWriteMode.WriteSchema);
                    string xml = sw.ToString();
                    writer.SetString("DataTableXml", xml);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool Read(GH_IReader reader)
        {
            try
            {
                string xml = reader.GetString("DataTableXml");
                using (StringReader sr = new StringReader(xml))
                {
                    DataTable dt = new DataTable();
                    dt.ReadXml(sr);
                    m_value = dt;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
