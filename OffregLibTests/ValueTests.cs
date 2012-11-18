using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using OffregLib;

namespace OffregLibTests
{
    [TestClass]
    public class ValueTests
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

        private void EnsureValueNames(params string[] expectNames)
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
        public void ValueSaveReadSz()
        {
            string test = "test";
            string test2 = "testRocks"; // Longer string, to test if the previous one gets padded with a null-terminator

            _key.SetValue("B", test);
            EnsureValueNames("B");

            _key.SetValue("C", test2);
            EnsureValueNames("B", "C");

            Assert.AreEqual(RegValueType.REG_SZ, _key.GetValueKind("B"));
            Assert.AreEqual(RegValueType.REG_SZ, _key.GetValueKind("C"));

            object result = _key.GetValue("B");
            object result2 = _key.GetValue("C");

            Assert.IsInstanceOfType(result, typeof(string));
            Assert.AreEqual(test, (string)result);

            Assert.IsInstanceOfType(result2, typeof(string));
            Assert.AreEqual(test2, (string)result2);

            _key.DeleteValue("B");
            EnsureValueNames("C");

            _key.DeleteValue("C");
            EnsureValueNames();
        }

        [TestMethod]
        public void ValueSaveReadExpandSz()
        {
            string test = "test";

            _key.SetValue("B", test, RegValueType.REG_EXPAND_SZ);
            EnsureValueNames("B");

            Assert.AreEqual(RegValueType.REG_EXPAND_SZ, _key.GetValueKind("B"));

            object result = _key.GetValue("B");

            Assert.IsInstanceOfType(result, typeof(string));
            Assert.AreEqual(test, (string)result);

            _key.DeleteValue("B");
            EnsureValueNames();
        }

        [TestMethod]
        public void ValueSaveReadBinary()
        {
            byte[] test = new byte[8];
            new Random().NextBytes(test);

            _key.SetValue("B", test);

            Assert.AreEqual(RegValueType.REG_BINARY, _key.GetValueKind("B"));

            object result = _key.GetValue("B");

            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.IsTrue(test.SequenceEqual((byte[])result));

            _key.DeleteValue("B");
            EnsureValueNames();
        }

        [TestMethod]
        public void ValueSaveReadDword()
        {
            int[] tests = new int[4];
            Random random = new Random();
            tests[0] = random.Next(int.MinValue, -1);
            tests[1] = random.Next(int.MinValue, int.MaxValue);
            tests[2] = random.Next(1, int.MaxValue);
            tests[3] = 0;

            foreach (int test in tests)
            {
                _key.SetValue("B", test);
                EnsureValueNames("B");

                Assert.AreEqual(RegValueType.REG_DWORD, _key.GetValueKind("B"));

                object result = _key.GetValue("B");

                Assert.IsInstanceOfType(result, typeof(int));
                Assert.AreEqual(test, (int)result);
            }

            _key.DeleteValue("B");
            EnsureValueNames();
        }

        [TestMethod]
        public void ValueSaveReadDwordBigEndian()
        {
            int[] tests = new int[4];
            Random random = new Random();
            tests[0] = random.Next(int.MinValue, -1);
            tests[1] = random.Next(int.MinValue, int.MaxValue);
            tests[2] = random.Next(1, int.MaxValue);
            tests[3] = 0;

            foreach (int test in tests)
            {
                _key.SetValue("B", test, RegValueType.REG_DWORD_BIG_ENDIAN);
                EnsureValueNames("B");

                Assert.AreEqual(RegValueType.REG_DWORD_BIG_ENDIAN, _key.GetValueKind("B"));

                object result = _key.GetValue("B");

                Assert.IsInstanceOfType(result, typeof(int));
                Assert.AreEqual(test, (int)result);
            }

            _key.DeleteValue("B");
            EnsureValueNames();
        }

        [TestMethod]
        public void ValueSaveReadLink()
        {
            string test = "test";

            _key.SetValue("B", test, RegValueType.REG_LINK);
            EnsureValueNames("B");

            Assert.AreEqual(RegValueType.REG_LINK, _key.GetValueKind("B"));

            object result = _key.GetValue("B");

            Assert.IsInstanceOfType(result, typeof(string));
            Assert.AreEqual(test, (string)result);

            _key.DeleteValue("B");
            EnsureValueNames();
        }

        [TestMethod]
        public void ValueSaveReadMultiSz()
        {
            string[] test = new[] { "Hello", "World", "Rocks!" };

            _key.SetValue("B", test);
            EnsureValueNames("B");

            Assert.AreEqual(RegValueType.REG_MULTI_SZ, _key.GetValueKind("B"));

            object result = _key.GetValue("B");

            Assert.IsInstanceOfType(result, typeof(string[]));
            Assert.IsTrue(test.SequenceEqual((string[])result));

            _key.DeleteValue("B");
            EnsureValueNames();
        }

        [TestMethod]
        public void ValueSaveReadResourceList()
        {
            Assert.Inconclusive("Not supported");
        }

        [TestMethod]
        public void ValueSaveReadFullResourceDescription()
        {
            Assert.Inconclusive("Not supported");
        }

        [TestMethod]
        public void ValueSaveReadResourceRequirementsList()
        {
            Assert.Inconclusive("Not supported");
        }

        [TestMethod]
        public void ValueSaveReadQword()
        {
            long[] tests = new long[4];
            Random random = new Random();
            tests[0] = random.Next(int.MinValue, -1);
            tests[1] = random.Next(int.MinValue, int.MaxValue);
            tests[2] = random.Next(1, int.MaxValue);
            tests[3] = 0;

            foreach (long test in tests)
            {
                _key.SetValue("B", test);
                EnsureValueNames("B");

                Assert.AreEqual(RegValueType.REG_QWORD, _key.GetValueKind("B"));

                object result = _key.GetValue("B");

                Assert.IsInstanceOfType(result, typeof(long));
                Assert.AreEqual(test, (long)result);
            }

            _key.DeleteValue("B");
            EnsureValueNames();
        }

        [TestMethod]
        public void ValueMultiLineStringInvalid()
        {
            _key.SetValue("test", new byte[0], RegValueType.REG_MULTI_SZ);
            EnsureValueNames("test");

            object result = _key.GetValue("test");

            Assert.IsInstanceOfType(result, typeof(string[]));
            Assert.AreEqual(0, ((string[])result).Length);
        }
    }
}