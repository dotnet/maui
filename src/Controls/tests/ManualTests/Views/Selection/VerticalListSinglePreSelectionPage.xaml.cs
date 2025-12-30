using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListSinglePreSelectionPage : ContentPage
	{
		public VerticalListSinglePreSelectionPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
