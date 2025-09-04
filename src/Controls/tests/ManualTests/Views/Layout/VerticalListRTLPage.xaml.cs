using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListRTLPage : ContentPage
	{
		public VerticalListRTLPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
