using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace OffregLib
{
    public class SubKeyContainer
    {
        public string Name { get; set; }
        public string Class { get; set; }
        public FILETIME LastWriteTime { get; set; }
    }

    public class ValueContainer
    {
        public string Name { get; set; }
        public object Data { get; set; }
        public RegValueType Type { get; set; }
    }

    /// <summary>
    /// Represents a key in the offline registry. Remember to close it (wrap it in usings).
    /// </summary>
    public class OffregKey : OffregBase
    {
        /// <summary>
        /// The name of the key.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Best-effort full path of the key.
        /// </summary>
        public string FullName { get; protected set; }

        /// <summary>
        /// The parent key.
        /// </summary>
        private OffregKey _parent;

        /// <summary>
        /// Gets the number of subkeys under this key.
        /// </summary>
        public int SubkeyCount
        {
            get { return (int) _metadata.SubKeysCount; }
        }

        /// <summary>
        /// Gets the number of values under this key.
        /// </summary>
        public int ValueCount
        {
            get { return (int) _metadata.ValuesCount; }
        }

        /// <summary>
        /// Indicates if we should close the handle when <see cref="Close"/> is called.
        /// </summary>
        private bool _ownsPointer = true;

        /// <summary>
        /// Internal metadata from QueryInfoKey
        /// </summary>
        private QueryInfoKeyData _metadata;

        /// <summary>
        /// Constructor, uses an already-open pointer as a key.
        /// </summary>
        /// <param name="parent">The parent key.</param>
        /// <param name="ptr">Handle to an open key.</param>
        /// <param name="name">The name of the open key.</param>
        internal OffregKey(OffregKey parent, IntPtr ptr, string name)
        {
            _intPtr = ptr;

            Name = name;
            FullName = (parent == null || parent.FullName == null ? "" : parent.FullName + "\\") + name;
            _parent = parent;

            _metadata = new QueryInfoKeyData();
            RefreshMetadata();
        }

        /// <summary>
        /// Constructor, opens a subkey.
        /// </summary>
        /// <param name="parentKey">The parent key.</param>
        /// <param name="name">The name of the subkey to open.</param>
        internal OffregKey(OffregKey parentKey, string name)
        {
            Win32Result result = OffregNative.OpenKey(parentKey._intPtr, name, out _intPtr);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int) result);

            Name = name;
            FullName = (parentKey.FullName == null ? "" : parentKey.FullName + "\\") + name;
            _parent = parentKey;

            _metadata = new QueryInfoKeyData();
            RefreshMetadata();
        }

        /// <summary>
        /// Calls QueryInfoKey and updates _metadata.
        /// </summary>
        private void RefreshMetadata()
        {
            uint sizeClass = 0;
            uint countSubKeys = 0, maxSubKeyLen = 0, maxClassLen = 0;
            uint countValues = 0, maxValueNameLen = 0, maxValueLen = 0;
            uint securityDescriptorSize = 0;
            FILETIME lastWrite = new FILETIME();

            // Get size of class
            Win32Result result = OffregNative.QueryInfoKey(_intPtr, null, ref sizeClass, ref countSubKeys,
                                                           ref maxSubKeyLen, ref maxClassLen,
                                                           ref countValues, ref maxValueNameLen, ref maxValueLen,
                                                           ref securityDescriptorSize,
                                                           ref lastWrite);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int) result);

            // The returned size does is in characters (unicode), excluding NULL chars. Increment it to have space
            sizeClass = sizeClass*2 + 1;

            // Allocate
            StringBuilder sbClass = new StringBuilder((int) sizeClass);

            result = OffregNative.QueryInfoKey(_intPtr, sbClass, ref sizeClass, ref countSubKeys, ref maxSubKeyLen,
                                               ref maxClassLen,
                                               ref countValues, ref maxValueNameLen, ref maxValueLen,
                                               ref securityDescriptorSize,
                                               ref lastWrite);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int) result);

            _metadata.Class = sbClass.ToString();
            _metadata.LastWriteTime = lastWrite;

            _metadata.SubKeysCount = countSubKeys;
            _metadata.MaxSubKeyLen = maxSubKeyLen*2 + 1; // Unicode chars, no null terminator.
            _metadata.MaxClassLen = maxClassLen*2 + 1; // Unicode chars, no null terminator.
            _metadata.ValuesCount = countValues;
            _metadata.MaxValueNameLen = maxValueNameLen*2 + 1; // Unicode chars, no null terminator.
            _metadata.MaxValueLen = maxValueLen; // Bytes
            _metadata.SizeSecurityDescriptor = securityDescriptorSize;
        }

        /// <summary>
        /// Enumerates all subkeys, retrieving both their name and class at the same time.
        /// </summary>
        /// <returns>Names and classes of all the subkeys.</returns>
        public SubKeyContainer[] EnumerateSubKeys()
        {
            SubKeyContainer[] results = new SubKeyContainer[_metadata.SubKeysCount];

            for (uint item = 0; item < _metadata.SubKeysCount; item++)
            {
                uint sizeName = _metadata.MaxSubKeyLen;
                uint sizeClass = _metadata.MaxClassLen;

                StringBuilder sbName = new StringBuilder((int) sizeName);
                StringBuilder sbClass = new StringBuilder((int) sizeClass);
                FILETIME fileTime = new FILETIME();

                Win32Result result = OffregNative.EnumKey(_intPtr, item, sbName, ref sizeName, sbClass, ref sizeClass,
                                                          ref fileTime);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int) result);

                SubKeyContainer container = new SubKeyContainer();

                container.Name = sbName.ToString();
                container.Class = sbClass.ToString();
                container.LastWriteTime = fileTime;

                results[item] = container;
            }

            return results;
        }

        /// <summary>
        /// Enumerates all subkeys, only retrieving their names.
        /// </summary>
        /// <returns>Names of all the subkeys.</returns>
        public string[] GetSubKeyNames()
        {
            string[] results = new string[_metadata.SubKeysCount];

            for (uint item = 0; item < _metadata.SubKeysCount; item++)
            {
                uint sizeName = _metadata.MaxSubKeyLen;

                StringBuilder sbName = new StringBuilder((int) sizeName);
                Win32Result result = OffregNative.EnumKey(_intPtr, item, sbName, ref sizeName, null, IntPtr.Zero,
                                                          IntPtr.Zero);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int) result);

                results[item] = sbName.ToString();
            }

            return results;
        }

        /// <summary>
        /// Opens a subkey. If you'd like to create it if it doesn't exist, see <see cref="CreateSubKey"/>.
        /// </summary>
        /// <param name="name">Name of the subkey to open.</param>
        /// <returns>The opened subkey.</returns>
        public OffregKey OpenSubKey(string name)
        {
            return new OffregKey(this, name);
        }

        /// <summary>
        /// Creates a new subkey (or opens an existing one).
        /// </summary>
        /// <param name="name">The name of the subkey to create (or open).</param>
        /// <param name="options">Key creation options.</param>
        /// <returns>The newly created (or opened) key.</returns>
        public OffregKey CreateSubKey(string name, RegOption options = 0)
        {
            IntPtr newKeyPtr;
            KeyDisposition disposition;
            Win32Result result = OffregNative.CreateKey(_intPtr, name, null, options, IntPtr.Zero, out newKeyPtr,
                                                        out disposition);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int) result);

            // Return new key
            OffregKey newKey = new OffregKey(this, newKeyPtr, name);

            RefreshMetadata();

            return newKey;
        }

        /// <summary>
        /// Deletes this key, further operations will be invalid (except calls to <see cref="Close"/>).
        /// </summary>
        public void Delete()
        {
            if (_parent == null)
                throw new InvalidOperationException("Cannot delete the root key");

            Win32Result result = OffregNative.DeleteKey(_intPtr, null);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int) result);

            // Refresh parent
            _parent.RefreshMetadata();
        }

        /// <summary>
        /// Deletes a subkey of this key. The subkey must not contain any subkeys of its own, to delete recursively - see <see cref="DeleteSubKeyTree"/>.
        /// </summary>
        /// <param name="name">The name of the subkey to delete</param>
        public void DeleteSubKey(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Win32Result result = OffregNative.DeleteKey(_intPtr, name);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int) result);

            RefreshMetadata();
        }

        /// <summary>
        /// Recursively delete a subkey and all its subkeys.
        /// </summary>
        /// <param name="name">Name of the subkey to delete.</param>
        public void DeleteSubKeyTree(string name)
        {
            // Open key
            using (OffregKey subKey = OpenSubKey(name))
            {
                DeleteSubKeyTree(subKey);
            }

            // Refresh
            RefreshMetadata();
        }

        /// <summary>
        /// Internal recursive function.
        /// </summary>
        /// <param name="key"></param>
        private static void DeleteSubKeyTree(OffregKey key)
        {
            // Get childs
            string[] childs = key.GetSubKeyNames();

            // Delete all those childs
            foreach (string child in childs)
            {
                try
                {
                    using (OffregKey childKey = key.OpenSubKey(child))
                        DeleteSubKeyTree(childKey);
                }
                catch (Win32Exception ex)
                {
                    switch (ex.NativeErrorCode)
                    {
                        case (int) Win32Result.ERROR_FILE_NOT_FOUND:
                            // Child didn't exist
                            break;
                        default:
                            throw;
                    }
                }
            }

            // Delete self
            key.Delete();
        }

        /// <summary>
        /// Enumerates all vaues, only retrieving their names.
        /// </summary>
        /// <returns>Names of all the values.</returns>
        public string[] GetValueNames()
        {
            string[] results = new string[_metadata.ValuesCount];

            for (uint item = 0; item < _metadata.ValuesCount; item++)
            {
                uint sizeName = _metadata.MaxValueNameLen;

                StringBuilder sbName = new StringBuilder((int) sizeName);
                Win32Result result = OffregNative.EnumValue(_intPtr, item, sbName, ref sizeName, IntPtr.Zero, IntPtr.Zero,
                                                            IntPtr.Zero);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int) result);

                results[item] = sbName.ToString();
            }

            return results;
        }

        /// <summary>
        /// Enumerates all values, retrieving both their name, data and type at the same time.
        /// </summary>
        /// <returns>Names, datas and types of all the values.</returns>
        public ValueContainer[] EnumerateValues()
        {
            ValueContainer[] results = new ValueContainer[_metadata.ValuesCount];

            // Allocate data buffer
            IntPtr dataPtr = IntPtr.Zero;
            try
            {
                dataPtr = Marshal.AllocHGlobal((int) _metadata.MaxValueLen);

                // Iterate all values
                for (uint item = 0; item < _metadata.ValuesCount; item++)
                {
                    uint sizeName = _metadata.MaxValueNameLen;
                    uint sizeData = _metadata.MaxValueLen;

                    StringBuilder sbName = new StringBuilder((int) sizeName);
                    RegValueType type;

                    // Get item
                    var result = OffregNative.EnumValue(_intPtr, item, sbName, ref sizeName, out type, dataPtr,
                                                        ref sizeData);

                    if (result != Win32Result.ERROR_SUCCESS)
                        throw new Win32Exception((int) result);

                    Debug.WriteLine("Read " + sbName + " to " + sizeData + " bytes");

                    byte[] data = new byte[sizeData];
                    Marshal.Copy(dataPtr, data, 0, (int) sizeData);

                    ValueContainer container = new ValueContainer();

                    container.Name = sbName.ToString();
                    container.Data = OffregHelper.ConvertValueDataToObject(type, data);
                    container.Type = type;

                    results[item] = container;
                }
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
            }

            return results;
        }

        /// <summary>
        /// Gets the type of a single value.
        /// </summary>
        /// <param name="name">The name of the value to retrieve the type of.</param>
        /// <returns>The type of the value.</returns>
        public RegValueType GetValueKind(string name)
        {
            RegValueType type;

            Win32Result result = OffregNative.GetValue(_intPtr, null, name, out type, IntPtr.Zero, IntPtr.Zero);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int) result);

            return type;
        }

        /// <summary>
        /// Gets the data of a specific value.
        /// </summary>
        /// <param name="name">The name of the value to retrieve the data of.</param>
        /// <returns>The data for the value.</returns>
        public object GetValue(string name)
        {
            Tuple<RegValueType, byte[]> data = GetValueInternal(name);

            return OffregHelper.ConvertValueDataToObject(data.Item1, data.Item2);
        }

        /// <summary>
        /// Sets a value to the REG_SZ type.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The data for the value.</param>
        /// <param name="type">The optional type for the value.</param>
        public void SetValue(string name, string value, RegValueType type = RegValueType.REG_SZ)
        {
            // Always leave a trailing null-terminator
            byte[] data = new byte[OffregHelper.StringEncoding.GetByteCount(value) + 2];
            OffregHelper.StringEncoding.GetBytes(value, 0, value.Length, data, 0);

            SetValue(name, type, data);
        }

        /// <summary>
        /// Sets a value to the REG_MULTI_SZ type.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="values">The data for the value.</param>
        /// <param name="type">The optional type for the value.</param>
        public void SetValue(string name, string[] values, RegValueType type = RegValueType.REG_MULTI_SZ)
        {
            if (values.Any(string.IsNullOrEmpty))
                throw new ArgumentException("No empty strings allowed");

            // A null char for each string, plus a null char at the end
            int bytes = values.Select(s => OffregHelper.StringEncoding.GetByteCount(s) + 2).Sum() + 2;
            byte[] data = new byte[bytes];

            int position = 0;
            for (int i = 0; i < values.Length; i++)
            {
                // Save and increment position
                position += OffregHelper.StringEncoding.GetBytes(values[i], 0, values[i].Length, data, position) + 2;
            }

            SetValue(name, type, data);
        }

        /// <summary>
        /// Sets a value to the REG_BINARY type.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The data for the value.</param>
        /// <param name="type">The optional type for the value.</param>
        public void SetValue(string name, byte[] value, RegValueType type = RegValueType.REG_BINARY)
        {
            SetValue(name, type, value);
        }

        /// <summary>
        /// Sets a value to the REG_DWORD type.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The data for the value.</param>
        /// <param name="type">The optional type for the value.</param>
        public void SetValue(string name, int value, RegValueType type = RegValueType.REG_DWORD)
        {
            byte[] data = BitConverter.GetBytes(value);

            if (type == RegValueType.REG_DWORD_BIG_ENDIAN)
                // Reverse it
                Array.Reverse(data);

            SetValue(name, type, data);
        }

        /// <summary>
        /// Sets a value to the REG_QWORD type.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The data for the value.</param>
        /// <param name="type">The optional type for the value.</param>
        public void SetValue(string name, long value, RegValueType type = RegValueType.REG_QWORD)
        {
            byte[] data = BitConverter.GetBytes(value);

            SetValue(name, type, data);
        }

        /// <summary>
        /// Sets a value to the specified type.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="type">The optional type for the value.</param>
        /// <param name="data">The data for the value.</param>
        private void SetValue(string name, RegValueType type, byte[] data)
        {
            IntPtr dataPtr = IntPtr.Zero;
            try
            {
                dataPtr = Marshal.AllocHGlobal(data.Length);
                Marshal.Copy(data, 0, dataPtr, data.Length);

                Debug.WriteLine("Setting " + name + " to " + data.Length + " bytes");

                Win32Result result = OffregNative.SetValue(_intPtr, name, type, dataPtr, (uint) data.Length);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int) result);
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
            }

            RefreshMetadata();
        }

        /// <summary>
        /// Deletes a specified value.
        /// </summary>
        /// <param name="name">The name of the value to delete.</param>
        public void DeleteValue(string name)
        {
            Win32Result result = OffregNative.DeleteValue(_intPtr, name);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int) result);

            RefreshMetadata();
        }

        /// <summary>
        /// Internal helper to get the type and data for a specified value.
        /// </summary>
        /// <param name="name">The name of the value to retrieve data for.</param>
        /// <returns>The type and data for the specified value.</returns>
        internal Tuple<RegValueType, byte[]> GetValueInternal(string name)
        {
            RegValueType type;

            // Get the size first
            uint size = 0;
            Win32Result result = OffregNative.GetValue(_intPtr, null, name, out type, IntPtr.Zero, ref size);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int) result);

            // Allocate buffer
            byte[] res = new byte[size];
            IntPtr dataPtr = IntPtr.Zero;
            try
            {
                dataPtr = Marshal.AllocHGlobal((int) size);

                // Get data
                result = OffregNative.GetValue(_intPtr, null, name, out type, dataPtr, ref size);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int) result);

                // Copy data
                Marshal.Copy(dataPtr, res, 0, (int) size);
            }
            finally
            {
                // Release data
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
            }

            return new Tuple<RegValueType, byte[]>(type, res);
        }

        public override void Close()
        {
            if (_intPtr != IntPtr.Zero && _ownsPointer)
            {
                Win32Result res = OffregNative.CloseKey(_intPtr);

                if (res != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int) res);
            }
        }

        public override void Dispose()
        {
            Close();
        }
    }
}