using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void ValueCreateLongName()
        {
            StringBuilder sb = new StringBuilder(16383);
            while (sb.Length < 16383)
                sb.Append("A");
            string longName = sb.ToString();

            _key.SetValue(longName, "Hello");

            object value = _key.GetValue(longName);

            Assert.IsInstanceOfType(value, typeof(string));
            Assert.AreEqual("Hello", (string)value);
        }

        [TestMethod]
        public void ValueCreateTooLongName()
        {
            try
            {
                StringBuilder sb = new StringBuilder(16384);
                while (sb.Length < 16384)
                    sb.Append("A");
                string longName = sb.ToString();

                _key.SetValue(longName, "Hello");

                Assert.Fail();
            }
            catch (Win32Exception ex)
            {
                Assert.AreEqual(Win32Result.ERROR_INVALID_PARAMETER, (Win32Result)ex.NativeErrorCode);
            }
        }

        [TestMethod]
        public void ValueSaveReadSz()
        {
            string test = "test";
            string test2 = "testRocks"; // Longer string, to test if the previous one gets padded with a null-terminator

            _key.SetValue("B", test);
            EnsureValueNamesExist("B");

            _key.SetValue("C", test2);
            EnsureValueNamesExist("B", "C");

            Assert.AreEqual(RegValueType.REG_SZ, _key.GetValueKind("B"));
            Assert.AreEqual(RegValueType.REG_SZ, _key.GetValueKind("C"));

            object result = _key.GetValue("B");
            object result2 = _key.GetValue("C");

            byte[] binary1 = _key.GetValueBytes("B");
            byte[] binary2 = _key.GetValueBytes("C");

            Assert.IsInstanceOfType(result, typeof(string));
            Assert.AreEqual(test, (string)result);

            Assert.IsInstanceOfType(result2, typeof(string));
            Assert.AreEqual(test2, (string)result2);

            Assert.IsTrue(binary1.SequenceEqual(Encoding.Unicode.GetBytes(test).Concat(new byte[] { 0x00, 0x00 })));
            Assert.IsTrue(binary2.SequenceEqual(Encoding.Unicode.GetBytes(test2).Concat(new byte[] { 0x00, 0x00 })));

            _key.DeleteValue("B");
            EnsureValueNamesExist("C");

            _key.DeleteValue("C");
            EnsureValueNamesExist();
        }

        [TestMethod]
        public void ValueSaveReadExpandSz()
        {
            string test = "test";

            _key.SetValue("B", test, RegValueType.REG_EXPAND_SZ);
            EnsureValueNamesExist("B");

            Assert.AreEqual(RegValueType.REG_EXPAND_SZ, _key.GetValueKind("B"));

            object result = _key.GetValue("B");
            byte[] binary = _key.GetValueBytes("B");

            Assert.IsInstanceOfType(result, typeof(string));
            Assert.AreEqual(test, (string)result);
            Assert.IsTrue(binary.SequenceEqual(Encoding.Unicode.GetBytes(test).Concat(new byte[] { 0x00, 0x00 })));

            _key.DeleteValue("B");
            EnsureValueNamesExist();
        }

        private void BinaryTest(RegValueType type, int size)
        {
            byte[] test = new byte[size];
            new Random().NextBytes(test);

            _key.SetValue("B", test, type);

            Assert.AreEqual(type, _key.GetValueKind("B"));

            object result = _key.GetValue("B");
            byte[] binary = _key.GetValueBytes("B");

            Assert.IsInstanceOfType(result, typeof(byte[]));
            Assert.IsTrue(test.SequenceEqual((byte[])result));
            Assert.IsTrue(test.SequenceEqual(binary));

            _key.DeleteValue("B");
            EnsureValueNamesExist();
        }

        [TestMethod]
        public void ValueSaveReadBinary()
        {
            for (int i = 0; i < 65535; i += 128)
                BinaryTest(RegValueType.REG_BINARY, i);
        }

        [TestMethod]
        public void ValueSaveReadBinaryAsAllTypes()
        {
            byte[] expected = new byte[256];
            Random random = new Random();

            foreach (RegValueType type in Enum.GetValues(typeof(RegValueType)))
            {
                random.NextBytes(expected);

                _key.SetValue("B", expected, type);
                Assert.AreEqual(type, _key.GetValueKind("B"));

                byte[] binaryResult = _key.GetValueBytes("B");
                Assert.IsTrue(expected.SequenceEqual(binaryResult));

                object parsedResult;
                _key.TryGetValue("B", out parsedResult);

                _key.DeleteValue("B");
                EnsureValueNamesExist();
            }
        }

        [TestMethod]
        public void ValueSaveReadBinaryAsAllTypesLarge()
        {
            byte[] expected = new byte[65535];
            Random random = new Random();

            foreach (RegValueType type in Enum.GetValues(typeof(RegValueType)))
            {
                random.NextBytes(expected);

                _key.SetValue("B", expected, type);
                Assert.AreEqual(type, _key.GetValueKind("B"));

                byte[] binaryResult = _key.GetValueBytes("B");
                Assert.IsTrue(expected.SequenceEqual(binaryResult));

                object parsedResult;
                _key.TryGetValue("B", out parsedResult);

                _key.DeleteValue("B");
                EnsureValueNamesExist();
            }
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
                EnsureValueNamesExist("B");

                Assert.AreEqual(RegValueType.REG_DWORD, _key.GetValueKind("B"));

                object result = _key.GetValue("B");
                byte[] binary = _key.GetValueBytes("B");

                Assert.IsInstanceOfType(result, typeof(int));
                Assert.AreEqual(test, (int)result);

                byte[] expectedBinary = BitConverter.GetBytes(test);
                Assert.IsTrue(binary.SequenceEqual(expectedBinary));
            }

            _key.DeleteValue("B");
            EnsureValueNamesExist();
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
                EnsureValueNamesExist("B");

                Assert.AreEqual(RegValueType.REG_DWORD_BIG_ENDIAN, _key.GetValueKind("B"));

                object result = _key.GetValue("B");
                byte[] binary = _key.GetValueBytes("B");

                Assert.IsInstanceOfType(result, typeof(int));
                Assert.AreEqual(test, (int)result);

                byte[] expectedBinary = BitConverter.GetBytes(test);
                Array.Reverse(expectedBinary);
                Assert.IsTrue(binary.SequenceEqual(expectedBinary));
            }

            _key.DeleteValue("B");
            EnsureValueNamesExist();
        }

        [TestMethod]
        public void ValueSaveReadLink()
        {
            string test = "test";

            _key.SetValue("B", test, RegValueType.REG_LINK);
            EnsureValueNamesExist("B");

            Assert.AreEqual(RegValueType.REG_LINK, _key.GetValueKind("B"));

            object result = _key.GetValue("B");
            byte[] binary = _key.GetValueBytes("B");

            Assert.IsInstanceOfType(result, typeof(string));
            Assert.AreEqual(test, (string)result);
            Assert.IsTrue(binary.SequenceEqual(Encoding.Unicode.GetBytes(test).Concat(new byte[] { 0x00, 0x00 })));

            _key.DeleteValue("B");
            EnsureValueNamesExist();
        }

        [TestMethod]
        public void ValueSaveReadMultiSz()
        {
            string[] test = new[] { "Hello", "World", "Rocks!" };

            _key.SetValue("B", test);
            EnsureValueNamesExist("B");

            Assert.AreEqual(RegValueType.REG_MULTI_SZ, _key.GetValueKind("B"));

            object result = _key.GetValue("B");
            byte[] binary = _key.GetValueBytes("B");

            Assert.IsInstanceOfType(result, typeof(string[]));
            Assert.IsTrue(test.SequenceEqual((string[])result));

            byte[] expectedBinary =
                test.SelectMany(s => Encoding.Unicode.GetBytes(s).Concat(new byte[] { 0x00, 0x00 }))
                    .Concat(new byte[] { 0x00, 0x00 })
                    .ToArray();
            Assert.IsTrue(binary.SequenceEqual(expectedBinary));

            _key.DeleteValue("B");
            EnsureValueNamesExist();
        }

        [TestMethod]
        public void ValueSaveReadResourceList()
        {
            for (int i = 0; i < 65535; i += 128)
                BinaryTest(RegValueType.REG_RESOURCE_LIST, i);
        }

        [TestMethod]
        public void ValueSaveReadFullResourceDescription()
        {
            for (int i = 0; i < 65535; i += 128)
                BinaryTest(RegValueType.REG_FULL_RESOURCE_DESCRIPTOR, i);
        }

        [TestMethod]
        public void ValueSaveReadResourceRequirementsList()
        {
            for (int i = 0; i < 65535; i += 128)
                BinaryTest(RegValueType.REG_RESOURCE_REQUIREMENTS_LIST, i);
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
                EnsureValueNamesExist("B");

                Assert.AreEqual(RegValueType.REG_QWORD, _key.GetValueKind("B"));

                object result = _key.GetValue("B");
                byte[] binary = _key.GetValueBytes("B");

                Assert.IsInstanceOfType(result, typeof(long));
                Assert.AreEqual(test, (long)result);

                byte[] expectedBinary = BitConverter.GetBytes(test);
                Assert.IsTrue(binary.SequenceEqual(expectedBinary));
            }

            _key.DeleteValue("B");
            EnsureValueNamesExist();
        }
    }
}