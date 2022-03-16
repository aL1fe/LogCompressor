using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;

namespace LogCompressor
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string pathSharpLogs = ConfigurationManager.AppSettings.Get("pathSharpLogs");
            string pathZebraLogs = ConfigurationManager.AppSettings.Get("pathZebraLogs");
            
            string pathArchiveLogsDirectory = ConfigurationManager.AppSettings.Get("pathArchiveLogsDirectory");
            
            string fileArchiveNameSharpLogs = ConfigurationManager.AppSettings.Get("fileArchiveNameSharpLogs");
            string archivePathSharpLogs = $"{pathArchiveLogsDirectory}\\{fileArchiveNameSharpLogs}";
            
            string fileArchiveNameZebraLogs = ConfigurationManager.AppSettings.Get("fileArchiveNameZebraLogs");
            string archivePathZebraLogs = $"{pathArchiveLogsDirectory}\\{fileArchiveNameZebraLogs}";

            double deltaDays = double.Parse(ConfigurationManager.AppSettings.Get("deltaDays"));
            DateTime fileDate = DateTime.Now.AddDays(-deltaDays);

            try
            {
                if (!Directory.Exists(pathArchiveLogsDirectory))
                {
                    Directory.CreateDirectory(pathArchiveLogsDirectory);
                    File.Create(archivePathSharpLogs).Close();
                    File.Create(archivePathZebraLogs).Close();
                }
                else
                {
                    if (!File.Exists(archivePathSharpLogs)) File.Create(archivePathSharpLogs).Close();
                    if (!File.Exists(archivePathZebraLogs)) File.Create(archivePathZebraLogs).Close();
                }

                ArchiveLogs(archivePathSharpLogs, pathSharpLogs, fileDate);
                ArchiveLogs(archivePathZebraLogs, pathZebraLogs, fileDate);

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void ArchiveLogs(string archivePath, string path, DateTime fileDate)
        {
            int i = 0;
            
            using (ZipArchive zipArchive = ZipFile.Open(archivePath, ZipArchiveMode.Update))
            {
                foreach (string pathFileToAdd in TraverseTree(path))
                {
                    try
                    {
                        FileInfo fi = new FileInfo(pathFileToAdd);
                        if (fi.LastWriteTime < fileDate)
                        {
                            zipArchive.CreateEntryFromFile(pathFileToAdd, fi.Name);
                            Console.WriteLine($"{++i} {fi.Name}, {fi.LastWriteTime}");
                            fi.Delete();
                            
                            //TODO delete empty folder???
                        }
                    }
                    catch (FileNotFoundException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        //The method returns all file paths that contain "/Logs" in their "path" 
        private static List<string> TraverseTree(string path)
        {
            Stack<string> dirs = new Stack<string>();
            List<string> allFiles = new List<string>();

            if (!Directory.Exists(path))
                throw new ArgumentException();

            dirs.Push(path);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;

                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                if (currentDir.Contains(@"\Logs"))
                {
                    string[] files = null;
                    try
                    {
                        files = Directory.GetFiles(currentDir);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    catch (DirectoryNotFoundException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }

                    allFiles.AddRange(files);
                }

                foreach (string str in subDirs)
                    dirs.Push(str);
            }

            return allFiles;
        }
    }
}