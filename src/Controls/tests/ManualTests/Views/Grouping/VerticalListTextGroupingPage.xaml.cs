using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListTextGroupingPage : ContentPage
	{
		public VerticalListTextGroupingPage()
		{
			InitializeComponent();
			BindingContext = new GroupedAnimalsViewModel();
		}
	}
}
