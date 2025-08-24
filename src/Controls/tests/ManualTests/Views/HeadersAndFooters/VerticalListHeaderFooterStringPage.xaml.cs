using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListHeaderFooterStringPage : ContentPage
	{
		public VerticalListHeaderFooterStringPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
