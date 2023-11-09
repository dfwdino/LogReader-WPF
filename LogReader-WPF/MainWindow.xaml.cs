using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static LogReader_WPF.LogFileErrorColorModel;

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

        public delegate void AddTextToRowCallback(string message);

        private void AddTextToRow(string message)
        {
            foreach (var item in message.Split(Environment.NewLine))
            {
                DataGridRow dgr = new DataGridRow();

                dgr.Item = item;

                dgr.Background = GetRowColor(item);

                LogFileData.Items.Add(dgr);

            }
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



            //await Task.Run(() =>
            //{
            //    OpenFileLog();
            //});
            LogFileLocation.Text = FileLocation;
            OpenFileLog(FileLocation);
        }

        private void OpenFileLog(string filelocation)


        private void LoadDataGrid(string filedata)
        {
            LogFileData.Items.Clear();

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate () { AddTextToRow(filedata); }));

            //foreach (var item in filedata.Split(Environment.NewLine))
            //{
            //    DataGridRow dgr = new DataGridRow();

            //    dgr.Item = item;

            //    dgr.Background = GetRowColor(item);

            //    LogFileData.Items.Add(dgr);
                
            //}

            StatusBar.Text = $"Log file loaded {LogFileLocation.Text}.";

            WarningNumber.Text = _WarningNumber.ToString();
            ErrorNumber.Text = _ErrorNumber.ToString();

            _ErrorNumber = 0;
            _WarningNumber = 0;
        }

        private void OpenFileLog(string logfilelocation)
        {
            System.IO.StreamReader myFile = null;
            bool FoundHistory = false;


            if (filelocation.Length.Equals(0))
            {
                return;
            }

            string filename = System.IO.Path.GetFileNameWithoutExtension(LogFileLocation.Text);

            try
            {
                myFile = new System.IO.StreamReader(filelocation);
                

                foreach (MenuItem item in MenuHistory.Items)
                {
                     if(item.Header.ToString() == filename)
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

                        

                    MenuHistory.Items.Add(menuItem);

                }

                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return;
            }

            string myString = myFile.ReadToEnd();

            myFile.Close();

            LoadDataGrid(myString);

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

            AppErrorColors = JsonConvert.DeserializeObject<LogFileErrorColorModel>(AppSettingsJson);
        }

        private void DataGridFile_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                LogFileLocation.Text = files[0];

                //WTF need to make this a function
                bool FoundHistory = false;
                string filename = System.IO.Path.GetFileNameWithoutExtension(files[0]);

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

                    MenuHistory.Items.Add(menuItem);

                }

                OpenFileLog(files[0]);


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

            foreach (DataGridRow dgr in LogFileData.Items)
            {
                if (dgr.Item.ToString().IndexOf(SearchBox.Text) < 0)
                {
                    dgr.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ClearSearch(object sender, RoutedEventArgs e)
        {
            ShowAllRows();
        }

        private void ShowAllRows()
        {
            foreach (DataGridRow dgr in LogFileData.Items)
            {
                dgr.Visibility = Visibility.Visible;
            }
        }
    }
}
