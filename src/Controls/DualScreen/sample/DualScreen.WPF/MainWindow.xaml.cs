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
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.WPF;

namespace DualScreen.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
	{
        public MainWindow()
        {
            InitializeComponent();
			Forms.Init();
			LoadApplication(new DualScreen.App());
		}
    }
}
