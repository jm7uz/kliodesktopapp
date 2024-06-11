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

namespace KlioDesktopApp.Components.Basket
{
    /// <summary>
    /// Interaction logic for BasketViewControl.xaml
    /// </summary>
    public partial class BasketViewControl : UserControl
    {
        public event EventHandler<int> OnMinusClicked;
        public event EventHandler<int> OnPlusClicked;
        public event EventHandler<int> OnTrashClicked;

        public int ProductId { get; set; }

        public BasketViewControl()
        {
            InitializeComponent();
        }
    }
}
