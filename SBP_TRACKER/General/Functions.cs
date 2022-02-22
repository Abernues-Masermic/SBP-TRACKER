using System;
using System.Linq;


namespace SBP_TRACKER
{
    public static class Functions
    {
        public static string Read_value(int[] read_values, int start_pos, TypeCode var_Type)
        {
            string s_value = string.Empty;

            switch (var_Type)
            {
                case TypeCode.Int16:

                    byte[] array_data= BitConverter.GetBytes(read_values[start_pos]);
                    Int16 i16_data = BitConverter.ToInt16(array_data, 0);
                    s_value = i16_data.ToString();

                    break;

                case TypeCode.Int32:
                    byte[] array_data_1 = BitConverter.GetBytes(read_values[start_pos]);
                    byte[] array_data_2 = BitConverter.GetBytes(read_values[start_pos + 1]);

                    array_data = array_data_1.SubArray(0,2).Concat(array_data_2.SubArray(0,2)).ToArray();
                    Int32 i32_data = BitConverter.ToInt32(array_data, 0);
                    s_value = i32_data.ToString();

                    break;

                case TypeCode.UInt16:
                    array_data = BitConverter.GetBytes(read_values[start_pos]);
                    UInt16 ui16_data = BitConverter.ToUInt16(array_data, 0);
                    s_value = ui16_data.ToString();

                    break;

                case TypeCode.UInt32:
                    array_data_1 = BitConverter.GetBytes(read_values[start_pos]);
                    array_data_2 = BitConverter.GetBytes(read_values[start_pos + 1]);

                    array_data = array_data_1.SubArray(0, 2).Concat(array_data_2.SubArray(0, 2)).ToArray();
                    UInt32 ui32_data = BitConverter.ToUInt32(array_data, 0);
                    s_value = ui32_data.ToString();

                    break;

                case TypeCode.Single:
                    array_data_1 = BitConverter.GetBytes(read_values[start_pos]);
                    array_data_2 = BitConverter.GetBytes(read_values[start_pos + 1]);

                    array_data = array_data_1.SubArray(0, 2).Concat(array_data_2.SubArray(0, 2)).ToArray();
                    Single single_data = BitConverter.ToSingle(array_data, 0);
                    s_value = single_data.ToString();

                    break;

                case TypeCode.Byte:

       
                    break;

                case TypeCode.DateTime:

          
                    break;

                case TypeCode.String:

        
                    break;
            }



            return s_value;
        }


        public static T[] SubArray<T>(this T[] array, int offset, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }

    }
}
