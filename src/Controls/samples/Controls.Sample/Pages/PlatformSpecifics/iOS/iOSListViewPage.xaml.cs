using Microsoft.Maui.Controls;
using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Pages
{
    public partial class iOSListViewPage : ContentPage
    {
        public iOSListViewPage()
        {
            InitializeComponent();
			BindingContext = new ListViewViewModel();
        }
    }
}
