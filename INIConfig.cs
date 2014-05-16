using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace INIManager
{
    public class ParsingException : Exception
    {
        public ParsingException(string msg)
            : base("Parsing Error: " + msg) { }

        public ParsingException(string msg, Exception innerException)
            : base("Parsing Error: " + msg, innerException) { }
    }

    public class InvalidNameException : Exception
    {
        public InvalidNameException(string msg)
            : base("Invalid name: " + msg) { }

        public InvalidNameException(string msg, Exception innerException)
            : base("Invalid name: " + msg, innerException) { }
    }

    public class INIConfig
    {
        #region Public variables
        public bool CaseSensitive { get; set; }
        #endregion

        #region Private variables
        private CultureInfo culture = CultureInfo.InvariantCulture;
        private string eol = "\r\n";
        private List<string[]> configData = new List<string[]>();
        private Regex rHeader = new Regex(@"(?<=^\[)[a-zA-Z0-9]{1,}(?=\]$)");
        private Regex rVariable = new Regex(@"(?<=^)[a-zA-Z0-9]{1,}(?=$)");
        #endregion

        /// <summary>
        /// INI Configuration handler
        /// </summary>
        /// <param name="config">Config data</param>
        public INIConfig(System.IO.StreamReader stream)
            : this()
        {
            LoadConfig(stream.ReadToEnd());
            stream.Close();
        }

        /// <summary>
        /// INI Configuration handler
        /// </summary>
        /// <param name="config">Config data</param>
        public INIConfig(string config)
            : this()
        {
            LoadConfig(config);
        }

        /// <summary>
        /// INI Configuration handler
        /// </summary>
        public INIConfig()
        {
            CaseSensitive = false;
        }

        /// <summary>
        /// Loads a configuration file
        /// </summary>
        /// <param name="config">Config data</param>
        public void LoadConfig(string config)
        {
            LoadConfig(config, true);
        }

        /// <summary>
        /// Loads a configuration file
        /// </summary>
        /// <param name="config">Config data</param>
        /// <param name="overwriteOld">Overwrite the old data (default true)</param>
        public void LoadConfig(string config, bool overwriteOld)
        {
            if (overwriteOld)
                configData.Clear();

            string[] configLines = config.Replace("\r", "").Split('\n');

            string currentHeader = "";
            foreach (string l in configLines)
            {
                string line = l.Split(';')[0].Trim();

                // Skip "empty" lines
                if (line.Length == 0)
                    continue;

                Match match = rHeader.Match(line);
                if (match.Success)
                    currentHeader = match.Groups[0].Value;
                else
                {
                    string[] kv = line.Split(new char[] { '=' }, 2);

                    if (kv.Length == 1 && line.Contains("="))
                        kv = new string[] { kv[0], "" };
                    else if (kv.Length != 2)
                        throw new ParsingException("Invalid line: \"" + line + "\". Unable to extract the key/value");

                    if (!rVariable.Match(kv[0]).Success)
                        throw new InvalidNameException("Variable name\"" + kv[1] + "\" contains invalid characters");

                    if (currentHeader.Length == 0)
                        throw new ParsingException("Trying to define a variable without it having a valid header");

                    SetVariable(currentHeader, kv[0], kv[1]);
                }
            }
        }

        /// <summary>
        /// Saves the config
        /// Warning: It does NOT store any comments
        /// </summary>
        /// <returns>Config in a string form</returns>
        public string SaveConfig()
        {
            configData.Sort(delegate(string[] s1, string[] s2) { return string.Compare(s1[0], s2[0]); });

            string config = "";
            string currentHeader = "";
            foreach (string[] variable in configData)
            {
                if (variable[0] != currentHeader)
                {
                    currentHeader = variable[0];
                    config += eol + eol + "[" + currentHeader + "]";
                }
                config += eol + variable[1] + "=" + variable[2];
            }

            return config.TrimStart();
        }


        override public string ToString()
        {
            return SaveConfig();
        }

        #region Getters
        /// <summary>
        /// Checks if a header exists
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <returns>If the header exists</returns>
        public bool Exists(string header)
        {
            if (!rVariable.Match(header).Success)
                throw new InvalidNameException("Header name\"" + header + "\" contains invalid characters");

            if (!CaseSensitive)
                header = header.ToLower();

            for (int i = 0; i < configData.Count; i++)
            {
                if (configData[i][0] == header)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a variable exists
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <returns>If the variable exists</returns>
        public bool Exists(string header, string variable)
        {
            if (!rVariable.Match(header).Success)
                throw new InvalidNameException("Header name\"" + header + "\" contains invalid characters");
            else if (!rVariable.Match(variable).Success)
                throw new InvalidNameException("Variable name\"" + variable + "\" contains invalid characters");

            if (!CaseSensitive)
            {
                header = header.ToLower();
                variable = variable.ToLower();
            }

            for (int i = 0; i < configData.Count; i++)
            {
                if (configData[i][0] == header && configData[i][1] == variable)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a variable and tries to parse it as an string. 
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <returns>Returns the string or throws an exception if failed</returns>
        public string GetString(string header, string variable) { return GetString(header, variable, null); }

        /// <summary>
        /// Gets a variable and tries to parse it as an string. 
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <param name="defaultVar">Default return value</param>
        /// <returns>Returns the string or returns the default</returns>
        public string GetString(string header, string variable, string defaultVal)
        {
            if (!rVariable.Match(header).Success)
                throw new InvalidNameException("Header name\"" + header + "\" contains invalid characters");
            else if (!rVariable.Match(variable).Success)
                throw new InvalidNameException("Variable name\"" + variable + "\" contains invalid characters");

            if (!CaseSensitive)
            {
                header = header.ToLower();
                variable = variable.ToLower();
            }

            for (int i = 0; i < configData.Count; i++)
            {
                if (configData[i][0] == header && configData[i][1] == variable)
                {
                    return configData[i][2];
                }
            }

            if (defaultVal != null)
                return defaultVal;

            throw new ParsingException("The variable \"" + variable + "\" does not exist under header \"" + header + "\"!");
        }

        /// <summary>
        /// Gets a variable and tries to parse it as a boolean
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <returns>Returns the parsed bool or throws an exception if it failed</returns>
        public bool GetBool(string header, string variable) { return GetBool(header, variable, null); }

        /// <summary>
        /// Gets a variable and tries to parse it as a boolean
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <param name="defaultVal">The default return value</param>
        /// <returns>Returns the parsed bool or the default if it failed</returns>
        public bool GetBool(string header, string variable, bool? defaultVal)
        {
            bool outVal;
            if (Exists(header, variable) && bool.TryParse(GetString(header, variable).Trim().ToLower(), out outVal))
                return outVal;
            else if (defaultVal.HasValue)
                return (bool)defaultVal;

            throw new ParsingException("\"" + GetString(header, variable) + "\" could not be parsed as a boolean!");
        }

        /// <summary>
        /// Gets a variable and tries to parse it as an int. 
        /// WARNING: It rounds doubles down
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <returns>Returns the parsed bool or throws an exception if it failed</returns>
        public int GetInt(string header, string variable) { return GetInt(header, variable, null); }

        /// <summary>
        /// Gets a variable and tries to parse it as an int. 
        /// WARNING: It rounds doubles down
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <param name="defaultVal">The default return value</param>
        /// <returns>Returns the parsed bool or returns the default if it failed</returns>
        public int GetInt(string header, string variable, int? defaultVal)
        {
            int outVal;
            if (Exists(header, variable) && int.TryParse(GetString(header, variable).Trim(), out outVal))
                return outVal;
            else if (defaultVal.HasValue)
                return (int)defaultVal;

            throw new ParsingException("\"" + GetString(header, variable) + "\" could not be parsed as a integer!");
        }

        /// <summary>
        /// Gets a variable and tries to parse it as an int. 
        /// WARNING: It rounds doubles down
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <returns>Returns the parsed bool or throws an exception if it failed</returns>
        public char GetChar(string header, string variable) { return GetChar(header, variable, null); }

        /// <summary>
        /// Gets a variable and tries to parse it as an int. 
        /// WARNING: It rounds doubles down
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <param name="defaultVal">The default return value</param>
        /// <returns>Returns the parsed bool or throws an exception if it failed</returns>
        public char GetChar(string header, string variable, char? defaultVal)
        {
            if (Exists(header, variable))
            {
                string outVal = GetString(header, variable);
                if (outVal.Trim().Length == 1)
                    return outVal.Trim()[0];
                else if (outVal.Trim().Length == 0 && outVal.Length > 0)
                    return ' ';
            }
            else if (defaultVal.HasValue)
                return (char)defaultVal;

            throw new ParsingException("\"" + GetString(header, variable) + "\" could not be parsed as a integer!");
        }

        /// <summary>
        /// Gets a variable and tries to parse it as an double. 
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <returns>Returns the parsed bool or throws an exception if it failed</returns>
        public double GetDouble(string header, string variable) { return GetDouble(header, variable, null); }

        /// <summary>
        /// Gets a variable and tries to parse it as an double. 
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <param name="defaultVal">The default return value</param>
        /// <returns>Returns the parsed bool or returns the default if it failed</returns>
        public double GetDouble(string header, string variable, double? defaultVal)
        {
            double outVal;
            // Includes i18n
            if (Exists(header, variable) && double.TryParse(GetString(header, variable).Trim(), NumberStyles.Float, culture, out outVal))
                return outVal;
            else if (defaultVal.HasValue)
                return (double)defaultVal;

            throw new ParsingException("\"" + GetString(header, variable) + "\" could not be parsed as a double!");
        }
        #endregion

        #region Setters
        /// <summary>
        /// Sets the value of a variable and creates it if it doesn't exist
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <param name="value">The value you want to set the variable to</param>
        public void SetVariable(string header, string variable, string value)
        {
            if (!rVariable.Match(header).Success)
                throw new InvalidNameException("Header name\"" + header + "\" contains invalid characters");
            else if (!rVariable.Match(variable).Success)
                throw new InvalidNameException("Variable name\"" + variable + "\" contains invalid characters");

            if (!CaseSensitive)
            {
                header = header.ToLower();
                variable = variable.ToLower();
            }

            for (int i = 0; i < configData.Count; i++)
            {
                if (configData[i][0] == header && configData[i][1] == variable)
                {
                    configData[i][2] = value;
                    return;
                }
            }
            
            configData.Add(new string[] { header, variable, value });
        }

        /// <summary>
        /// Sets the value of a variable
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <param name="value">The value you want to set the variable to</param>
        public void SetVariable(string header, string variable, bool value)
        {
            SetVariable(header, variable, value ? "True" : "False");
        }

        /// <summary>
        /// Sets the value of a variable
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <param name="value">The value you want to set the variable to</param>
        public void SetVariable(string header, string variable, char value)
        {
            SetVariable(header, variable, value.ToString());
        }

        /// <summary>
        /// Sets the value of a variable
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <param name="value">The value you want to set the variable to</param>
        public void SetVariable(string header, string variable, int value)
        {
            SetVariable(header, variable, value.ToString());
        }

        /// <summary>
        /// Sets the value of a variable
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <param name="value">The value you want to set the variable to</param>
        public void SetVariable(string header, string variable, double value)
        {
            SetVariable(header, variable, value.ToString(culture));
        }

        /// <summary>
        /// Removes the header and all the variables under that header
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <returns></returns>
        public bool RemoveHeader(string header)
        {
            if (!rVariable.Match(header).Success)
                throw new InvalidNameException("Header name\"" + header + "\" contains invalid characters");

            if (!CaseSensitive)
                header = header.ToLower();

            bool found = false;
            for (int i = 0; i < configData.Count; i++)
            {
                if (configData[i][0] == header)
                {
                    configData.Remove(configData[i]);
                    found = true;
                }
            }

            return found;
        }

        /// <summary>
        /// Removes a variable
        /// </summary>
        /// <param name="header">Category name/header</param>
        /// <param name="variable">Variable name</param>
        /// <returns></returns>
        public bool RemoveVariable(string header, string variable)
        {
            if (!rVariable.Match(header).Success)
                throw new InvalidNameException("Header name\"" + header + "\" contains invalid characters");
            else if (!rVariable.Match(variable).Success)
                throw new InvalidNameException("Variable name\"" + variable + "\" contains invalid characters");

            if (!CaseSensitive)
            {
                header = header.ToLower();
                variable = variable.ToLower();
            }

            bool found = false;
            for (int i = 0; i < configData.Count; i++)
            {
                if (configData[i][0] == header && configData[i][1] == variable)
                {
                    configData.Remove(configData[i]);
                    found = true;
                }
            }

            return found;
        }
        #endregion
    }
}
