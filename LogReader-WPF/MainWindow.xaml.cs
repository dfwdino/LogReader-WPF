using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
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

namespace LogReader_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _ErrorNumber = 0;
        private int _WarningNumber = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        
        private async void OpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            LogFileLocation.Text = FIleIO.OpenFileDialog();
            LogFileData.Items.Clear();
            StatusBar.Text = $"Loading file {LogFileLocation.Text}.";

            WarningNumber.Text = _WarningNumber.ToString();
            ErrorNumber.Text = _ErrorNumber.ToString();

            //await Task.Run(() =>
            //{
            //    OpenFileLog();
            //});

            OpenFileLog();
        }

        private async void OpenFileLog()
        {
            
            System.IO.StreamReader myFile = null;

            try
            {
                myFile = new System.IO.StreamReader(LogFileLocation.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error {ex.Message}");
                return;
            }
            
            
            
            string myString = myFile.ReadToEnd();

            myFile.Close();

            foreach (var item in myString.Split(Environment.NewLine))
            {
                DataGridRow dgr = new DataGridRow();

                dgr.Item = item;

                dgr.Background = GetRowColor(item);

                LogFileData.Items.Add(dgr);
            }

            StatusBar.Text = $"Log file loaded {LogFileLocation.Text}.";

            WarningNumber.Text = _WarningNumber.ToString();
            ErrorNumber.Text = _ErrorNumber.ToString();

            _ErrorNumber = 0;
            _WarningNumber = 0;

        }


        private SolidColorBrush GetRowColor(string data)
        {
            if (data.IndexOf("Error", StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                _ErrorNumber += 1;
                return Brushes.Red;
            }
            else if (data.IndexOf("Warning", StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                _WarningNumber += 1;
                return Brushes.Yellow;

            }

            return Brushes.White;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = $"The Log - {Assembly.GetEntryAssembly().GetName().Version}";
        }

        private void DataGridFile_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                LogFileLocation.Text = files[0];

                OpenFileLog();


            }
        }
    }
}
