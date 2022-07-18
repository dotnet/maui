using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;

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
