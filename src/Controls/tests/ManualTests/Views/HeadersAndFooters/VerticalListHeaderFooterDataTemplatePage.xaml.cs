using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListHeaderFooterDataTemplatePage : ContentPage
	{
		public VerticalListHeaderFooterDataTemplatePage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
