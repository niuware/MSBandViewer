using System;
using System.Reflection;

namespace Niuware.MSBandViewer.Helpers
{
    /// <summary>
    /// Helper functions
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Returns the name or values as a string, of all fields in the given Type
        /// </summary>
        /// <param name="type">Data type</param>
        /// <param name="separator">Separator for each value</param>
        /// <param name="parent">Parent name if there are soome sub types of the main Type</param>
        /// <param name="obj">If the values are printed in the string, we need the object</param>
        /// <returns>Full string with all name/value fields</returns>
        public static string GetFieldsAsHeaders(Type type, string separator, string parent = "", object obj = null)
        {
            FieldInfo[] fieldInfo;
            string output = "";

            fieldInfo = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo field in fieldInfo)
            {
                string value = (obj!= null) ? field.GetValue(obj).ToString() : field.Name;

                // Loop through sub types as well
                if (field.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance).Length > 0)
                {
                    if (obj != null)
                    {
                        output += GetFieldsAsHeaders(field.FieldType, separator, "", obj.GetType().GetField(field.Name).GetValue(obj));
                    }
                    else
                    {
                        output += GetFieldsAsHeaders(field.FieldType, separator, value + ".", null);
                    }
                }
                else
                {
                    output += parent + value + separator;
                }
            }

            return output;
        }
    }
}