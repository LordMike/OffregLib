# OffregLib
A .NET wrapper for Microsoft's Offreg.dll

## Purpose

Almost [all versions of Windows](http://en.wikipedia.org/wiki/Windows_Registry "Windows Registry history") comes with a Registry. It's a common store for all kinds of settings, and is available in almost all languages via. some form of API. Internally, Windows stores these registries (LocalUser, LocalMachine etc.) in multiple "RegHive" files. Most of them are in: "C:\Windows\System32\Config".

I had a task where I needed to access and interpret / modify these Reghive files in their raw format (processing data from other computers). 'Normally', one would attach the RegHive file to the local system's registry and then use it from there. This however requires some privileges which you don't always have (various client and servers). 

For that purpose, microsoft provided the [Offreg.dll](http://msdn.microsoft.com/en-us/library/ee210757.aspx "MSDN Offline Registry") (available in various Windows SDK's and Driver kits), which is a library for interacting with raw RegHive files. It is a COM library, and I have made a .NET wrapper for it which resembles the .NET Registry API as much as possible.

## Features


* Open, create and modify RegHive files programmatically.
* Read, enumerate and delete subkeys and values.
* API helpers to delete subkey trees (normally you'd need to recurse manually).
* All objects are disposable, and will close their respective pointers correctly.
* Resembles the .NET Registry ([Microsoft.Win32.Registry](http://msdn.microsoft.com/en-us/library/microsoft.win32.registry.aspx)) as much as possible.
* Automatic switching between 32 and 64 bit offreg.dll files.

## Examples 
### Create a reghive, save a string, and read it again.

```csharp
private static void Main(string[] args)
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
```

### Recursively enumerate everything

```csharp
private static void Main(string[] args)
{
    string file = @"C:\Users\Mike\Desktop\SOFTWARE";

    // Open an existing Reghive
    using (OffregHive hive = OffregHive.Open(file))
    {
        OffregKey root = hive.Root;

        Console.WriteLine("Enumerating keys");
        Recurse(root);
        Console.WriteLine("Done");
    }
}

private static void Recurse(OffregKey key)
{
    string[] values = key.GetValueNames();

    if (values.Length > 0)
    {
        Console.WriteLine("[" + key.FullName + "]");
        foreach (string value in values)
        {
            RegValueType type = key.GetValueKind(value);
            object data = key.GetValue(value);
            Console.WriteLine(value + " (" + type + ")=" + data);
        }

        Console.WriteLine("");
    }

    string[] subKeys = key.GetSubKeyNames();

    foreach (string subKey in subKeys)
    {
        try
        {
            using (OffregKey sub = key.OpenSubKey(subKey))
            {
                Recurse(sub);
            }
        }
        catch (Win32Exception ex)
        {
            Console.WriteLine("<" + key.FullName + " -> " + subKey + ": " + ex.Message + ">");
        }
    }
}
```

For more examples, look at the Test library on how to accomplish various functions.