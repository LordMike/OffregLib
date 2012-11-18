using System;
using System.ComponentModel;
using System.IO;

namespace OffregLib
{
    public class OffregHive : OffregBase
    {
        public OffregKey Root { get; private set; }

        internal OffregHive(IntPtr hivePtr)
        {
            _intPtr = hivePtr;

            // Represent this as a key also
            Root = new OffregKey(null, _intPtr, null);
        }

        public void SaveHive(string targetFile, uint majorVersionTarget, uint minorVersionTarget)
        {
            Win32Result res = OffregNative.SaveHive(_intPtr, targetFile, majorVersionTarget, minorVersionTarget);

            if (res != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int) res);
        }

        public static OffregHive Create()
        {
            IntPtr newHive;
            Win32Result res = OffregNative.CreateHive(out newHive);

            if (res != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int) res);

            return new OffregHive(newHive);
        }
        public static OffregHive Open(string hiveFile)
        {
            IntPtr existingHive;
            Win32Result res = OffregNative.OpenHive(hiveFile, out existingHive);

            if (res != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)res);

            return new OffregHive(existingHive);
        }

        public override void Close()
        {
            if (_intPtr != IntPtr.Zero)
            {
                Win32Result res = OffregNative.CloseHive(_intPtr);

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