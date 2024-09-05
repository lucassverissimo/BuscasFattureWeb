using System.ComponentModel;
using System.Reflection;

namespace FattureWebAuxiliar
{
    public static class EnumExtensions
    {
        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            if (field != null)
            {
                DescriptionAttribute attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

                if (attribute != null)
                {
                    return attribute.Description;
                }
            }

            // Caso não haja uma descrição, retorna o nome do enum
            return value.ToString();
        }
    }
}
