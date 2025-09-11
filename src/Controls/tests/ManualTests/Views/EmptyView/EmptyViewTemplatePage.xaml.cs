using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class EmptyViewTemplatePage : ContentPage
	{
		public EmptyViewTemplatePage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}
	}
}
