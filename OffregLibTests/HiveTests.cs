using System.ComponentModel;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using OffregLib;

namespace OffregLibTests
{
    [TestClass]
    public class HiveTests
    {
        [TestMethod]
        public void RegHiveOpen()
        {
            const string file = "exampleHive";

            try
            {
                File.WriteAllBytes(file, Resources.ExampleHive);

                // Open existing reghive
                using (OffregHive hive = OffregHive.Open(file))
                {
                    SubKeyContainer[] subKeysRoot = hive.Root.EnumerateSubKeys();

                    Assert.AreEqual(1, subKeysRoot.Length);
                    Assert.AreEqual("test", subKeysRoot[0].Name);

                    using (OffregKey subKey1 = hive.Root.OpenSubKey(subKeysRoot[0].Name))
                    {
                        SubKeyContainer[] subKeys1 = subKey1.EnumerateSubKeys();

                        Assert.AreEqual(1, subKeys1.Length);
                        Assert.AreEqual("test", subKeys1[0].Name);

                        using (OffregKey subKey2 = subKey1.OpenSubKey(subKeys1[0].Name))
                        {
                            ValueContainer[] values2 = subKey2.EnumerateValues();

                            Assert.AreEqual(1, values2.Length);

                            Assert.AreEqual("valueName", values2[0].Name);
                            Assert.AreEqual(RegValueType.REG_SZ, values2[0].Type);
                            Assert.AreEqual("Hello world", (string) values2[0].Data);
                        }

                        ValueContainer[] values1 = subKey1.EnumerateValues();

                        Assert.AreEqual(2, values1.Length);

                        ValueContainer valueInt = values1.SingleOrDefault(s => s.Name == "valueInt");
                        ValueContainer valueLong = values1.SingleOrDefault(s => s.Name == "valueLong");

                        Assert.IsNotNull(valueInt);
                        Assert.AreEqual("valueInt", valueInt.Name);
                        Assert.AreEqual(RegValueType.REG_DWORD, valueInt.Type);
                        Assert.AreEqual(42, (int) valueInt.Data);

                        Assert.IsNotNull(valueLong);
                        Assert.AreEqual("valueLong", valueLong.Name);
                        Assert.AreEqual(RegValueType.REG_QWORD, valueLong.Type);
                        Assert.AreEqual(1337, (long) valueLong.Data);
                    }
                }
            }
            finally
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
        }

        [TestMethod]
        public void RegHiveOpenMissingFile()
        {
            const string file = "exampleHive";

            try
            {
                Assert.IsFalse(File.Exists(file));

                // Open existing reghive
                using (OffregHive.Open(file))
                {
                    Assert.Fail();
                }
            }
            catch (Win32Exception ex)
            {
                Assert.AreEqual(Win32Result.ERROR_FILE_NOT_FOUND, (Win32Result) ex.NativeErrorCode);
            }
        }

        [TestMethod]
        public void RegHiveCreate()
        {
            using (OffregHive hive = OffregHive.Create())
            {
                Assert.AreEqual(0, hive.Root.SubkeyCount);

                hive.Root.CreateSubKey("test").Close();
                Assert.AreEqual(1, hive.Root.SubkeyCount);

                hive.Root.CreateSubKey("test2").Close();
                Assert.AreEqual(2, hive.Root.SubkeyCount);

                hive.Root.CreateSubKey("test2").Close();
                Assert.AreEqual(2, hive.Root.SubkeyCount);
            }
        }

        [TestMethod]
        public void RegHiveSave()
        {
            string fileName = Path.GetTempFileName();
            try
            {
                // File must not exist
                File.Delete(fileName);

                // Create hive
                using (OffregHive hive = OffregHive.Create())
                {
                    using (OffregKey key = hive.Root.CreateSubKey("test"))
                    {
                        key.SetValue("Value", "Hello world");
                    }

                    // Save for XP
                    hive.SaveHive(fileName, 5, 1);
                }

                Assert.IsTrue(File.Exists(fileName));

                // Open hive
                using (OffregHive hive = OffregHive.Open(fileName))
                {
                    Assert.AreEqual(1, hive.Root.SubkeyCount);
                    Assert.AreEqual(0, hive.Root.ValueCount);

                    using (OffregKey key = hive.Root.OpenSubKey("test"))
                    {
                        Assert.AreEqual(0, key.SubkeyCount);
                        Assert.AreEqual(1, key.ValueCount);

                        ValueContainer container = key.EnumerateValues().First();

                        Assert.AreEqual(RegValueType.REG_SZ, container.Type);
                        Assert.AreEqual("Value", container.Name);
                        Assert.IsInstanceOfType(container.Data, typeof (string));
                        Assert.AreEqual("Hello world", (string) container.Data);
                    }
                }
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        [TestMethod]
        public void RegHiveSaveExistingFile()
        {
            string fileName = Path.GetTempFileName();

            try
            {
                Assert.IsTrue(File.Exists(fileName));

                // Create hive
                using (OffregHive hive = OffregHive.Create())
                {
                    // Save for XP
                    hive.SaveHive(fileName, 5, 1);
                }

                Assert.Fail();
            }
            catch (Win32Exception ex)
            {
                Assert.AreEqual(Win32Result.ERROR_FILE_EXISTS, (Win32Result) ex.NativeErrorCode);
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }
    }
}