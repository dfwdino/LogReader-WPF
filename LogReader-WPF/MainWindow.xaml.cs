using LogReader_WPF.Models;
using LogReader_WPF.src.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogReader_WPF
{
    public partial class MainWindow : Window
    {
        private int _errorNumber = 0;
        private int _warningNumber = 0;
        private int _totalRowCount = 0;
        private SettingsModel? _settingModel;
        private string _currentFolder = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Processes log lines on a background thread, then updates the grid on the UI thread.
        private async Task CreateEntriesForGridAsync(List<string> logfile)
        {
            List<LogEntry> logEntry = new();
            int errorCount = 0;
            int warningCount = 0;

            await Task.Run(() =>
            {
                foreach (var item in logfile)
                {
                    var clearedItem = item.Replace("\0", "");

                    bool isError = FlagWords.ErrorWords.Any(word => clearedItem.Contains(word, StringComparison.OrdinalIgnoreCase));
                    bool isWarning = FlagWords.WarningWords.Any(word => clearedItem.Contains(word, StringComparison.OrdinalIgnoreCase));

                    logEntry.Add(new LogEntry
                    {
                        Content = clearedItem,
                        IsError = isError,
                        IsWarning = isWarning
                    });

                    if (isError) errorCount++;
                    if (isWarning) warningCount++;
                }
            });

            _errorNumber = errorCount;
            _warningNumber = warningCount;

            LogFileData.ItemsSource = logEntry;
            _totalRowCount = LogFileData.Items.Count;
            RowCount.Text = _totalRowCount.ToString("N0");
        }

        private async Task LoadDataGridAsync(List<string> filedata)
        {
            LogFileData.ItemsSource = null;
            StatusBar.Text = $"Loading {Path.GetFileName(LogFileLocation.Text)}...";
            Cursor = Cursors.Wait;

            try
            {
                await CreateEntriesForGridAsync(filedata);

                WarningNumber.Text = _warningNumber.ToString("N0");
                ErrorNumber.Text = _errorNumber.ToString("N0");
                RowCount.Text = _totalRowCount.ToString("N0");
                StatusBar.Text = $"Loaded: {LogFileLocation.Text}  |  {_totalRowCount:N0} rows  |  {_errorNumber:N0} errors  |  {_warningNumber:N0} warnings";
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private async Task OpenFileLogAsync(string filelocation)
        {
            if (string.IsNullOrWhiteSpace(filelocation)) return;

            List<string> logFileText;

            try
            {
                logFileText = await ReadFileAsync(filelocation);
                LogFileLocation.Text = filelocation;
                AddHistoryEntry(filelocation);
                LoadLogFileFolder(filelocation);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBar.Text = "Failed to load file.";
                return;
            }

            await LoadDataGridAsync(logFileText);
        }

        private async Task<List<string>> ReadFileAsync(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            return content.Split(["\r\n", "\n"], StringSplitOptions.None).ToList();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = $"The Log - {RandomHelpers.GetCurrentVerion()}";

            string jsonpath = Path.Combine(Directory.GetCurrentDirectory(), "AppSettings", "AppSettings.json");
            string appSettingsJson = await File.ReadAllTextAsync(jsonpath);
            var fullConfig = JsonConvert.DeserializeObject<dynamic>(appSettingsJson);

            _settingModel = JsonConvert.DeserializeObject<SettingsModel>(fullConfig?.Settings?.ToString() ?? "{}");

            if (_settingModel?.FontSize > 0)
                LogFileData.FontSize = _settingModel.FontSize;

            foreach (string filelocation in FileIO.GetHistoryFile(_settingModel?.HistoryFilename ?? "History.txt"))
            {
                if (!string.IsNullOrWhiteSpace(filelocation))
                    AddHistoryEntry(filelocation);
            }
        }

        private void LoadLogFileFolder(string path)
        {
            string? currentFileDir = Path.GetDirectoryName(path);
            if (currentFileDir == null || _currentFolder.Equals(currentFileDir))
                return;

            FileList.Items.Clear();
            _currentFolder = currentFileDir;

            foreach (var file in Directory.GetFiles(currentFileDir))
            {
                FileList.Items.Add(new ListBoxItem
                {
                    Tag = file,
                    Content = Path.GetFileName(file)
                });
            }
        }

        private async void LoadLogFile(object sender, SelectionChangedEventArgs e)
        {
            var selectedLogFile = FileList.SelectedItem as ListBoxItem;
            if (selectedLogFile == null) return;

            string filePath = selectedLogFile.Tag?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(filePath)) return;

            LogFileLocation.Text = filePath;
            await OpenFileLogAsync(filePath);
        }

        private void DataGridFile_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0) return;

            if (files.Length > 1)
                StatusBar.Text = $"Multiple files dropped — loading first file ({files.Length - 1} others ignored).";

            string fileLocation = files[0];
            LogFileLocation.Text = fileLocation;
            _ = OpenFileLogAsync(fileLocation);
        }

        // Adds a file path to the History menu. Most-recently-opened appears at the top.
        private void AddHistoryEntry(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            bool alreadyExists = MenuHistory.Items
                .OfType<MenuItem>()
                .Any(item => item.ToolTip?.ToString() == filePath);

            if (alreadyExists) return;

            MenuItem menuItem = new MenuItem
            {
                Header = Path.GetFileNameWithoutExtension(filePath),
                ToolTip = filePath,
                Style = (Style)FindResource("SubMenuItem")
            };
            menuItem.Click += OpenLogFile_Click;

            // Insert at top so most recent appears first; separator + Clear History stay at bottom
            MenuHistory.Items.Insert(0, menuItem);
        }

        private async void OpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            string fileLocation = string.Empty;

            if (sender is MenuItem historyItem)
            {
                fileLocation = historyItem.ToolTip?.ToString() ?? string.Empty;
            }
            else
            {
                fileLocation = FileIO.OpenFileDialog();
            }

            if (string.IsNullOrEmpty(fileLocation)) return;

            LogFileLocation.Text = fileLocation;
            await OpenFileLogAsync(fileLocation);
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            for (int i = MenuHistory.Items.Count - 1; i >= 0; i--)
            {
                var item = MenuHistory.Items[i];
                if (item is Separator || (item is MenuItem mi && mi.Tag?.ToString() == "ClearHistory"))
                    continue;
                MenuHistory.Items.RemoveAt(i);
            }
            StatusBar.Text = "History cleared.";
        }

        private void OpenAbout(object sender, RoutedEventArgs e)
        {
            About about = new About { Owner = this };
            about.ShowDialog();
        }

        private void SearchGrid(object? sender, RoutedEventArgs? e)
        {
            ShowAllRows();

            string searchText = SearchBox.Text;

            if (!string.IsNullOrEmpty(searchText))
            {
                LogFileData.Items.Filter = item =>
                {
                    var logEntry = item as LogEntry;
                    return logEntry != null && logEntry.Content.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                };

                int filteredCount = LogFileData.Items.Count;
                RowCount.Text = $"{filteredCount:N0} / {_totalRowCount:N0}";
                StatusBar.Text = $"Filter \"{searchText}\": {filteredCount:N0} of {_totalRowCount:N0} rows match.";
            }
        }

        private void ClearSearch(object? sender, RoutedEventArgs? e)
        {
            SearchBox.Text = string.Empty;
            ShowAllRows();
            StatusBar.Text = string.Empty;
        }

        private void ShowAllRows()
        {
            LogFileData.Items.Filter = null;
            RowCount.Text = _totalRowCount.ToString("N0");
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SearchGrid(sender, null);
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                ClearSearch(sender, null);
            }
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                e.Handled = true;
                if (!string.IsNullOrEmpty(LogFileLocation.Text))
                    await OpenFileLogAsync(LogFileLocation.Text);
                return;
            }

            if (Keyboard.Modifiers != ModifierKeys.Control) return;

            if (e.Key == Key.O)
            {
                e.Handled = true;
                string fileLocation = FileIO.OpenFileDialog();
                if (!string.IsNullOrEmpty(fileLocation))
                {
                    LogFileLocation.Text = fileLocation;
                    await OpenFileLogAsync(fileLocation);
                }
            }
            else if (e.Key == Key.F)
            {
                e.Handled = true;
                SearchBox.Focus();
                SearchBox.SelectAll();
            }
        }

        private async void ReloadLogFile_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(LogFileLocation.Text))
                await OpenFileLogAsync(LogFileLocation.Text);
        }

        private void HideShowFolderList(object sender, RoutedEventArgs e)
        {
            var fileListColumn = FileListGrid.ColumnDefinitions[0];

            if (FileListPanel.Visibility == Visibility.Collapsed)
            {
                FileListPanel.Visibility = Visibility.Visible;
                FileListSplitter.Visibility = Visibility.Visible;
                fileListColumn.MinWidth = 250;
                fileListColumn.Width = GridLength.Auto;
                ShowHideFileList.Content = "Hide File List";
            }
            else
            {
                FileListPanel.Visibility = Visibility.Collapsed;
                FileListSplitter.Visibility = Visibility.Collapsed;
                fileListColumn.MinWidth = 0;
                fileListColumn.Width = new GridLength(0);
                ShowHideFileList.Content = "Show File List";
            }
        }

        private void ErrorBadge_Click(object sender, MouseButtonEventArgs e)
        {
            SearchBox.Text = string.Empty;
            LogFileData.Items.Filter = item => item is LogEntry entry && entry.IsError;
            int filteredCount = LogFileData.Items.Count;
            RowCount.Text = $"{filteredCount:N0} / {_totalRowCount:N0}";
            StatusBar.Text = $"Showing errors: {filteredCount:N0} of {_totalRowCount:N0} rows.";
        }

        private void WarningBadge_Click(object sender, MouseButtonEventArgs e)
        {
            SearchBox.Text = string.Empty;
            LogFileData.Items.Filter = item => item is LogEntry entry && entry.IsWarning;
            int filteredCount = LogFileData.Items.Count;
            RowCount.Text = $"{filteredCount:N0} / {_totalRowCount:N0}";
            StatusBar.Text = $"Showing warnings: {filteredCount:N0} of {_totalRowCount:N0} rows.";
        }

        private void CopyRowContent_Click(object sender, RoutedEventArgs e)
        {
            if (LogFileData.SelectedItem is LogEntry entry)
                Clipboard.SetText(entry.Content);
        }

        private void CopyAllContent_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var item in LogFileData.Items)
            {
                if (item is LogEntry entry)
                    sb.AppendLine(entry.Content);
            }
            if (sb.Length > 0)
                Clipboard.SetText(sb.ToString());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var paths = MenuHistory.Items
                .OfType<MenuItem>()
                .Where(item => item.ToolTip != null && item.Tag?.ToString() != "ClearHistory")
                .Select(item => item.ToolTip.ToString() ?? string.Empty)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            FileIO.AddToHistoryFile(paths, _settingModel?.HistoryFilename ?? "History.txt");
        }
    }
}
