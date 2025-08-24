using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListSelectionColorPage : ContentPage
	{
		public VerticalListSelectionColorPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
