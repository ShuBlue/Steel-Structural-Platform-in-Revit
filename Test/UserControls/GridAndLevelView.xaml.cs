using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Test.ViewModel;

namespace Test.UserControls
{
    /// <summary>
    /// GridAndLevelView.xaml 的交互逻辑
    /// </summary>
    public partial class GridAndLevelView : UserControl
    {
        private static GridAndLevelView _instance;
        public MainViewModel VM { get; set; }
        private GridAndLevelView()
        {
            InitializeComponent();
           
        }
        public void Initial()
        {
            BentNumber.Text = "6";
            NetDepth.Text = "18000";
            Space.Text = "6000";
            IsUniform.IsChecked = true;
            VM = new MainViewModel(Convert.ToInt32(BentNumber.Text));
            this.DataContext = VM;
        }
        public static GridAndLevelView GetInstance()
        {
            if (_instance == null)
            {
                _instance = new GridAndLevelView();
            }
            return _instance;
        }



        private void IsUniform_Checked(object sender, RoutedEventArgs e)
        {
            IsRandom.IsChecked = false;
            SpaceList.IsEnabled = false;
        }


        private void IsRandom_Checked(object sender, RoutedEventArgs e)
        {
            IsUniform.IsChecked = false;
            Space.IsEnabled = false;

        }

        private void IsUniform_Unchecked(object sender, RoutedEventArgs e)
        {
            IsRandom.IsChecked = true;
            SpaceList.IsEnabled = true;
        }

        private void IsRandom_Unchecked(object sender, RoutedEventArgs e)
        {
            IsUniform.IsChecked = true;
            Space.IsEnabled = true;
        }
        //框架数量发生变化，通知DataGrid中的行发生相应的变化。
        private void BentNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(BentNumber.Text, out int result))
            {
                Length.Text = (space * (result - 1)).ToString();
                VM = new MainViewModel(result);
                Messenger.Default.Send<int>(result, "bentNumber");
            }
            else
            {
                VM = new MainViewModel(5);
            }
            DataContext = VM;

        }

        private int space;
        private void Space_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(Space.Text, out int result))
            {
                if (BentNumber.Text != null)
                {
                    Length.Text = (result * (Convert.ToInt32(BentNumber.Text) - 1)).ToString();
                    space = result;
                }
                else
                {
                    Length.Text = (result * (5 - 1)).ToString();
                    space = result;
                }
            }
            else
            {
                if (BentNumber.Text != null)
                {
                    Length.Text = (3000 * (Convert.ToInt32(BentNumber.Text) - 1)).ToString();
                    space = 3000;
                }
                else
                {
                    Length.Text = (result * (5 - 1)).ToString();
                    space = 3000;
                }
            }
        }
    }

}
