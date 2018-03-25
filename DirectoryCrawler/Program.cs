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
    internal class Program
    {
        private static string targetdir;
        private static int notAuthorizedCount;
        private static int accessibleCount;

        private static void Main(string[] args)
        {
            Console.Title = "Directory Crawler " + Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
            ConsoleHelper.FixEncoding();

            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    if (arg.Contains("/targetdir="))
                    {
                        string[] extract = arg.Split('=');
                        targetdir = extract[1];
                        goto START;
                    }
                }
            }
            else
            {
                Console.WriteLine();
                ConsoleHelper.WriteLine("    ==========================", ConsoleColor.Green);
                ConsoleHelper.WriteLine(string.Format("    | Directory Crawler v{0} |", Assembly.GetExecutingAssembly().GetName().Version.ToString(2)), ConsoleColor.Green);
                ConsoleHelper.WriteLine("    ==========================", ConsoleColor.Green);
                Console.WriteLine();
                Console.WriteLine("    Please run this program with the following command-line:");
                Console.WriteLine();
                ConsoleHelper.WriteWrapped("      .\\DirectoryCrawler.exe /targetdir=<PATH>", true);
                Console.WriteLine();
                ConsoleHelper.WriteWrapped("      NOTE: Please quote the long path or path that contains spacing.", true);
                Console.WriteLine();
                ConsoleHelper.Wait();
                Environment.Exit(0);
            }

            START:
            if (!Directory.Exists(targetdir))
            {
                ConsoleHelper.WriteLine("ERROR: targetdir does not exist.", ConsoleColor.Red);
                Environment.Exit(1);
            }

            // Generate filename
            string dirName = new DirectoryInfo(targetdir).Name.Replace(" ", "_");
            if (dirName.Length > 10)
                dirName = dirName.Substring(0, 10);

            string filename = dirName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            accessibleCount = 0;
            notAuthorizedCount = 0;
            List<string> accessibleDirs = new List<string>();

            Stopwatch sw1 = new Stopwatch();
            Stopwatch sw2 = new Stopwatch();

            ConsoleHelper.WriteLine("    Start crawling accessible directories...", ConsoleColor.Green);
            sw1.Start();
            Task t1 = Task.Factory.StartNew(() =>
            {
                accessibleDirs = GetDirs(targetdir, "*", SearchOption.AllDirectories);
            });
            t1.Wait();
            sw1.Stop();
            ConsoleHelper.WriteLine("    Crawling complete.", ConsoleColor.Green);
            Console.WriteLine();
            ConsoleHelper.WriteLine("    Writing all accessible directories to output file...", ConsoleColor.Green);
            sw2.Start();
            Task t2 = Task.Factory.StartNew(() =>
            {
                TextWriter tw = new StreamWriter(filename + "_OUTPUT.txt");
                foreach (var dir in accessibleDirs)
                {
                    tw.WriteLine(dir);
                    accessibleCount++;
                }
                tw.Close();
            });
            t2.Wait();
            sw2.Stop();
            ConsoleHelper.WriteLine("    OUTPUT FILENAME: " + filename + "_OUTPUT.txt", ConsoleColor.Cyan);

            // Output text
            string content = string.Format("TARGETDIR: {0}{1}CRAWLTIME: {2} ms{3}WRITETIME: {4} ms{5}DIRCOUNT: {6}{7}NOTAUTHORIZED_DIRCOUNT: {8}",
                targetdir, Environment.NewLine,
                sw1.ElapsedMilliseconds, Environment.NewLine,
                sw2.ElapsedMilliseconds, Environment.NewLine,
                accessibleCount, Environment.NewLine,
                notAuthorizedCount);
            File.WriteAllText(filename + "_INFO.txt", content);
            Console.WriteLine();
            ConsoleHelper.WriteLine("    CRAWLING INFO", ConsoleColor.Yellow);
            ConsoleHelper.WriteLine("    =============", ConsoleColor.Yellow);
            Console.WriteLine("    Crawling time taken: {0} ms", sw1.ElapsedMilliseconds);
            Console.WriteLine("    Writing to output file time taken: {0} ms", sw2.ElapsedMilliseconds);
            Console.WriteLine("    Accessible directory count: {0}", accessibleCount);
            Console.WriteLine("    Inaccessible directry count: {0}", notAuthorizedCount);
        }

        private static List<string> GetDirs(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
                return Directory.GetDirectories(path, searchPattern).ToList();

            var dirs = new List<string>(GetDirs(path, searchPattern));
            for (var i = 0; i < dirs.Count; i++)
            {
                dirs.AddRange(GetDirs(dirs[i], searchPattern));
                Console.WriteLine(dirs[i]);
            }
            return dirs;
        }

        private static List<string> GetDirs(string path, string searchPattern)
        {
            try
            {
                return Directory.GetDirectories(path, searchPattern).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                notAuthorizedCount++;
                return new List<string>();
            }
        }
    }
}