using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unclassified.Util;

namespace DirectoryCrawler
{
    class Program
    {
        static List<string> DirectoryPathList;
        static int TotalFailedToAccess;

        static void Main(string[] args)
        {
            Console.Title = "DirectoryCrawler " + Assembly.GetExecutingAssembly().GetName().Version;
            ConsoleHelper.FixEncoding();

            Console.WriteLine(Console.Title);

            Block1:
            Console.WriteLine();
            Console.Write("Enter directory path to scan: ");
            string insertPath = Console.ReadLine();

            if (insertPath.Length == 0)
            {
                ConsoleHelper.Write("[ERROR]", ConsoleColor.Black, ConsoleColor.Red);
                Console.Write(" ");
                ConsoleHelper.Write("Directory path is empty.", ConsoleColor.Red);
                Console.WriteLine();
                goto Block1;
            }
            else if (!Directory.Exists(insertPath))
            {
                ConsoleHelper.Write("[ERROR]", ConsoleColor.Black, ConsoleColor.Red);
                Console.Write(" ");
                ConsoleHelper.Write("Invalid directory path.", ConsoleColor.Red);
                Console.WriteLine();
                goto Block1;
            }

            Console.Clear();
            Console.WriteLine(Console.Title);

            Console.WriteLine();
            ConsoleHelper.Write("--> ", ConsoleColor.Green);
            Console.Write(insertPath);
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Crawling will be started in 5 seconds...");
            Thread.Sleep(5000);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Console.WriteLine();

            // Start the Task
            Task t = GetDirectoriesTask(insertPath);

            // Wait for the task to complete
            t.Wait();

            Console.WriteLine();
            Console.WriteLine("Crawling Statistics:");
            ConsoleHelper.WriteLine(string.Format("  Total directories found  : {0}", DirectoryPathList.Count), ConsoleColor.Green);
            ConsoleHelper.WriteLine(string.Format("  Total failed to access   : {0}", TotalFailedToAccess), ConsoleColor.Red);

            Console.WriteLine();
            Console.WriteLine("Creating output file...");
            string exMsg = "";
            string outputFilePath = "";
            bool isOutputFileCreated = false;

            try
            {
                string dirName = new DirectoryInfo(insertPath).Name;
                dirName = dirName.Replace(" ", "_");
                if (dirName.Length > 10)
                    dirName = dirName.Substring(0, 10);

                string filename = dirName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                outputFilePath = AppDomain.CurrentDomain.BaseDirectory + filename;

                foreach (var directoryPath in DirectoryPathList)
                    isOutputFileCreated = WriteToFile(outputFilePath, directoryPath, out exMsg);
            }
            catch (Exception ex)
            {
                isOutputFileCreated = false;
                exMsg = ex.ToString();
            }

            if (isOutputFileCreated)
            {
                ConsoleHelper.Write("[SUCCESS]", ConsoleColor.Black, ConsoleColor.Green);
                Console.Write(" ");
                ConsoleHelper.Write(outputFilePath, ConsoleColor.Green);
                Console.WriteLine();
            }
            else
            {
                ConsoleHelper.Write("[ERROR]", ConsoleColor.Black, ConsoleColor.Red);
                Console.Write(" ");
                ConsoleHelper.Write("Failed to create the output file: " + exMsg, ConsoleColor.Red);
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);

            Console.WriteLine();
            ConsoleHelper.Wait();
        }

        #region Tasker
        static Task GetDirectoriesTask(string targetPath, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Task.Factory.StartNew(() => {
                DirectoryPathList = GetDirectories(targetPath, searchPattern, searchOption);
            });
        }
        #endregion

        #region Spinner
        public static int spinnerPos = 0;
        public delegate void UpdateProgressDelegate(float pctComplete, int complete = 0, int total = 0);
        public static UpdateProgressDelegate UpdateProgress = (float pctComplete, int complete, int total) => {
            if (pctComplete >= 100f)
            {
                ConsoleHelper.Write("\rCrawling Complete!".PadRight(Console.BufferWidth), ConsoleColor.Black, ConsoleColor.Green);
                Console.Write(Environment.NewLine);
            }
            else
            {
                char[] spinner = new char[] { '-', '\\', '|', '/' };
                ConsoleHelper.Write(string.Format("\rCrawling directories... ({0}/{1}) {2} - {3:0.00}%", complete, total, spinner[spinnerPos], pctComplete).PadRight(Console.BufferWidth), ConsoleColor.White, ConsoleColor.Red);
                spinnerPos = (spinnerPos >= 3) ? 0 : spinnerPos + 1;
            }
        };
        #endregion

        #region Directory Listing Methods
        public static List<string> GetDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Show the starting progress
            UpdateProgress(0f);

            if (searchOption == SearchOption.TopDirectoryOnly)
                return Directory.GetDirectories(path, searchPattern).ToList();

            var directories = new List<string>(GetDirectories(path, searchPattern));

            for (var i = 0; i < directories.Count; i++)
            {
                directories.AddRange(GetDirectories(directories[i], searchPattern));

                // Update the progress
                float pctComplete = (((float) (i + 1) / (float) directories.Count) * 100f);
                UpdateProgress(pctComplete, (i + 1), directories.Count);
            }

            return directories;
        }

        private static List<string> GetDirectories(string path, string searchPattern)
        {
            try
            {
                return Directory.GetDirectories(path, searchPattern).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                TotalFailedToAccess++;
                return new List<string>();
            }
            catch (PathTooLongException)
            {
                TotalFailedToAccess++;
                return new List<string>();
            }
        }
        #endregion

        #region WriteToFile Method
        static bool WriteToFile(string filePath, string text, out string exMsg)
        {
            if (!File.Exists(filePath))
            {
                try
                {
                    File.WriteAllText(filePath, text);
                }
                catch (Exception ex)
                {
                    exMsg = ex.ToString();
                    return false;
                }
            }

            using (StreamWriter file = new StreamWriter(filePath, true, Encoding.ASCII))
            {
                file.WriteLine(text);
                exMsg = "";
                return true;
            }
        }
        #endregion
    }
}
