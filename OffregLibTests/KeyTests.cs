using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OffregLib;

namespace OffregLibTests
{
    [TestClass]
    public class KeyTests
    {
        private OffregHive _hive;
        private OffregKey _key;

        [TestInitialize]
        public void Initiate()
        {
            _hive = OffregHive.Create();
            _key = _hive.Root;
        }

        private void EnsureKeyNames(params string[] expectNames)
        {
            Assert.AreEqual(expectNames.Length, _key.SubkeyCount);

            // Check actual names
            string[] actualNames = _key.GetSubKeyNames();
            Assert.AreEqual(expectNames.Length, actualNames.Length);

            // Check actual names, #2
            SubKeyContainer[] actualEnumNames = _key.EnumerateSubKeys();
            Assert.AreEqual(expectNames.Length, actualNames.Length);

            // Ensure all expected names exist
            Assert.IsTrue(expectNames.All(actualNames.Contains));
            Assert.IsTrue(expectNames.All(x => actualEnumNames.Any(s => s.Name == x)));
        }

        [TestCleanup]
        public void Destroy()
        {
            _hive.Close();
        }

        [TestMethod]
        public void KeyCreate()
        {
            _key.CreateSubKey("test").Close();
            EnsureKeyNames("test");

            _key.CreateSubKey("test2").Close();
            EnsureKeyNames("test", "test2");

            StringBuilder sb = new StringBuilder(255);
            for (int i = 0; i < 255 / 5; i++)
                sb.Append("LongN");
            string longName = sb.ToString();

            _key.CreateSubKey(longName).Close();
            EnsureKeyNames("test", "test2", longName);
        }

        [TestMethod]
        public void KeyCreateMaxLevels()
        {
            // Documentation says that we can create 512 levels deep. 
            // Testing however indicates that there is some limit at 508 levels.
            // So we test if we can create at least 500 levels.

            int i = 0;
            try
            {
                OffregKey key = _key;
                for (i = 0; i < 512; i++)
                {
                    key = key.CreateSubKey("longName");
                    Debug.WriteLine(i + " - " + key.FullName);
                }

                Assert.Fail();
            }
            catch (Win32Exception ex)
            {
                Assert.AreEqual(Win32Result.ERROR_INVALID_PARAMETER, (Win32Result)ex.NativeErrorCode);

                if (i < 500)
                    Assert.Fail("Failed before we could create 500+ levels of subkeys");
            }
        }

        [TestMethod]
        public void KeyCreateTooLongName()
        {
            try
            {
                StringBuilder sb = new StringBuilder(257);
                while (sb.Length < 257)
                    sb.Append("A");
                string longName = sb.ToString();

                _key.CreateSubKey(longName).Close();

                Assert.Fail();
            }
            catch (Win32Exception ex)
            {
                Assert.AreEqual(Win32Result.ERROR_INVALID_PARAMETER, (Win32Result)ex.NativeErrorCode);
            }
        }

        [TestMethod]
        public void KeyDelete()
        {
            _key.CreateSubKey("test").Close();
            EnsureKeyNames("test");

            _key.CreateSubKey("test2").Close();
            EnsureKeyNames("test", "test2");

            _key.DeleteSubKey("test");
            EnsureKeyNames("test2");

            _key.DeleteSubKey("test2");
            EnsureKeyNames();
        }

        [TestMethod]
        public void KeyDeleteNotExist()
        {
            try
            {
                _key.DeleteSubKey("test2");

                Assert.Fail();
            }
            catch (Win32Exception ex)
            {
                Assert.AreEqual(Win32Result.ERROR_FILE_NOT_FOUND, (Win32Result)ex.NativeErrorCode);
            }
        }

        [TestMethod]
        public void KeyDeleteSelf()
        {
            _key.CreateSubKey("test").Close();
            EnsureKeyNames("test");

            using (OffregKey key = _key.OpenSubKey("test"))
            {
                key.Delete();
            }
            EnsureKeyNames();
        }

        [TestMethod]
        public void KeyDeleteRoot()
        {
            try
            {
                _key.Delete();

                Assert.Fail();
            }
            catch (InvalidOperationException ex)
            {
                // Success
            }
        }

        [TestMethod]
        public void KeyDeleteWithSubkey()
        {
            try
            {
                using (OffregKey key = _key.CreateSubKey("test"))
                {
                    key.CreateSubKey("test2").Close();
                }
                EnsureKeyNames("test");

                _key.DeleteSubKey("test");

                Assert.Fail();
            }
            catch (Win32Exception ex)
            {
                Assert.AreEqual(Win32Result.ERROR_KEY_HAS_CHILDREN, (Win32Result)ex.NativeErrorCode);
            }
        }

        [TestMethod]
        public void KeyDeleteTree()
        {
            using (OffregKey key = _key.CreateSubKey("test"))
            {
                key.CreateSubKey("test2").Close();
                Assert.AreEqual(1, key.SubkeyCount);
            }
            EnsureKeyNames("test");

            _key.DeleteSubKeyTree("test");
            EnsureKeyNames();
        }

        [TestMethod]
        public void KeyDeleteTreeNotExists()
        {
            try
            {
                _key.DeleteSubKeyTree("test");

                Assert.Fail();
            }
            catch (Win32Exception ex)
            {
                Assert.AreEqual(Win32Result.ERROR_FILE_NOT_FOUND, (Win32Result)ex.NativeErrorCode);
            }
        }

        [TestMethod]
        public void KeyCreateExisting()
        {
            using (OffregKey subKey = _key.CreateSubKey("test"))
            {
                subKey.SetValue("val", "test");
            }

            using (OffregKey subKey = _key.CreateSubKey("test"))
            {
                Assert.AreEqual(1, subKey.ValueCount);

                object result = subKey.GetValue("val");

                Assert.IsInstanceOfType(result, typeof(string));
                Assert.AreEqual("test", result as string);
            }
        }

        [TestMethod]
        public void KeyTryOpen()
        {
            _key.CreateSubKey("test").Close();

            OffregKey tmpKey;
            bool couldOpen = _key.TryOpenSubKey("test", out tmpKey);
            tmpKey.Close();

            Assert.IsTrue(couldOpen);

            couldOpen = _key.TryOpenSubKey("test2", out tmpKey);
            if (couldOpen)
                tmpKey.Close();

            Assert.IsFalse(couldOpen);
        }

        [TestMethod]
        public void KeyOpenMulti()
        {
            // ROOT\Test\Test2\Test3
            using (OffregKey key = _key.CreateSubKey("Test"))
            using (OffregKey key2 = key.CreateSubKey("Test2"))
            using (OffregKey key3 = key2.CreateSubKey("Test3"))
            {
                key3.SetValue("A", 42);
            }

            // Open multiple levels
            using (OffregKey key = _key.OpenSubKey(@"Test\Test2\Test3"))
            {
                Assert.IsTrue(key.ValueExist("A"));
                Assert.AreEqual(42, key.GetValue("A"));
            }

            // Try to open multiple levels
            OffregKey tmpKey;
            bool couldOpen = _key.TryOpenSubKey(@"Test\Test2\Test3", out tmpKey);
            Assert.IsTrue(couldOpen);
            tmpKey.Close();

            couldOpen = _key.TryOpenSubKey(@"Test\NONEXISTENT\Test3", out tmpKey);
            Assert.IsFalse(couldOpen);
        }
    }
}