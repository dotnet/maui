using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListSwipeContextItemsPage : ContentPage
	{
		public VerticalListSwipeContextItemsPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
