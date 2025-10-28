using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListEmptyGroupsPage : ContentPage
	{
		public VerticalListEmptyGroupsPage()
		{
			InitializeComponent();
			BindingContext = new GroupedAnimalsViewModel(true);
		}
	}
}
