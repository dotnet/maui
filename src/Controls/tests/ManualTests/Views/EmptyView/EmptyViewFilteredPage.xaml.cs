using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class EmptyViewFilteredPage : ContentPage
	{
		public EmptyViewFilteredPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
