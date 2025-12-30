using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListHeaderFooterViewPage : ContentPage
	{
		public VerticalListHeaderFooterViewPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
