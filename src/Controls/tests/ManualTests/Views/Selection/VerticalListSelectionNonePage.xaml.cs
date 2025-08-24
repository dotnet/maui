using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListSelectionNonePage : ContentPage
	{
		public VerticalListSelectionNonePage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}