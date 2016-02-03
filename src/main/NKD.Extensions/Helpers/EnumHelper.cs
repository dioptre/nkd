using System.Reflection;   
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace NKD.Helpers
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumValueDescriptionAttribute : Attribute
    {
        public EnumValueDescriptionAttribute(string description)
        {
            Description = description;
        }

        public string Description;
    }

    public static class EnumHelper
    {
        private const char ENUM_FLAGGED_VALUE_SEPERATOR_CHARACTER = ',';

        public static string EnumToString(Enum enumValue)
        {
            StringBuilder enumValueAsString = new StringBuilder();
            Type enumType = enumValue.GetType();
            string[] enumToStringParts = enumValue.ToString().Split(ENUM_FLAGGED_VALUE_SEPERATOR_CHARACTER);
            foreach (string enumValueStringPart in enumToStringParts)
            {
                FieldInfo enumValueField = enumType.GetField(enumValueStringPart.Trim());
                EnumValueDescriptionAttribute[] enumDesc = enumValueField.GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false) as
                EnumValueDescriptionAttribute[];
                if (enumValueAsString.Length > 0)
                {
                    enumValueAsString.Append(ENUM_FLAGGED_VALUE_SEPERATOR_CHARACTER);
                }
                if (enumDesc.Length == 1)
                {
                    enumValueAsString.Append(enumDesc[0].Description);
                }
                else
                {
                    enumValueAsString.Append(enumValueStringPart);
                }

            }
            return enumValueAsString.ToString();
        }
    }
}