using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class HorizontalGridPullToRefreshPage : ContentPage
	{
		public HorizontalGridPullToRefreshPage()
		{
			InitializeComponent();
			BindingContext = new AnimalsViewModel();
		}
	}
}
