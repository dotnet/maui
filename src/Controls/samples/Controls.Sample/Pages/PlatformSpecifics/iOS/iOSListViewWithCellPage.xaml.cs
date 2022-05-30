using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;

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
