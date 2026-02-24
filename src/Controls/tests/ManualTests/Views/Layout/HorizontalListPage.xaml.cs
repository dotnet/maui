using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class HorizontalListPage : ContentPage
	{
		public HorizontalListPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
