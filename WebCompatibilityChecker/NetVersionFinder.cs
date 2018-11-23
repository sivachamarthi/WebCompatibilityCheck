using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace WebCompatibilityChecker
{
    /// <summary>
    /// Get installed version on a machine
    /// </summary>
    public static class NetVersionFinder
    {
        public static Dictionary<string, string> InstalledVersions { get; set; }

       
        public static void ReadAnSetVersionsFromRegistry()
        {
            InstalledVersions = new Dictionary<string, string>();
            // Opens the registry key for the .NET Framework entry.
            using (RegistryKey ndpKey =
                RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    if (versionKeyName.StartsWith("v"))
                    {
                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        string name = (string)versionKey.GetValue("Version", "");
                        string sp = versionKey.GetValue("SP", "").ToString();
                        string install = versionKey.GetValue("Install", "").ToString();
                        
                        if (name != "")
                        {
                            InstalledVersions.Add(versionKeyName, name);
                            continue;
                        }
                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            if (name != "")
                                sp = subKey.GetValue("SP", "").ToString();
                            install = subKey.GetValue("Install", "").ToString();
                        }
                        InstalledVersions.Add(versionKeyName, name);
                    }
                }
            }
        }
    }
}
