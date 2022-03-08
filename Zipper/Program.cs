namespace Zipper
{
    using CommandLine;
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;

    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        static string currentAssemblyDirectoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(RunOptions);            
        }

        private static void RunOptions(CommandLineOptions commandLineOptions)
        {
            string[] itemsToBeZipped = commandLineOptions.Input.Split(',');

            List<string> filesToBeZipped = new List<string>();

            foreach (string itemToBeZipped in itemsToBeZipped)
            {
                logger.Info("Item to be zipped = " + itemToBeZipped);
                try
                {
                    if (itemToBeZipped.StartsWith(".\\", StringComparison.OrdinalIgnoreCase))
                    {
                        HandlePattern(ref filesToBeZipped, itemToBeZipped, currentAssemblyDirectoryName, commandLineOptions.Latest);
                    }
                    else
                    {
                        string expandedFolder = Path.GetDirectoryName(itemToBeZipped);
                        string trimmedItemToBeZipped = Path.GetFileName(itemToBeZipped);
                        HandlePattern(ref filesToBeZipped, trimmedItemToBeZipped, expandedFolder, commandLineOptions.Latest);
                    }
                }
                catch (Exception exception)
                {
                    logger.Error("Pattern handling exception = " + exception.Message);
                }
            }

            using (ZipArchive outputZipFile = ZipFile.Open(commandLineOptions.Output, ZipArchiveMode.Create))
            {
                foreach (string fileToBeZipped in filesToBeZipped)
                {
                    if (File.Exists(fileToBeZipped))
                    {
                        try
                        {
                            outputZipFile.CreateEntryFromFile(fileToBeZipped, Path.GetFileName(fileToBeZipped));
                        }
                        catch (Exception exception)
                        {
                            logger.Error("Zipping exception = " + exception.Message);
                        }
                    }
                    else
                    {
                        logger.Warn("FILE NOT FOUND = " + fileToBeZipped);
                    }
                }
            }
        }

        private static void HandlePattern(ref List<string> filesToBeZipped, string itemToBeZipped, string pathPrefix, bool latest)
        {
            if (itemToBeZipped.Contains("*"))
            {
                if (latest == true)
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
