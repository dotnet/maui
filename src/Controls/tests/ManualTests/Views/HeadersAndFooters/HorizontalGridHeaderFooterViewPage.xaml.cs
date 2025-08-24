using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class HorizontalGridHeaderFooterViewPage : ContentPage
	{
		public HorizontalGridHeaderFooterViewPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
