using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Zipper
{
    class Program
    {
        // const string currentAssemblyDirectoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        const string currentAssemblyDirectoryName = "C:\\ProgramData\\Rodenstock\\Rodenstock Consulting\\";

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
                Console.WriteLine("itemToBeZipped = " + itemToBeZipped);

                if (itemToBeZipped.StartsWith(".\\"))
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

            ZipFile outputZipFile = new ZipFile(currentAssemblyDirectoryName + "\\" + Settings.Default.Output);

            foreach (string fileToBeZipped in filesToBeZipped)
            {
                if (File.Exists(fileToBeZipped))
                {
                    outputZipFile.AddFile(fileToBeZipped);
                }
                else
                {
                    Console.WriteLine("FILE NOT FOUND: " + fileToBeZipped);
                }
            }

            outputZipFile.Save();
        }

        private static void HandlePattern(ref List<string> filesToBeZipped, string itemToBeZipped, string pathPrefix)
        {
            if (itemToBeZipped.Contains("*"))
            {
                if (Settings.Default.Latest == true)
                {
                    filesToBeZipped.Add(GetLatestFile(itemToBeZipped, pathPrefix));
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
            DirectoryInfo di = new DirectoryInfo(pathPrefix);
            var latestFileInDirectory = di.GetFiles(pattern)
                                          .OrderByDescending(x => x.LastWriteTime)
                                          .Take(1);
            latestFile = latestFileInDirectory.First().FullName;
            return latestFile;
        }
    }
}
