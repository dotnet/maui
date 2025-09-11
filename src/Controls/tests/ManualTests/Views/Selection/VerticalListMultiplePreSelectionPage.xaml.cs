using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListMultiplePreSelectionPage : ContentPage
	{
		public VerticalListMultiplePreSelectionPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
