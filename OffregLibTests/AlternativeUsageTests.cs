using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OffregLib;

namespace OffregLibTests
{
    [TestClass]
    public class AlternativeUsageTests
    {
        [TestMethod]
        public void MultiLevelCreate()
        {
            using (OffregHive hive = OffregHive.Create())
            {
                using (OffregKey key2 = hive.Root.CreateSubKey("test"))
                {
                    using (OffregKey key = key2.CreateSubKey(@"level1"))
                    {
                        Debug.WriteLine(key.FullName);
                    }
                }
            }
        }
    }
}