using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalGridPage : ContentPage
	{
		public VerticalGridPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
