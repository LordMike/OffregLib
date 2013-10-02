# OffregLib
A .NET wrapper for Microsoft's Offreg.dll

## Purpose

Almost [all versions of Windows](http://en.wikipedia.org/wiki/Windows_Registry "Windows Registry history") comes with a Registry. It's a common store for all kinds of settings, and is available in almost all languages via. some form of API. Internally, Windows stores these registries (LocalUser, LocalMachine etc.) in multiple "Registry Hive" files. Most of them are in: "C:\Windows\System32\Config".

I had a task where I needed to access and interpret / modify these Registry Hive files in their raw format (processing data from other computers). 'Normally', one would attach the Registry Hive file to the local system's registry and then use it from there. This however requires some privileges which you don't always have (various client and servers). 

For that purpose, microsoft provided the [Offreg.dll](http://msdn.microsoft.com/en-us/library/ee210757.aspx "MSDN Offline Registry") (available in various Windows SDK's and Driver kits), which is a library for interacting with raw Registry Hive files. It is a COM library, and I have made a .NET wrapper for it which resembles the .NET Registry API as much as possible.

## Nuget

Install from Nuget using the command: **Install-Package OffregLib**
View more about that here: http://nuget.org/packages/OffregLib/

## Features

* Open, create and modify Registry Hive files programmatically.
* Read, enumerate and delete subkeys and values.
* API helpers to delete subkey trees (normally you'd need to recurse manually).
* All objects are disposable, and will close their respective pointers correctly.
* Resembles the .NET Registry ([Microsoft.Win32.Registry](http://msdn.microsoft.com/en-us/library/microsoft.win32.registry.aspx)) as much as possible.
* Automatic switching between 32 and 64 bit offreg.dll files.
* Unit tests to test basic functionality

## Notes

* The library works in UTF-16 Unicode, so on some older systems (like Windows 2000), I expect it to not work (if one could run it even), as this system uses ANSI encoding.
* To obtain a large registry hive to work with, see ObtainingSystemHives.md
* The library is currently very slow, somewhere near 6-7 time slower than .NET's own Registry access. Future improvements can be made to improve performance.
* I have limited support for the Resource types:
	* REG_FULL_RESOURCE_DESCRIPTOR
	* REG_RESOURCE_LIST
	* REG_RESOURCE_REQUIREMENTS_LIST

## Examples 
### Create a registry hive, save a string, and read it again.

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
    string file = @"hiveFile";

    // Open an existing Registry Hive
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

For more examples, look at the Test application on how to accomplish various functions.