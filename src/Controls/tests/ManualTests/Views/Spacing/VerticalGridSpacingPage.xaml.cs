using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalGridSpacingPage : ContentPage
	{
		public VerticalGridSpacingPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
