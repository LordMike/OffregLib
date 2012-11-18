using System;
using System.Runtime.InteropServices;
using System.Text;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace OffregLib
{
    public enum RegValueType : uint
    {
        REG_NONE = 0,
        REG_SZ = 1,
        REG_EXPAND_SZ = 2,
        REG_BINARY = 3,
        REG_DWORD = 4,
        REG_DWORD_LITTLE_ENDIAN = 4,
        REG_DWORD_BIG_ENDIAN = 5,
        REG_LINK = 6,
        REG_MULTI_SZ = 7,
        REG_RESOURCE_LIST = 8,
        REG_FULL_RESOURCE_DESCRIPTOR = 9,
        REG_RESOURCE_REQUIREMENTS_LIST = 10,
        REG_QWORD = 11,
        REG_QWORD_LITTLE_ENDIAN = 11
    }

    public enum RegPredefinedKeys : int
    {
        HKEY_CLASSES_ROOT = unchecked((int) 0x80000000),
        HKEY_CURRENT_USER = unchecked((int) 0x80000001),
        HKEY_LOCAL_MACHINE = unchecked((int) 0x80000002),
        HKEY_USERS = unchecked((int) 0x80000003),
        HKEY_PERFORMANCE_DATA = unchecked((int) 0x80000004),
        HKEY_CURRENT_CONFIG = unchecked((int) 0x80000005),
        HKEY_DYN_DATA = unchecked((int) 0x80000006),
        HKEY_CURRENT_USER_LOCAL_SETTINGS = unchecked((int) 0x80000007)
    }

    public enum KeyDisposition : long
    {
        REG_CREATED_NEW_KEY = 0x00000001,
        REG_OPENED_EXISTING_KEY = 0x00000002
    }

    public enum KeySecurity : int
    {
        KEY_QUERY_VALUE = 0x0001,
        KEY_SET_VALUE = 0x0002,
        KEY_ENUMERATE_SUB_KEYS = 0x0008,
        KEY_NOTIFY = 0x0010,
        DELETE = 0x10000,
        STANDARD_RIGHTS_READ = 0x20000,
        KEY_READ = 0x20019,
        KEY_WRITE = 0x20006,
        KEY_ALL_ACCESS = 0xF003F,
        MAXIMUM_ALLOWED = 0x2000000
    }

    [Flags]
    public enum RegOption : uint
    {
        REG_OPTION_RESERVED = 0x00000000,
        REG_OPTION_NON_VOLATILE = 0x00000000,
        REG_OPTION_VOLATILE = 0x00000001,
        REG_OPTION_CREATE_LINK = 0x00000002,
        REG_OPTION_BACKUP_RESTORE = 0x00000004,
        REG_OPTION_OPEN_LINK = 0x00000008
    }

    public enum SECURITY_INFORMATION : uint
    {
        OWNER_SECURITY_INFORMATION = 0x00000001,
        GROUP_SECURITY_INFORMATION = 0x00000002,
        DACL_SECURITY_INFORMATION = 0x00000004,
        SACL_SECURITY_INFORMATION = 0x00000008,
        LABEL_SECURITY_INFORMATION = 0x00000010,
        PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000,
        PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
        UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
        UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000,
    }

    /// <summary>
    /// All the functions can be read about here:
    ///   http://msdn.microsoft.com/en-us/library/ee210756(v=vs.85).aspx
    /// </summary>
    public static class OffregNative
    {
        private const string OffRegDllName32 = "offreg.x86.dll";
        private const string OffRegDllName64 = "offreg.x64.dll";

        [DllImport(OffRegDllName32, EntryPoint = "ORCreateHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result CreateHive32(out IntPtr rootKeyHandle);

        [DllImport(OffRegDllName64, EntryPoint = "ORCreateHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result CreateHive64(out IntPtr rootKeyHandle);

        public static Win32Result CreateHive(out IntPtr rootKeyHandle)
        {
            return Environment.Is64BitProcess ? CreateHive64(out rootKeyHandle) : CreateHive32(out rootKeyHandle);
        }

        [DllImport(OffRegDllName32, EntryPoint = "OROpenHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result OpenHive32(string path, out IntPtr rootKeyHandle);

        [DllImport(OffRegDllName64, EntryPoint = "OROpenHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result OpenHive64(string path, out IntPtr rootKeyHandle);

        public static Win32Result OpenHive(string path, out IntPtr rootKeyHandle)
        {
            return Environment.Is64BitProcess
                       ? OpenHive64(path, out rootKeyHandle)
                       : OpenHive32(path, out rootKeyHandle);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORCloseHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result CloseHive32(IntPtr rootKeyHandle);

        [DllImport(OffRegDllName64, EntryPoint = "ORCloseHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result CloseHive64(IntPtr rootKeyHandle);

        public static Win32Result CloseHive(IntPtr rootKeyHandle)
        {
            return Environment.Is64BitProcess ? CloseHive64(rootKeyHandle) : CloseHive32(rootKeyHandle);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORSaveHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result SaveHive32(
            IntPtr rootKeyHandle,
            string path,
            uint dwOsMajorVersion,
            uint dwOsMinorVersion);

        [DllImport(OffRegDllName64, EntryPoint = "ORSaveHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result SaveHive64(
            IntPtr rootKeyHandle,
            string path,
            uint dwOsMajorVersion,
            uint dwOsMinorVersion);

        public static Win32Result SaveHive(IntPtr rootKeyHandle,
                                           string path,
                                           uint dwOsMajorVersion,
                                           uint dwOsMinorVersion)
        {
            return Environment.Is64BitProcess
                       ? SaveHive64(rootKeyHandle, path, dwOsMajorVersion, dwOsMinorVersion)
                       : SaveHive32(rootKeyHandle, path, dwOsMajorVersion, dwOsMinorVersion);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORCloseKey")]
        private static extern Win32Result CloseKey32(IntPtr hKey);

        [DllImport(OffRegDllName64, EntryPoint = "ORCloseKey")]
        private static extern Win32Result CloseKey64(IntPtr hKey);

        public static Win32Result CloseKey(IntPtr hKey)
        {
            return Environment.Is64BitProcess ? CloseKey64(hKey) : CloseKey32(hKey);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORCreateKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result CreateKey32(
            IntPtr hKey,
            string lpSubKey,
            string lpClass,
            RegOption dwOptions,
            /*ref SECURITY_DESCRIPTOR*/ IntPtr lpSecurityDescriptor,
            /*ref IntPtr*/ out IntPtr phkResult,
            out KeyDisposition lpdwDisposition);

        [DllImport(OffRegDllName64, EntryPoint = "ORCreateKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result CreateKey64(
            IntPtr hKey,
            string lpSubKey,
            string lpClass,
            RegOption dwOptions,
            /*ref SECURITY_DESCRIPTOR*/ IntPtr lpSecurityDescriptor,
            /*ref IntPtr*/ out IntPtr phkResult,
            out KeyDisposition lpdwDisposition);

        public static Win32Result CreateKey(IntPtr hKey,
                                            string lpSubKey,
                                            string lpClass,
                                            RegOption dwOptions,
                                            /*ref SECURITY_DESCRIPTOR*/ IntPtr lpSecurityDescriptor,
                                            /*ref IntPtr*/ out IntPtr phkResult,
                                            out KeyDisposition lpdwDisposition)
        {
            return Environment.Is64BitProcess
                       ? CreateKey64(hKey, lpSubKey, lpClass, dwOptions, lpSecurityDescriptor, out phkResult,
                                     out lpdwDisposition)
                       : CreateKey32(hKey, lpSubKey, lpClass, dwOptions, lpSecurityDescriptor, out phkResult,
                                     out lpdwDisposition);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORDeleteKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result DeleteKey32(
            IntPtr hKey,
            string lpSubKey);

        [DllImport(OffRegDllName64, EntryPoint = "ORDeleteKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result DeleteKey64(
            IntPtr hKey,
            string lpSubKey);

        public static Win32Result DeleteKey(IntPtr hKey,
                                            string lpSubKey)
        {
            return Environment.Is64BitProcess ? DeleteKey64(hKey, lpSubKey) : DeleteKey32(hKey, lpSubKey);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORDeleteValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result DeleteValue32(
            IntPtr hKey,
            string lpValueName);

        [DllImport(OffRegDllName64, EntryPoint = "ORDeleteValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result DeleteValue64(
            IntPtr hKey,
            string lpValueName);

        public static Win32Result DeleteValue(IntPtr hKey,
                                              string lpValueName)
        {
            return Environment.Is64BitProcess ? DeleteValue64(hKey, lpValueName) : DeleteValue32(hKey, lpValueName);
        }

        [DllImport(OffRegDllName32, EntryPoint = "OREnumKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumKey32(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpName,
            ref uint lpcchName,
            StringBuilder lpClass,
            ref uint lpcchClass,
            ref FILETIME lpftLastWriteTime);

        [DllImport(OffRegDllName64, EntryPoint = "OREnumKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumKey64(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpName,
            ref uint lpcchName,
            StringBuilder lpClass,
            ref uint lpcchClass,
            ref FILETIME lpftLastWriteTime);

        public static Win32Result EnumKey(IntPtr hKey,
                                          uint dwIndex,
                                          StringBuilder lpName,
                                          ref uint lpcchName,
                                          StringBuilder lpClass,
                                          ref uint lpcchClass,
                                          ref FILETIME lpftLastWriteTime)
        {
            return Environment.Is64BitProcess
                       ? EnumKey64(hKey, dwIndex, lpName, ref lpcchName, lpClass, ref lpcchClass, ref lpftLastWriteTime)
                       : EnumKey32(hKey, dwIndex, lpName, ref lpcchName, lpClass, ref lpcchClass, ref lpftLastWriteTime);
        }

        [DllImport(OffRegDllName32, EntryPoint = "OREnumKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumKey32(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpName,
            ref uint lpcchName,
            StringBuilder lpClass,
            IntPtr lpcchClass,
            IntPtr lpftLastWriteTime);

        [DllImport(OffRegDllName64, EntryPoint = "OREnumKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumKey64(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpName,
            ref uint lpcchName,
            StringBuilder lpClass,
            IntPtr lpcchClass,
            IntPtr lpftLastWriteTime);

        public static Win32Result EnumKey(IntPtr hKey,
                                          uint dwIndex,
                                          StringBuilder lpName,
                                          ref uint lpcchName,
                                          StringBuilder lpClass,
                                          IntPtr lpcchClass,
                                          IntPtr lpftLastWriteTime)
        {
            return Environment.Is64BitProcess
                       ? EnumKey64(hKey, dwIndex, lpName, ref lpcchName, lpClass, lpcchClass, lpftLastWriteTime)
                       : EnumKey32(hKey, dwIndex, lpName, ref lpcchName, lpClass, lpcchClass, lpftLastWriteTime);
        }

        [DllImport(OffRegDllName32, EntryPoint = "OREnumValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumValue32(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpValueName,
            ref uint lpcchValueName,
            out RegValueType lpType,
            IntPtr lpData,
            ref uint lpcbData);

        [DllImport(OffRegDllName64, EntryPoint = "OREnumValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumValue64(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpValueName,
            ref uint lpcchValueName,
            out RegValueType lpType,
            IntPtr lpData,
            ref uint lpcbData);

        public static Win32Result EnumValue(IntPtr hKey,
                                            uint dwIndex,
                                            StringBuilder lpValueName,
                                            ref uint lpcchValueName,
                                            out RegValueType lpType,
                                            IntPtr lpData,
                                            ref uint lpcbData)
        {
            return Environment.Is64BitProcess
                       ? EnumValue64(hKey, dwIndex, lpValueName, ref lpcchValueName, out lpType, lpData, ref lpcbData)
                       : EnumValue32(hKey, dwIndex, lpValueName, ref lpcchValueName, out lpType, lpData, ref lpcbData);
        }

        [DllImport(OffRegDllName32, EntryPoint = "OREnumValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumValue32(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpValueName,
            ref uint lpcchValueName,
            IntPtr lpType,
            StringBuilder lpData,
            IntPtr lpcbData);

        [DllImport(OffRegDllName64, EntryPoint = "OREnumValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumValue64(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpValueName,
            ref uint lpcchValueName,
            IntPtr lpType,
            StringBuilder lpData,
            IntPtr lpcbData);

        public static Win32Result EnumValue(IntPtr hKey,
                                            uint dwIndex,
                                            StringBuilder lpValueName,
                                            ref uint lpcchValueName,
                                            IntPtr lpType,
                                            StringBuilder lpData,
                                            IntPtr lpcbData)
        {
            return Environment.Is64BitProcess
                       ? EnumValue64(hKey, dwIndex, lpValueName, ref lpcchValueName, lpType, lpData, lpcbData)
                       : EnumValue32(hKey, dwIndex, lpValueName, ref lpcchValueName, lpType, lpData, lpcbData);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORGetKeySecurity")]
        private static extern Win32Result GetKeySecurity32(
            IntPtr hKey,
            SECURITY_INFORMATION securityInformation,
            IntPtr pSecurityDescriptor,
            ref uint lpcbSecurityDescriptor);

        [DllImport(OffRegDllName64, EntryPoint = "ORGetKeySecurity")]
        private static extern Win32Result GetKeySecurity64(
            IntPtr hKey,
            SECURITY_INFORMATION securityInformation,
            IntPtr pSecurityDescriptor,
            ref uint lpcbSecurityDescriptor);

        public static Win32Result GetKeySecurity(IntPtr hKey,
                                                 SECURITY_INFORMATION securityInformation,
                                                 IntPtr pSecurityDescriptor,
                                                 ref uint lpcbSecurityDescriptor)
        {
            return Environment.Is64BitProcess
                       ? GetKeySecurity64(hKey, securityInformation, pSecurityDescriptor, ref lpcbSecurityDescriptor)
                       : GetKeySecurity32(hKey, securityInformation, pSecurityDescriptor, ref lpcbSecurityDescriptor);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORGetValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result GetValue32(
            IntPtr hKey,
            string lpSubKey,
            string lpValue,
            out RegValueType pdwType,
            IntPtr pvData,
            ref uint pcbData);

        [DllImport(OffRegDllName64, EntryPoint = "ORGetValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result GetValue64(
            IntPtr hKey,
            string lpSubKey,
            string lpValue,
            out RegValueType pdwType,
            IntPtr pvData,
            ref uint pcbData);

        public static Win32Result GetValue(IntPtr hKey,
                                           string lpSubKey,
                                           string lpValue,
                                           out RegValueType pdwType,
                                           IntPtr pvData,
                                           ref uint pcbData)
        {
            return Environment.Is64BitProcess
                       ? GetValue64(hKey, lpSubKey, lpValue, out pdwType, pvData, ref pcbData)
                       : GetValue32(hKey, lpSubKey, lpValue, out pdwType, pvData, ref pcbData);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORGetValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result GetValue32(
            IntPtr hKey,
            string lpSubKey,
            string lpValue,
            out RegValueType pdwType,
            IntPtr pvData,
            IntPtr pcbData);

        [DllImport(OffRegDllName64, EntryPoint = "ORGetValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result GetValue64(
            IntPtr hKey,
            string lpSubKey,
            string lpValue,
            out RegValueType pdwType,
            IntPtr pvData,
            IntPtr pcbData);

        public static Win32Result GetValue(IntPtr hKey,
                                           string lpSubKey,
                                           string lpValue,
                                           out RegValueType pdwType,
                                           IntPtr pvData,
                                           IntPtr pcbData)
        {
            return Environment.Is64BitProcess
                       ? GetValue64(hKey, lpSubKey, lpValue, out pdwType, pvData, pcbData)
                       : GetValue32(hKey, lpSubKey, lpValue, out pdwType, pvData, pcbData);
        }

        [DllImport(OffRegDllName32, EntryPoint = "OROpenKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result OpenKey32(
            IntPtr hKey,
            string lpSubKey,
            out IntPtr phkResult);

        [DllImport(OffRegDllName64, EntryPoint = "OROpenKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result OpenKey64(
            IntPtr hKey,
            string lpSubKey,
            out IntPtr phkResult);

        public static Win32Result OpenKey(IntPtr hKey,
                                          string lpSubKey,
                                          out IntPtr phkResult)
        {
            return Environment.Is64BitProcess
                       ? OpenKey64(hKey, lpSubKey, out phkResult)
                       : OpenKey32(hKey, lpSubKey, out phkResult);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORQueryInfoKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result QueryInfoKey32(
            IntPtr hKey,
            StringBuilder lpClass,
            ref uint lpcchClass,
            ref uint lpcSubKeys,
            ref uint lpcbMaxSubKeyLen,
            ref uint lpcbMaxClassLen,
            ref uint lpcValues,
            ref uint lpcbMaxValueNameLen,
            ref uint lpcbMaxValueLen,
            ref uint lpcbSecurityDescriptor,
            ref FILETIME lpftLastWriteTime);

        [DllImport(OffRegDllName64, EntryPoint = "ORQueryInfoKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result QueryInfoKey64(
            IntPtr hKey,
            StringBuilder lpClass,
            ref uint lpcchClass,
            ref uint lpcSubKeys,
            ref uint lpcbMaxSubKeyLen,
            ref uint lpcbMaxClassLen,
            ref uint lpcValues,
            ref uint lpcbMaxValueNameLen,
            ref uint lpcbMaxValueLen,
            ref uint lpcbSecurityDescriptor,
            ref FILETIME lpftLastWriteTime);

        public static Win32Result QueryInfoKey(IntPtr hKey,
                                               StringBuilder lpClass,
                                               ref uint lpcchClass,
                                               ref uint lpcSubKeys,
                                               ref uint lpcbMaxSubKeyLen,
                                               ref uint lpcbMaxClassLen,
                                               ref uint lpcValues,
                                               ref uint lpcbMaxValueNameLen,
                                               ref uint lpcbMaxValueLen,
                                               ref uint lpcbSecurityDescriptor,
                                               ref FILETIME lpftLastWriteTime)
        {
            return Environment.Is64BitProcess
                       ? QueryInfoKey64(hKey, lpClass, ref lpcchClass, ref lpcSubKeys, ref lpcbMaxSubKeyLen,
                                        ref lpcbMaxClassLen, ref lpcValues, ref lpcbMaxValueNameLen, ref lpcbMaxValueLen,
                                        ref lpcbSecurityDescriptor, ref lpftLastWriteTime)
                       : QueryInfoKey32(hKey, lpClass, ref lpcchClass, ref lpcSubKeys, ref lpcbMaxSubKeyLen,
                                        ref lpcbMaxClassLen, ref lpcValues, ref lpcbMaxValueNameLen, ref lpcbMaxValueLen,
                                        ref lpcbSecurityDescriptor, ref lpftLastWriteTime);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORSetValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result SetValue32(
            IntPtr hKey,
            string lpValueName,
            RegValueType dwType,
            IntPtr lpData,
            int cbData);

        [DllImport(OffRegDllName64, EntryPoint = "ORSetValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result SetValue64(
            IntPtr hKey,
            string lpValueName,
            RegValueType dwType,
            IntPtr lpData,
            int cbData);

        public static Win32Result SetValue(IntPtr hKey,
                                           string lpValueName,
                                           RegValueType dwType,
                                           IntPtr lpData,
                                           int cbData)
        {
            return Environment.Is64BitProcess
                       ? SetValue64(hKey, lpValueName, dwType, lpData, cbData)
                       : SetValue32(hKey, lpValueName, dwType, lpData, cbData);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORSetKeySecurity")]
        private static extern Win32Result SetKeySecurity32(
            IntPtr hKey,
            SECURITY_INFORMATION securityInformation,
            /*ref IntPtr*/ IntPtr pSecurityDescriptor);

        [DllImport(OffRegDllName64, EntryPoint = "ORSetKeySecurity")]
        private static extern Win32Result SetKeySecurity64(
            IntPtr hKey,
            SECURITY_INFORMATION securityInformation,
            /*ref IntPtr*/ IntPtr pSecurityDescriptor);

        public static Win32Result SetKeySecurity(IntPtr hKey,
                                                 SECURITY_INFORMATION securityInformation,
                                                 /*ref IntPtr*/ IntPtr pSecurityDescriptor)
        {
            return Environment.Is64BitProcess
                       ? SetKeySecurity64(hKey, securityInformation, pSecurityDescriptor)
                       : SetKeySecurity32(hKey, securityInformation, pSecurityDescriptor);
        }
    }
}