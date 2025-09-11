using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListDataTemplateSelectorPage : ContentPage
	{
		public VerticalListDataTemplateSelectorPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
