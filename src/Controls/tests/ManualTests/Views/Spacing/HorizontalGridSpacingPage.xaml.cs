using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class HorizontalGridSpacingPage : ContentPage
	{
		public HorizontalGridSpacingPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
