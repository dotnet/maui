using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListSpacingPage : ContentPage
	{
		public VerticalListSpacingPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
