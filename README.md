INI-Config
==========

An INI config reader/writer with excelent documentation

#Example

```C#
using System;
 
namespace INIConfigTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            INIConfig config = new INIConfig(); //Create a new config with no data
            config.SetVariabe("Header1", "thing", 123);
            config.SetVariabe("Header1", "somebool", false);
            config.SetVariabe("user", "username", "bob");
            config.SetVariabe("user", "password", "bobIsCool123");
            console.WriteLine(config + ""); // Calls tostring which calls saveconfig.
        }
    }
}
```
