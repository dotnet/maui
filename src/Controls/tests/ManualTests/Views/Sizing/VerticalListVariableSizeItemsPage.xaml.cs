using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListVariableSizeItemsPage : ContentPage
	{
		public VerticalListVariableSizeItemsPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
