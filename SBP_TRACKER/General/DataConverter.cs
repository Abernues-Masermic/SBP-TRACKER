using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBP_TRACKER
{
    internal class DataConverter
    {

        #region Type code to String type code

        public static string Type_code_to_string(TypeCode code)
        {
            String s_type = String.Empty;

            switch (code)
            {
                case TypeCode.Int16:

                    s_type = "signed 16";
                    break;

                case TypeCode.Int32:

                    s_type = "signed 32";
                    break;

                case TypeCode.UInt16:

                    s_type = "unsigned 16";
                    break;

                case TypeCode.UInt32:

                    s_type = "unsigned 32";
                    break;

                case TypeCode.Single:

                    s_type = "float";
                    break;

                case TypeCode.Byte:

                    s_type = "byte";
                    break;

                case TypeCode.DateTime:

                    s_type = "datetime";
                    break;

                case TypeCode.String:

                    s_type = "string";
                    break;
            }

            return s_type;
        }


        public static TypeCode String_to_type_code(string s_code)
        {
            TypeCode type_code = TypeCode.Empty;

            switch (s_code)
            {
                case "signed 16":

                    type_code = TypeCode.Int16;
                    break;

                case "signed 32":
                    type_code = TypeCode.Int32;
                    break;

                case "unsigned 16":
                    type_code = TypeCode.UInt16;
                    break;

                case "unsigned 32":
                    type_code = TypeCode.UInt32;
                    break;

                case "float":
                    type_code = TypeCode.Single;
                    break;

                case "byte":
                    type_code = TypeCode.Byte;
                    break;

                case "datetime":
                    type_code = TypeCode.DateTime;
                    break;

                case "string":
                    type_code = TypeCode.String;
                    break;
            }

            return type_code;
        }


        #endregion


        #region Byte array to hex

        public static String ByteArrayToHex(byte[] array_byte)
        {
            StringBuilder builder = new StringBuilder(array_byte.Length * 3);
            foreach (byte data in array_byte) { builder.Append(Convert.ToString(data, 16).PadLeft(2, '0')); }

            //Nº de caracteres impar--> Añadir un 0
            String hex_value = builder.ToString().ToUpper();
            if (hex_value.Length % 2 != 0)
                hex_value = "0" + hex_value;

            return hex_value;
        }

        #endregion

    }
}
