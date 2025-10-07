using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class EmptyViewDataTemplateSelectorPage : ContentPage
	{
		public EmptyViewDataTemplateSelectorPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
