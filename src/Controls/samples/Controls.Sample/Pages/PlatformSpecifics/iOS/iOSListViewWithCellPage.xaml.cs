using Microsoft.Maui.Controls;
using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Pages
{
    public partial class iOSListViewWithCellPage : ContentPage
    {
        public iOSListViewWithCellPage()
        {
            InitializeComponent();
            BindingContext = new ListViewViewModel(20);
        }
    }
}
