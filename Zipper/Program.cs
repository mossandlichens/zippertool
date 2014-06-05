using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

[assembly: CLSCompliant(true)]
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Zipper
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static string currentAssemblyDirectoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        static void Main(string[] args)
        {
            var commandLineOptions = new CommandLineOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, commandLineOptions))
            {
                string input = commandLineOptions.Input;
                if (string.IsNullOrEmpty(input) == false)
                {
                    Settings.Default.Input = input;
                }

                Settings.Default.Latest = commandLineOptions.Latest;

                string output = commandLineOptions.Output;
                if (string.IsNullOrEmpty(output) == false)
                {
                    Settings.Default.Output = output;
                }
            }

            string[] itemsToBeZipped = Settings.Default.Input.Split(',');

            List<string> filesToBeZipped = new List<string>();

            foreach (string itemToBeZipped in itemsToBeZipped)
            {
                log.Info("Item to be zipped = " + itemToBeZipped);
                try
                {
                    if (itemToBeZipped.StartsWith(".\\", StringComparison.OrdinalIgnoreCase))
                    {
                        HandlePattern(ref filesToBeZipped, itemToBeZipped, currentAssemblyDirectoryName);
                    }
                    else
                    {
                        string expandedFolder = Path.GetDirectoryName(itemToBeZipped);
                        string trimmedItemToBeZipped = Path.GetFileName(itemToBeZipped);
                        HandlePattern(ref filesToBeZipped, trimmedItemToBeZipped, expandedFolder);
                    }
                }
                catch(Exception exception)
                {
                    log.Error("Pattern handling exception = " + exception.Message);
                }                
            }

            using (ZipFile outputZipFile = new ZipFile(currentAssemblyDirectoryName + "\\" + Settings.Default.Output))
            {
                foreach (string fileToBeZipped in filesToBeZipped)
                {
                    if (File.Exists(fileToBeZipped))
                    {
                        try
                        {
                            outputZipFile.AddFile(fileToBeZipped);
                        }
                        catch(Exception exception)
                        {
                            log.Error("Zipping exception = " + exception.Message);
                        }
                    }
                    else
                    {
                        log.Warn("FILE NOT FOUND = " + fileToBeZipped);
                    }
                }

                outputZipFile.Save();
            }
        }

        private static void HandlePattern(ref List<string> filesToBeZipped, string itemToBeZipped, string pathPrefix)
        {
            if (itemToBeZipped.Contains("*"))
            {
                if (Settings.Default.Latest == true)
                {
                    string latestFile = GetLatestFile(itemToBeZipped, pathPrefix);
                    if (string.IsNullOrEmpty(latestFile) == false)
                    {
                        filesToBeZipped.Add(latestFile);
                    }
                }
                else
                {
                    string[] patternMatchingFiles = GetAllFiles(itemToBeZipped);
                    foreach (string patternMatchingFile in patternMatchingFiles)
                    {
                        filesToBeZipped.Add(patternMatchingFile);
                    }
                }
            }
            else
            {
                filesToBeZipped.Add(pathPrefix + "\\" + itemToBeZipped);
            }
        }

        private static string[] GetAllFiles(string itemToBeZipped)
        {
            string[] allFiles = Directory.GetFiles(currentAssemblyDirectoryName, itemToBeZipped);
            return allFiles;
        }

        private static string GetLatestFile(string pattern, string pathPrefix)
        {
            string latestFile = string.Empty;
            if (Directory.Exists(pathPrefix))
            {
                DirectoryInfo di = new DirectoryInfo(pathPrefix);
                var latestFileInDirectory = di.GetFiles(pattern)
                                              .OrderByDescending(x => x.LastWriteTime)
                                              .Take(1);
                latestFile = latestFileInDirectory.First().FullName;
            }
            return latestFile;
        }
    }
}
