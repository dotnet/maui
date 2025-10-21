using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListPage : ContentPage
	{
		public VerticalListPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
