using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace LogReader_WPF
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VersionNumber.Text = RandomHelpers.GetCurrentVerion().ToString();
        }

        private void Close_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ProjectURL_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(
                new ProcessStartInfo { FileName = ProjectURL.Text, UseShellExecute = true }
            );
        }
    }
}
