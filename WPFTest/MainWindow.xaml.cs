
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace WPFTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow :Window
    {

        public MainWindow()
        {
            InitializeComponent();          
        }
        private void MessageBox_Click(object sender, RoutedEventArgs e)
        {
            HandyControl.Controls.Dialog.Show(new TextDialog("123456"));
        }
    }
}
