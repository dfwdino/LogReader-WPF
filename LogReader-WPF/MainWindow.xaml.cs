using LogReader_WPF.Model;
using LogReader_WPF.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        //public delegate void AddTextToRowCallback(string message);

        private async Task AddTextToRowAsync(List<string> logfile)
        {
            List<LogEntry> logEntry = new();

            //int errorCount = 0;
            //int warningCount = 0;

            foreach (var item in logfile)
            {
                bool isError = FlagWords.ErrorWords.Any(word => item.Contains(word));
                bool isWarning = FlagWords.WarningWords.Any(word => item.Contains(word));

                logEntry.Add(new LogEntry
                {
                    Content = item,
                    IsError = isError,
                    IsWarning = isWarning
                });

                if (isError) _ErrorNumber++;
                if (isWarning) _WarningNumber++;
            }



            await Dispatcher.InvokeAsync(() =>
            {
                LogFileData.ItemsSource = logEntry;
            });

        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            bool IsMenuType = sender.GetType().Name == typeof(MenuItem).Name;
            string FileLocation = string.Empty;

            if (IsMenuType)
            {
                MenuItem historyitem = (MenuItem)sender;
                FileLocation = historyitem.ToolTip.ToString();
            }
            else
            {
                FileLocation = FIleIO.OpenFileDialog();
                LogFileData.Items.Clear();
                StatusBar.Text = $"Loading file {LogFileLocation.Text}.";

                WarningNumber.Text = _WarningNumber.ToString();
                ErrorNumber.Text = _ErrorNumber.ToString();
            }

            LogFileLocation.Text = FileLocation;
            OpenFileLogAsync(FileLocation);
        }

        private async Task LoadDataGridAsync(List<string> filedata)
        {
            LogFileData.Items.Clear();

            await Task.Run(() =>
            {
                AddTextToRowAsync(filedata);
            });

            StatusBar.Text = $"Log file loaded {LogFileLocation.Text}.";

            WarningNumber.Text = _WarningNumber.ToString();
            ErrorNumber.Text = _ErrorNumber.ToString();

            _ErrorNumber = 0;
            _WarningNumber = 0;
        }

        private async Task OpenFileLogAsync(string filelocation)
        {
            List<string> LogFileText = new List<string>();

            if (filelocation.Length.Equals(0))
            {
                return;
            }

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

            LoadDataGridAsync(LogFileText);
        }

        private async Task<List<string>> ReadFileAsync(string filePath)
        {
            return await Task.Run(() => File.ReadAllLines(filePath).ToList());
        }

        private SolidColorBrush GetRowColor(string data)
        {
            //There has to be a better and faster way.
            foreach (var errorcolor in AppErrorColors.ErrorColors)
            {
                if (data.Contains(errorcolor.ErrorName))
                {
                    try
                    {
                        switch (errorcolor.ErrorName)
                        {
                            case "Error":
                                _ErrorNumber++;
                                break;

                            case "Warning":
                                _WarningNumber++;
                                break;

                            default:
                                break;
                        }

                        return new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString(errorcolor.ErrorColor)
                        );
                    }
                    catch (Exception ex)
                    {
                        return new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("Error")
                        );
                    }
                }
            }

            if (OddRow)
            {
                OddRow = false;
                return ErrorColors.EvenRowColor;
            }
            else
            {
                OddRow = true;
                return ErrorColors.OddRowColor;
            }
            //}

            //return ErrorColors.EvenRowColor;
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


        }



        private void LoadLogFileFolder(string path)
        {
            string currentfiledir = Path.GetDirectoryName(path);

            if (!CurrentFolder.Equals(currentfiledir))
            {
                FileList.Items.Clear();
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
                menuItem.ToolTip = LogFileLocation.Text;

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

            string searchtext = string.Empty;

            if (sender.GetType() == typeof(Label))
            {
                string valuetext = ((Label)sender).Tag.ToString();
                searchtext = valuetext;
            }
            else
            {
                searchtext = SearchBox.Text;
            }



            foreach (DataGridRow dgr in LogFileData.Items)
            {
                if (dgr.Item.ToString().IndexOf(searchtext, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    dgr.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ClearSearch(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            ShowAllRows();
        }

        private void ShowAllRows()
        {
            foreach (DataGridRow dgr in LogFileData.Items)
            {
                dgr.Visibility = Visibility.Visible;
            }
        }

        private void HideShowFolderList(object sender, RoutedEventArgs e)
        {
            double adsf = FileList.Width;

            if (FileList.Visibility == Visibility.Collapsed)
            {
                FileList.Visibility = Visibility.Visible;
                ShowHideFileList.Content = "Hide ListBox";

            }
            else
            {
                FileList.Visibility = Visibility.Collapsed;
                LogFileData.VerticalAlignment = VerticalAlignment.Stretch;
                ShowHideFileList.Content = "Show ListBox";

            }
        }
    }
}