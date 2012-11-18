using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using OffregLib;

namespace OffregLibTests
{
    [TestClass]
    public class InvalidValueTests
    {
        private OffregHive _hive;
        private OffregKey _key;

        [TestInitialize]
        public void Initiate()
        {
            _hive = OffregHive.Create();
            _key = _hive.Root.CreateSubKey("A");
        }

        [TestCleanup]
        public void Destroy()
        {
            _key.Close();
            _hive.Close();
        }

        private void EnsureValueNamesExist(params string[] expectNames)
        {
            Assert.AreEqual(expectNames.Length, _key.ValueCount);

            // Check actual names
            string[] actualNames = _key.GetValueNames();
            Assert.AreEqual(expectNames.Length, actualNames.Length);

            // Check actual names, #2
            ValueContainer[] actualEnumNames = _key.EnumerateValues();
            Assert.AreEqual(expectNames.Length, actualNames.Length);

            // Ensure all expected names exist
            Assert.IsTrue(expectNames.All(actualNames.Contains));
            Assert.IsTrue(expectNames.All(x => actualEnumNames.Any(s => s.Name == x)));
        }

        [TestMethod]
        public void InvalidValueSaveReadSz()
        {
            byte[] data = new byte[11];
            Encoding.Unicode.GetBytes("hello", 0, 5, data, 0);

            // Data is now misaligned (11 bytes for UTF-16)
            _key.SetValue("a", data, RegValueType.REG_SZ);
            EnsureValueNamesExist("a");

            object result = _key.GetValue("a");
            object tryParsed;
            bool couldParse = _key.TryGetValue("a", out tryParsed);

            Assert.IsFalse(couldParse);

            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.IsInstanceOfType(tryParsed, typeof(byte[]));

            Assert.IsTrue(((byte[])result).SequenceEqual(data));
            Assert.IsTrue(((byte[])tryParsed).SequenceEqual(data));
        }

        [TestMethod]
        public void InvalidValueSaveReadExpandSz()
        {
            byte[] data = new byte[11];
            Encoding.Unicode.GetBytes("hello", 0, 5, data, 0);

            _key.SetValue("a", data, RegValueType.REG_EXPAND_SZ);
            EnsureValueNamesExist("a");

            object result = _key.GetValue("a");
            object tryParsed;
            bool couldParse = _key.TryGetValue("a", out tryParsed);

            Assert.IsFalse(couldParse);

            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.IsInstanceOfType(tryParsed, typeof(byte[]));

            Assert.IsTrue(((byte[])result).SequenceEqual(data));
            Assert.IsTrue(((byte[])tryParsed).SequenceEqual(data));
        }

        [TestMethod]
        public void InvalidValueSaveReadBinary()
        {
            Assert.Inconclusive("No clue how to do this");
        }

        [TestMethod]
        public void InvalidValueSaveReadDword()
        {
            byte[] data = new byte[3];

            _key.SetValue("a", data, RegValueType.REG_DWORD);
            EnsureValueNamesExist("a");

            object result = _key.GetValue("a");
            object tryParsed;
            bool couldParse = _key.TryGetValue("a", out tryParsed);

            Assert.IsFalse(couldParse);

            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.IsInstanceOfType(tryParsed, typeof(byte[]));

            Assert.IsTrue(((byte[])result).SequenceEqual(data));
            Assert.IsTrue(((byte[])tryParsed).SequenceEqual(data));
        }

        [TestMethod]
        public void InvalidValueSaveReadDwordBigEndian()
        {
            byte[] data = new byte[3];

            _key.SetValue("a", data, RegValueType.REG_DWORD_BIG_ENDIAN);
            EnsureValueNamesExist("a");

            object result = _key.GetValue("a");
            object tryParsed;
            bool couldParse = _key.TryGetValue("a", out tryParsed);

            Assert.IsFalse(couldParse);

            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.IsInstanceOfType(tryParsed, typeof(byte[]));

            Assert.IsTrue(((byte[])result).SequenceEqual(data));
            Assert.IsTrue(((byte[])tryParsed).SequenceEqual(data));
        }

        [TestMethod]
        public void InvalidValueSaveReadLink()
        {
            byte[] data = new byte[11];
            Encoding.Unicode.GetBytes("hello", 0, 5, data, 0);

            _key.SetValue("a", data, RegValueType.REG_LINK);
            EnsureValueNamesExist("a");

            object result = _key.GetValue("a");
            object tryParsed;
            bool couldParse = _key.TryGetValue("a", out tryParsed);

            Assert.IsFalse(couldParse);

            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.IsInstanceOfType(tryParsed, typeof(byte[]));

            Assert.IsTrue(((byte[])result).SequenceEqual(data));
            Assert.IsTrue(((byte[])tryParsed).SequenceEqual(data));
        }

        [TestMethod]
        public void InvalidValueSaveReadMultiSzZeroLength()
        {
            // This test parses a zero-length string. This isn't possible according to the standard however.
            byte[] data = new byte[4];

            _key.SetValue("a", data, RegValueType.REG_MULTI_SZ);
            EnsureValueNamesExist("a");

            object result = _key.GetValue("a");
            object tryParsed;
            bool couldParse = _key.TryGetValue("a", out tryParsed);

            Assert.IsTrue(couldParse);

            Assert.IsInstanceOfType(result, typeof(string[]));
            Assert.IsInstanceOfType(tryParsed, typeof(string[]));

            Assert.AreEqual(1, ((string[])result).Length);
            Assert.AreEqual(1, ((string[])tryParsed).Length);

            Assert.AreEqual(string.Empty, ((string[])result)[0]);
            Assert.AreEqual(string.Empty, ((string[])tryParsed)[0]);
        }

        [TestMethod]
        public void InvalidValueSaveReadMultiSzZeroStrings()
        {
            byte[] data = new byte[2];

            _key.SetValue("a", data, RegValueType.REG_MULTI_SZ);
            EnsureValueNamesExist("a");

            object result = _key.GetValue("a");
            object tryParsed;
            bool couldParse = _key.TryGetValue("a", out tryParsed);

            Assert.IsTrue(couldParse);

            Assert.IsInstanceOfType(result, typeof(string[]));
            Assert.IsInstanceOfType(tryParsed, typeof(string[]));

            Assert.AreEqual(0, ((string[])result).Length);
            Assert.AreEqual(0, ((string[])tryParsed).Length);
        }

        [TestMethod]
        public void InvalidValueSaveReadMultiSzEmpty()
        {
            byte[] data = new byte[0];

            _key.SetValue("a", data, RegValueType.REG_MULTI_SZ);
            EnsureValueNamesExist("a");

            object result = _key.GetValue("a");
            object tryParsed;
            bool couldParse = _key.TryGetValue("a", out tryParsed);

            Assert.IsFalse(couldParse);

            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.IsInstanceOfType(tryParsed, typeof(byte[]));

            Assert.IsTrue(((byte[])result).SequenceEqual(data));
            Assert.IsTrue(((byte[])tryParsed).SequenceEqual(data));
        }

        [TestMethod]
        public void InvalidValueSaveReadMultiSzMisAligned()
        {
            byte[] data = new byte[3];

            _key.SetValue("a", data, RegValueType.REG_MULTI_SZ);
            EnsureValueNamesExist("a");

            object result = _key.GetValue("a");
            object tryParsed;
            bool couldParse = _key.TryGetValue("a", out tryParsed);

            Assert.IsFalse(couldParse);

            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.IsInstanceOfType(tryParsed, typeof(byte[]));

            Assert.IsTrue(((byte[])result).SequenceEqual(data));
            Assert.IsTrue(((byte[])tryParsed).SequenceEqual(data));
        }

        [TestMethod]
        public void InvalidValueSaveReadMultiSzNotEndsInNull()
        {
            byte[] data = new byte[4];
            data[0] = 20;
            data[1] = 40;
            data[2] = 60;
            data[3] = 80;

            _key.SetValue("a", data, RegValueType.REG_MULTI_SZ);
            EnsureValueNamesExist("a");

            object result = _key.GetValue("a");
            object tryParsed;
            bool couldParse = _key.TryGetValue("a", out tryParsed);

            Assert.IsFalse(couldParse);

            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.IsInstanceOfType(tryParsed, typeof(byte[]));

            Assert.IsTrue(((byte[])result).SequenceEqual(data));
            Assert.IsTrue(((byte[])tryParsed).SequenceEqual(data));
        }

        [TestMethod]
        public void InvalidValueSaveReadResourceList()
        {
            Assert.Inconclusive("Not supported");
        }

        [TestMethod]
        public void InvalidValueSaveReadFullResourceDescription()
        {
            Assert.Inconclusive("Not supported");
        }

        [TestMethod]
        public void InvalidValueSaveReadResourceRequirementsList()
        {
            Assert.Inconclusive("Not supported");
        }

        [TestMethod]
        public void InvalidValueSaveReadQword()
        {
            byte[] data = new byte[5];

            _key.SetValue("a", data, RegValueType.REG_QWORD);
            EnsureValueNamesExist("a");

            object result = _key.GetValue("a");
            object tryParsed;
            bool couldParse = _key.TryGetValue("a", out tryParsed);

            Assert.IsFalse(couldParse);

            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.IsInstanceOfType(tryParsed, typeof(byte[]));

            Assert.IsTrue(((byte[])result).SequenceEqual(data));
            Assert.IsTrue(((byte[])tryParsed).SequenceEqual(data));
        }
    }
}