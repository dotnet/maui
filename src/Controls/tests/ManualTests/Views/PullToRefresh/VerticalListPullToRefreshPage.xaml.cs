using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListPullToRefreshPage : ContentPage
	{
		public VerticalListPullToRefreshPage()
		{
			InitializeComponent();
			BindingContext = new AnimalsViewModel();
		}
	}
}
