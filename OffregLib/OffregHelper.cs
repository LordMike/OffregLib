using System;
using System.Text;

namespace OffregLib
{
    internal static class OffregHelper
    {
        /// <summary>
        /// Represents the encoding used by Windows Registry. 
        /// UTF-16 is valid for Windows 2000 and forward.
        /// </summary>
        public static Encoding StringEncoding
        {
            get { return Encoding.Unicode; }
        }

        /// <summary>
        /// Converts some binary data into the form used in the CLR. 
        /// </summary>
        /// <param name="type">The type of data to convert from.</param>
        /// <param name="data">The data to convert.</param>
        /// <returns>A CLR object.</returns>
        public static object ConvertValueDataToObject(RegValueType type, byte[] data)
        {
            switch (type)
            {
                case RegValueType.REG_NONE:
                    return data;
                case RegValueType.REG_LINK: // This is a unicode string
                case RegValueType.REG_EXPAND_SZ: // This is a unicode string
                case RegValueType.REG_SZ:
                    string s1 = StringEncoding.GetString(data);
                    if (s1.EndsWith("\0"))
                        s1 = s1.Remove(s1.Length - 1);
                    return s1;
                case RegValueType.REG_BINARY:
                    return data;
                case RegValueType.REG_DWORD:
                    return BitConverter.ToInt32(data, 0);
                case RegValueType.REG_DWORD_BIG_ENDIAN:
                    Array.Reverse(data);
                    return BitConverter.ToInt32(data, 0);
                case RegValueType.REG_MULTI_SZ:
                    // Get string without the ending null
                    if (data.Length <= 1)
                        // Weird behaviour. Shouldn't be possible
                        return new string[0];

                    string s2 = StringEncoding.GetString(data, 0, data.Length - 2);
                    return s2.Split(new[] {'\0'}, StringSplitOptions.RemoveEmptyEntries);
                case RegValueType.REG_RESOURCE_LIST:
                    throw new NotSupportedException("REG_RESOURCE_LIST are not supported");
                case RegValueType.REG_FULL_RESOURCE_DESCRIPTOR:
                    throw new NotSupportedException("REG_FULL_RESOURCE_DESCRIPTOR are not supported");
                case RegValueType.REG_RESOURCE_REQUIREMENTS_LIST:
                    throw new NotSupportedException("REG_RESOURCE_REQUIREMENTS_LIST are not supported");
                case RegValueType.REG_QWORD:
                    return BitConverter.ToInt64(data, 0);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}