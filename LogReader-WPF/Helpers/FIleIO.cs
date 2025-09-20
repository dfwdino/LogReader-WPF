using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogReader_WPF
{
    internal class FIleIO
    {
        public static string OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : string.Empty;
        }

        public static void AddToHistoryFile(string filepath)
        {
            try
            {
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var folder = System.IO.Path.Combine(appdata, "LogReader-WPF");
                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }
                var historyfile = System.IO.Path.Combine(folder, "history.txt");
                List<string> historylist = new();
                //if (System.IO.File.Exists(historyfile))
                //{
                //    historylist = System.IO.File.ReadAllLines(historyfile).Take(10).ToList();
                //}
                if (!historylist.Contains(filepath))
                {
                    historylist.Add(filepath);
                    System.IO.File.WriteAllLines(historyfile, historylist.Distinct().TakeLast(10));
                }
            }
            catch
            {
                // Do nothing
            }
        }

        public static List<string> GetHistoryFile()
        {
            try
            {
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var folder = System.IO.Path.Combine(appdata, "LogReader-WPF");
                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }
                var historyfile = System.IO.Path.Combine(folder, "history.txt");
                List<string> historylist = new();
                if (System.IO.File.Exists(historyfile))
                {
                    historylist = System.IO.File.ReadAllLines(historyfile).ToList();
                }
                return historylist;
            }
            catch
            {
                return new List<string>();
            }
        }

    }
}
