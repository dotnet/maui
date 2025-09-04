using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalGridSingleSpanLayoutPage : ContentPage
	{
		public VerticalGridSingleSpanLayoutPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}