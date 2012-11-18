using System;

namespace OffregLib
{
    public abstract class OffregBase : IDisposable
    {
        protected IntPtr _intPtr;

        public abstract void Close();

        public abstract void Dispose();
    }
}