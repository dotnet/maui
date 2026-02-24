using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class HorizontalListSpacingPage : ContentPage
	{
		public HorizontalListSpacingPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
