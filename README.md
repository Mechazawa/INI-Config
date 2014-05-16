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
            config.SetVariable("Header1", "thing", 123);
            config.SetVariable("Header1", "somebool", false);
            config.SetVariable("user", "username", "bob");
            config.SetVariable("user", "password", "bobIsCool123");
            console.WriteLine(config + ""); // Calls tostring which calls saveconfig.
        }
    }
}
```
