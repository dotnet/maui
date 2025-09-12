using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListGroupingPage : ContentPage
	{
		public VerticalListGroupingPage()
		{
			InitializeComponent();
			BindingContext = new GroupedAnimalsViewModel();
		}
	}
}
