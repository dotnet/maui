using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListGroupingVariableSizeItemsPage : ContentPage
	{
		public VerticalListGroupingVariableSizeItemsPage()
		{
			InitializeComponent();
			BindingContext = new GroupedAnimalsViewModel();
		}
	}
}
