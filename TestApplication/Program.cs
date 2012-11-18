using System;
using System.ComponentModel;
using System.IO;
using OffregLib;

namespace TestApplication
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ExampleCreateHive();
        }

        private static void ExampleRecurseHive()
        {
            string file = @"reghive";

            // Open an existing Reghive
            using (OffregHive hive = OffregHive.Open(file))
            {
                OffregKey root = hive.Root;

                Console.WriteLine("Enumerating keys");
                ExampleRecurse(root);
                Console.WriteLine("Done");
            }
        }

        private static void ExampleRecurse(OffregKey key)
        {
            ValueContainer[] values = key.EnumerateValues();

            if (values.Length > 0)
            {
                Console.WriteLine("[" + key.FullName + "]");
                foreach (ValueContainer value in values)
                {
                    RegValueType type = value.Type;
                    object data = value.Data;
                    Console.WriteLine(value.Name + " (" + type + ")=" + data);
                }

                Console.WriteLine("");
            }

            SubKeyContainer[] subKeys = key.EnumerateSubKeys();

            foreach (SubKeyContainer subKey in subKeys)
            {
                try
                {
                    using (OffregKey sub = key.OpenSubKey(subKey.Name))
                    {
                        ExampleRecurse(sub);
                    }
                }
                catch (Win32Exception ex)
                {
                    Console.WriteLine("<" + key.FullName + " -> " + subKey.Name + ": " + ex.Message + ">");
                }
            }
        }

        private static void ExampleCreateHive()
        {
            // Create a new hive
            // Always use Using's to avoid forgetting to close keys and hives.
            using (OffregHive hive = OffregHive.Create())
            {
                // Create a new key
                using (OffregKey key = hive.Root.CreateSubKey("testKey"))
                {
                    // Set a value to a string
                    key.SetValue("value1", "Hello World");
                }

                // Delete the file if it exists - Offreg requires files to not exist.
                if (File.Exists("hive"))
                    File.Delete("hive");

                // Save it to disk - version 5.1 is Windows XP. This is a form of compatibility option.
                // Read more here: http://msdn.microsoft.com/en-us/library/ee210773.aspx
                hive.SaveHive("hive", 5, 1);
            }

            // Open the newly created hive
            using (OffregHive hive = OffregHive.Open("hive"))
            {
                // Open the key
                using (OffregKey key = hive.Root.OpenSubKey("testKey"))
                {
                    string value = key.GetValue("value1") as string;
                    Console.WriteLine("value1 was: " + value);
                }
            }
        }
    }
}