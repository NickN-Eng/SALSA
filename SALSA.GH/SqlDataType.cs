using Grasshopper.Kernel.Types;
using System;

namespace SALSA.GH
{
    /// <summary>
    /// Column types supported by SQLite (boolean not really...).
    /// </summary>
    public enum SqlDataType
    {
        Text,
        Boolean,
        Integer,
        Double
    }

    public static class SqlDataTypeExtensions
    {
        public static object ConvertValue(this SqlDataType sqlDataType, IGH_Goo dataItem)
        {
            object convertedValue = null;
            switch (sqlDataType)
            {
                case SqlDataType.Text:
                    if (dataItem.CastTo(out string strValue))
                        convertedValue = strValue;
                    else
                        convertedValue = dataItem.ToString();
                    break;
                case SqlDataType.Boolean:
                    if (dataItem.CastTo(out bool boolValue))
                        convertedValue = boolValue;
                    else if (bool.TryParse(dataItem.ToString(), out boolValue))
                        convertedValue = boolValue;
                    else
                        convertedValue = false;
                    break;
                case SqlDataType.Integer:
                    if (dataItem.CastTo(out int intValue))
                        convertedValue = intValue;
                    else if (int.TryParse(dataItem.ToString(), out intValue))
                        convertedValue = intValue;
                    else
                        convertedValue = 0;
                    break;
                case SqlDataType.Double:
                    if (dataItem.CastTo(out double doubleValue))
                        convertedValue = doubleValue;
                    else if (double.TryParse(dataItem.ToString(), out doubleValue))
                        convertedValue = doubleValue;
                    else
                        convertedValue = 0.0;
                    break;
            }
            return convertedValue;
        }

        public static Type GetDataType(this SqlDataType sqlDataType)
        {
            switch (sqlDataType)
            {
                case SqlDataType.Text:
                    return typeof(string);
                case SqlDataType.Boolean:
                    return typeof(bool);
                case SqlDataType.Integer:
                    return typeof(int);
                case SqlDataType.Double:
                    return typeof(double);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public static bool TryParseFromNaturalLanguage(string input, out SqlDataType sqlDataType)
        {
            sqlDataType = default;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim().ToLower();

            switch (input)
            {
                case "text":
                case "string":
                    sqlDataType = SqlDataType.Text;
                    return true;
                case "bool":
                case "boolean":
                    sqlDataType = SqlDataType.Boolean;
                    return true;
                case "int":
                case "integer":
                    sqlDataType = SqlDataType.Integer;
                    return true;
                case "number":
                case "double":
                case "float":
                case "real":
                case "decimal":
                    sqlDataType = SqlDataType.Double;
                    return true;
                default:
                    return false;
            }
        }

        public static string ToSqliteKeyword(this SqlDataType sqlDataType)
        {
            switch (sqlDataType)
            {
                case SqlDataType.Text:
                    return "TEXT";
                case SqlDataType.Boolean:
                    return "BOOLEAN";
                case SqlDataType.Integer:
                    return "INTEGER";
                case SqlDataType.Double:
                    return "REAL";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}
