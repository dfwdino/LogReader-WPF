using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LogReader_WPF
{
    internal class FileIO
    {
        public static string OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : string.Empty;
        }

        public static void AddToHistoryFile(IEnumerable<string> paths, string historyFilename)
        {
            try
            {
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string projectName = Assembly.GetExecutingAssembly().GetName().Name ?? "LogReader-WPF";
                string folder = Path.Combine(appdata, projectName);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string historyFile = Path.Combine(folder, historyFilename);

                File.WriteAllLines(historyFile, paths.Distinct().TakeLast(10));
            }
            catch
            {
                // Could not save history — non-critical
            }
        }

        public static List<string> GetHistoryFile(string historyFilename = "History.txt")
        {
            try
            {
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string projectName = Assembly.GetExecutingAssembly().GetName().Name ?? "LogReader-WPF";
                string folder = Path.Combine(appdata, projectName);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string historyFile = Path.Combine(folder, historyFilename);

                if (!File.Exists(historyFile))
                    return new List<string>();

                return File.ReadAllLines(historyFile)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
