using LogReader_WPF.Model;
using LogReader_WPF.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LogReader_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _ErrorNumber = 0;
        private int _WarningNumber = 0;
        private bool OddRow = false;
        private LogFileErrorColorModel AppErrorColors;
        private SettingsModel SettingModel;
        private string CurrentFolder = string.Empty;



        private async Task CreateEntrysForGridAsync(List<string> logfile)
        {
            List<LogEntry> logEntry = new();

            foreach (var item in logfile)
            {
                var cleareditem = item.Replace("\0", "");

                bool isError = FlagWords.ErrorWords.Any(word => cleareditem.Contains(word, StringComparison.OrdinalIgnoreCase));
                bool isWarning = FlagWords.WarningWords.Any(word => cleareditem.Contains(word, StringComparison.OrdinalIgnoreCase));

                logEntry.Add(new LogEntry
                {
                    Content = cleareditem,
                    IsError = isError,
                    IsWarning = isWarning
                });

                if (isError) _ErrorNumber++;
                if (isWarning) _WarningNumber++;
            }

            await Dispatcher.InvokeAsync(() =>
            {
                LogFileData.ItemsSource = logEntry;
                RowCoount.Text = LogFileData.Items.Count.ToString();
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            string FileLocation = string.Empty;


            if (sender is MenuItem historyitem)
            {
                FileLocation = historyitem.ToolTip.ToString();

            }
            else
            {
                FileLocation = FIleIO.OpenFileDialog();
                LogFileData.ItemsSource = null;
                StatusBar.Text = $"Loading file {LogFileLocation.Text}.";
                //FIleIO.AddToHistoryFile(FileLocation);

            }

            LogFileLocation.Text = FileLocation;
            await OpenFileLogAsync(FileLocation);
        }

        private async Task LoadDataGridAsync(List<string> filedata)
        {
            LogFileData.ItemsSource = null;

            await Task.Run(() =>
            {
                CreateEntrysForGridAsync(filedata);

            });


            StatusBar.Text = $"Log file loaded {LogFileLocation.Text}.";
            WarningNumber.Text = _WarningNumber.ToString();
            ErrorNumber.Text = _ErrorNumber.ToString();
            RowCoount.Text = LogFileData.Items.Count.ToString();

            _ErrorNumber = 0;
            _WarningNumber = 0;


        }

        private async Task OpenFileLogAsync(string filelocation)
        {
            if (string.IsNullOrWhiteSpace(filelocation)) return;

            List<string> LogFileText = new();

            try
            {
                LogFileText = await ReadFileAsync(filelocation);

                LogFileLocation.Text = filelocation;
                AddCheckForHistoryEntry(filelocation, MenuHistory);
                LoadLogFileFolder(filelocation);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return;
            }

            await LoadDataGridAsync(LogFileText);

            RowCoount.Text = LogFileData.Items.Count.ToString();
        }

        private async Task<List<string>> ReadFileAsync(string filePath)
        {
            using var reader = new StreamReader(filePath);
            var content = await reader.ReadToEndAsync();
            return content.Split(["\r\n", "\n"], StringSplitOptions.None).ToList();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = $"The Log - {RandomHelpers.GetCurrentVerion().ToString()}";

            string jsonpath = System.IO.Path.Combine(
                Directory.GetCurrentDirectory(),
                "AppSettings",
                "AppSettings.json"
            );

            string AppSettingsJson = File.ReadAllText(jsonpath).ToString();

            SettingModel = JsonConvert.DeserializeObject<SettingsModel>(AppSettingsJson);

            AppErrorColors = JsonConvert.DeserializeObject<LogFileErrorColorModel>(AppSettingsJson);

            foreach (string filelocation in FIleIO.GetHistoryFile())
            {
                AddCheckForHistoryEntry(filelocation, MenuHistory);
            }



        }



        private void LoadLogFileFolder(string path)
        {
            string? currentfiledir = Path.GetDirectoryName(path);

            if (currentfiledir != null && !CurrentFolder.Equals(currentfiledir))
            {
                FileList.ItemsSource = null;
                CurrentFolder = currentfiledir;
                foreach (var item in Directory.GetFiles(currentfiledir))
                {
                    ListBoxItem listBoxItem = new ListBoxItem();

                    listBoxItem.Tag = item;
                    listBoxItem.Content = Path.GetFileName(item);

                    FileList.Items.Add(listBoxItem);
                }
            }
        }


        private void LoadLogFile(object sender, SelectionChangedEventArgs e)
        {

            if (((ListBox)sender).Items.Count > 0)//BAD code?????
            {

                var selectedLogFile = (ListBoxItem)FileList.SelectedItem;

                LogFileLocation.Text = selectedLogFile.Tag.ToString();

                AddCheckForHistoryEntry(selectedLogFile.Tag.ToString(), MenuHistory);

                //FIleIO.AddToHistoryFile(LogFileLocation.Text); //Need to move this to close window event. 

                OpenFileLogAsync(selectedLogFile.Tag.ToString());


            }

        }


        private void DataGridFile_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                string FileLocation = files[0];

                LogFileLocation.Text = FileLocation;

                AddCheckForHistoryEntry(FileLocation, MenuHistory);

                OpenFileLogAsync(FileLocation);
            }
        }

        private void AddCheckForHistoryEntry(string HistoryEntry, MenuItem HistoryMenu)
        {
            bool FoundHistory = false;
            string filename = System.IO.Path.GetFileNameWithoutExtension(HistoryEntry);

            foreach (MenuItem item in MenuHistory.Items)
            {
                if (item.Header.ToString() == filename)
                {
                    FoundHistory = true;
                    break;
                }
            }

            if (FoundHistory.Equals(false))
            {
                MenuItem menuItem = new MenuItem();

                menuItem.Header = filename;
                menuItem.ToolTip = HistoryEntry; //LogFileLocation.Text;

                HistoryMenu.Items.Add(menuItem);
            }
        }

        private void OpenAbout(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Owner = this;
            var resutls = about.ShowDialog();
        }

        private async void SearchGrid(object sender, RoutedEventArgs e)
        {
            ShowAllRows();

            string valuetext = SearchBox.Text;

            if (!string.IsNullOrEmpty(valuetext))
            {
                LogFileData.Items.Filter = item =>
                {
                    var logEntry = item as LogEntry;
                    return logEntry != null && logEntry.Content.Contains(valuetext, StringComparison.OrdinalIgnoreCase);
                };
            }

            RowCoount.Text = LogFileData.Items.Count.ToString();

        }

        private void ClearSearch(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            ShowAllRows();
            RowCoount.Text = LogFileData.Items.Count.ToString();
        }

        private void ShowAllRows()
        {
            LogFileData.Items.Filter = null;
            RowCoount.Text = LogFileData.Items.Count.ToString();
        }

        private void HideShowFolderList(object sender, RoutedEventArgs e)
        {
            double adsf = FileList.Width;



            if (FileList.Visibility == Visibility.Collapsed)
            {
                FileList.Visibility = Visibility.Visible;
                FileListGrid.Visibility = Visibility.Visible;

                ShowHideFileList.Content = "Hide ListBox";

            }
            else
            {
                FileList.Visibility = Visibility.Collapsed;
                FileListGrid.Visibility = Visibility.Collapsed;
                LogFileData.VerticalAlignment = VerticalAlignment.Stretch;
                ShowHideFileList.Content = "Show ListBox";

            }
        }

        private void ErrorLabel_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SearchBox.Text = "error";
            SearchGrid(null, null);
        }

        private void WarningLabel_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SearchBox.Text = "warning";
            SearchGrid(null, null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Need to loop thur the history and save it to file.
            StringBuilder sb = new StringBuilder();

            foreach (MenuItem item in MenuHistory.Items)
            {
                sb.AppendLine(item.ToolTip.ToString());
            }

            FIleIO.AddToHistoryFile(sb.ToString());
        }
    }
}