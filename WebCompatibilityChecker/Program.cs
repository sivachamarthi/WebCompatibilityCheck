using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace WebCompatibilityChecker
{
    class Program
    {
        private enum ExitCode : int
        {
            Success = 0,
            InvalidParameters = 1,
            NoDifference = 2,
            FilesNotFound = 3,
            UnknownError = 100
        }

        static int Main(string[] args)
        {
            Logger logger = new Logger();

            Console.WriteLine("\n");
            if (args == null || args.Length != 3)
            {
                Console.WriteLine("Error: Unexpected Number of Params.\n Usage: WebCompatibilityChecker WebConfig1 WebConfig2 WebappDllFile");
                logger.Error("Error: Unexpected Number of Params.\n Usage: WebCompatibilityChecker WebConfig1 WebConfig2 WebappDllFile");
                return (int)ExitCode.InvalidParameters;
            }

            NetVersionFinder.ReadAnSetVersionsFromRegistry();
            
            string configFile1 = args[0];
            string configFile2 = args[1];
            string dllPath = args[2];

            //validate file paths/existance
            if (!File.Exists(configFile1))
            {
                Console.WriteLine("File not found :"+ configFile1);
                logger.Error("File not found :" + configFile1);
                return (int)ExitCode.FilesNotFound;
            }
            if (!File.Exists(configFile2))
            {
                Console.WriteLine("File not found :" + configFile2);
                logger.Error("File not found :" + configFile2);
                return (int)ExitCode.FilesNotFound;
            }
            if (!File.Exists(dllPath))
            {
                Console.WriteLine("File not found :" + dllPath);
                logger.Error("File not found :" + dllPath);
                return (int)ExitCode.FilesNotFound;
            }

            var assembly = Assembly.ReflectionOnlyLoadFrom(dllPath);
            var dllTargetFramwrok = assembly.ImageRuntimeVersion;
            Console.WriteLine("DLL Target framework: "+ dllTargetFramwrok);
            
            var targetFramework1 = GetTargetFrameworkFromConfigFile(configFile1);
            var targetFramework2 = GetTargetFrameworkFromConfigFile(configFile2);

            //Compare target frameworks
            if (!targetFramework1.Equals(targetFramework2))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Target framework NOT MATCHING in config files .\n "+configFile1+" has : " + targetFramework1 + " ,"+configFile2+" has :" + targetFramework2);
                logger.Error("Target framework NOT MATCHING in config files .\n " + configFile1 + " has : " + targetFramework1 + " ," + configFile2 + " has :" + targetFramework2);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("Traget framework in config file match.Both files have "+targetFramework1);
                logger.Info("Traget framework in config file match.Both files have " + targetFramework1);
                var configFrameworkInt = ConvetVersionToInt(targetFramework1);
                var dllFrameworkInt = ConvetVersionToInt(dllTargetFramwrok);

                //if files match, Check the dll target framework if it is less than or equal to config file target
                if (dllFrameworkInt > configFrameworkInt)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The DLL file targets higher version (" + dllTargetFramwrok + ") than the one mentioned in config file(" + targetFramework1 + ").");
                    logger.Error("The DLL file targets higher version (" + dllTargetFramwrok + ") than the one mentioned in config file(" + targetFramework1 + ").");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("The DLL file targets lower version (" + dllTargetFramwrok + ") than the one mentioned in config file(" + targetFramework1 + ").");
                    logger.Info("The DLL file targets lower version (" + dllTargetFramwrok + ") than the one mentioned in config file(" + targetFramework1 + ").");
                } 
            }
            Console.WriteLine("\n Net frameworks installed on this Machine : \n");
            Console.WriteLine("".PadRight(47, '-'));
            foreach (var item in NetVersionFinder.InstalledVersions)
            {
                var output = "|     " + item.Key.PadRight(10) + "    |     " + item.Value.PadRight(15) + "     |";
                Console.WriteLine(output);
                Console.WriteLine("".PadRight(output.Length, '-'));
            }


            Console.ReadLine();
            return 0;
        }

        /// <summary>
        /// Works for config files 4.5 and above
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static string GetTargetFrameworkFromConfigFile(string filePath)
        {
            XmlDocument doc1 = new XmlDocument();
            doc1.Load(filePath);

            string targetedFramwork = "";

            foreach (XmlNode node in doc1.ChildNodes)
            {
                if (node.Name.Equals("configuration"))
                {
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        if (innerNode.Name.Equals("system.web"))
                        {
                            foreach (XmlNode item in innerNode.ChildNodes)
                            {
                                if (item.Name.Equals("httpRuntime"))
                                {
                                    targetedFramwork = item.Attributes["targetFramework"].Value;
                                }
                            }
                        }
                    }
                }
            }

            return targetedFramwork;
        }

        public static int ConvetVersionToInt(string version)
        {
            var str = version.Replace("v", "").Replace(".", "").PadRight(8,'0');
            return Convert.ToInt32(str);

        }
    }
}
