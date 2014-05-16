INI-Config
==========

An INI config reader/writer with excelent in-code documentation. 
It supports basic INI stuff but it can also write comments when saving and read multi line strings!

#Example

```C#
class Program
{
    static void Main(string[] args)
    {
        /* 
         * This is a very basic example on how to use the INIConfig
         * It can do a whole lot more
         */

        INIConfig config = new INIConfig();

        config.SetVariable("food", "cake", "Truth"); //overwrite
        config.SetVariable("food", "cake", "LIES");
        config.SetVariable("food", "strawberry", "great");
        config.SetVariable("food", "pie", "raspberry");
        config.SetVariable("food", "penuts", "alergic?");
        config.RemoveVariable("food", "penuts");

        config.SetVariable("generic", "string", "Lorem lipsum \n ipsum latin stuff or something..."); //newlines!
        config.SetVariable("generic", "int", 9001);
        config.SetVariable("generic", "double", Math.PI);
        config.SetVariable("generic", "bool", true);

        config.SetVariable("one", "thing", "blah");

        string saved = config.SaveConfig();
        Console.WriteLine(saved);

        Console.WriteLine("\r\n\r\nParsed:\r\n");

        INIConfig config2 = new INIConfig();
        config2.ReturnDefaultOnException = true;
        config2.LoadConfig(saved);

        Console.WriteLine("food, cake = " + config2.GetString("food", "cake"));
        Console.WriteLine("food, strawbery = " + config2.GetString("food", "strawbery"));
        Console.WriteLine("food, pie = " + config2.GetString("food", "pie"));
        Console.WriteLine("food, penuts = " + config2.GetString("food", "penuts", "Oh noes a backup value!")); //should be empty!

        Console.WriteLine("generic, string = " + config2.GetString("generic", "string"));
        Console.WriteLine("generic, int = {0}", config2.GetInt("generic", "int"));
        Console.WriteLine("generic, double = {0}", config2.GetDouble("generic", "double"));
        Console.WriteLine("generic, bool = {0}", config2.GetBool("generic", "bool"));

        Console.WriteLine("one, thing = " + config2.GetString("one", "thing"));

        Console.ReadLine();

    }
}
```
