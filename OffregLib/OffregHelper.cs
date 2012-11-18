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
        /// <param name="parsedData">The parsed CLR object.</param>
        /// <returns>Indication if the parsed data could be parsed correctly as the specified type. If false, data is always a byte[].</returns>
        public static bool TryConvertValueDataToObject(RegValueType type, byte[] data, out object parsedData)
        {
            parsedData = data;

            switch (type)
            {
                case RegValueType.REG_NONE:
                    // NONE format shouldn't be specified, ever.
                    return false;
                case RegValueType.REG_LINK: // This is a unicode string
                case RegValueType.REG_EXPAND_SZ: // This is a unicode string
                case RegValueType.REG_SZ:
                    if (data.Length % 2 != 0)
                        // UTF-16 strings are always an even number of bytes
                        return false;

                    string s1 = StringEncoding.GetString(data);
                    if (s1.EndsWith("\0"))
                        s1 = s1.Remove(s1.Length - 1);

                    parsedData = s1;
                    return true;
                case RegValueType.REG_BINARY:
                    return true;
                case RegValueType.REG_DWORD:
                    if (data.Length != 4)
                        return false;

                    parsedData = BitConverter.ToInt32(data, 0);
                    return true;
                case RegValueType.REG_DWORD_BIG_ENDIAN:
                    if (data.Length != 4)
                        return false;

                    Array.Reverse(data);
                    parsedData = BitConverter.ToInt32(data, 0);
                    return true;
                case RegValueType.REG_MULTI_SZ:
                    // Get string without the ending null
                    if (data.Length % 2 != 0)
                        // Invalid string data, must always be even in length
                        return false;

                    if (data.Length == 0)
                        // A badly formatted list
                        return false;

                    if (data.Length == 2 && data[0] == 0 && data[1] == 0)
                    {
                        // An empty list is identified by: \0
                        parsedData = new string[0];
                        return true;
                    }

                    if (data[data.Length - 4] != 0 || data[data.Length - 3] != 0 || data[data.Length - 2] != 0 || data[data.Length - 1] != 0)
                        // Must always end with four nulls
                        return false;

                    string s2 = StringEncoding.GetString(data, 0, data.Length - 4);
                    parsedData = s2.Split(new[] { '\0' });
                    return true;
                case RegValueType.REG_RESOURCE_LIST:
                    throw new NotSupportedException("REG_RESOURCE_LIST are not supported");
                case RegValueType.REG_FULL_RESOURCE_DESCRIPTOR:
                    throw new NotSupportedException("REG_FULL_RESOURCE_DESCRIPTOR are not supported");
                case RegValueType.REG_RESOURCE_REQUIREMENTS_LIST:
                    throw new NotSupportedException("REG_RESOURCE_REQUIREMENTS_LIST are not supported");
                case RegValueType.REG_QWORD:
                    if (data.Length != 8)
                        return false;

                    parsedData = BitConverter.ToInt64(data, 0);
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}