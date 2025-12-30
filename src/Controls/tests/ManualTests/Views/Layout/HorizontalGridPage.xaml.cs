using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class HorizontalGridPage : ContentPage
	{
		public HorizontalGridPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
