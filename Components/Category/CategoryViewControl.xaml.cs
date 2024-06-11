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

namespace KlioDesktopApp.Components.Category
{
    /// <summary>
    /// Interaction logic for CategoryViewControl.xaml
    /// </summary>
    public partial class CategoryViewControl : UserControl
    {
        public CategoryViewControl()
        {
            InitializeComponent();
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                UpdateBackground();
            }
        }

        private void UpdateBackground()
        {
            if (IsSelected)
            {
                cBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EC7403"));
            }
            else
            {
                cBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEFEFE"));
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsSelected = true;
        }
    }
}
