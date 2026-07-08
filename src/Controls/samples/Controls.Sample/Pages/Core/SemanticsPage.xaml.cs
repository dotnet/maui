using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SemanticsPage : BasePage
	{
		public SemanticsPage()
		{
			InitializeComponent();
			pushButton.Clicked += PushButton_Clicked;
			popButton.Clicked += PopButton_Clicked;

			foreach (var element in (this as IElementController).Descendants())
			{
				element.AutomationId = "If you are hearing this then AutomationId is currently breaking accesssibility for this control";
			}
		}

		private async void PopButton_Clicked(object? sender, System.EventArgs e)
		{
			if (Navigation.ModalStack.Count > 0)
				await Navigation.PopModalAsync();
			else
				await Navigation.PopAsync();
		}

		private async void PushButton_Clicked(object? sender, System.EventArgs e)
		{
			await Navigation.PushAsync(new SemanticsPage());
		}

		private void SetSemanticFocusButton_Clicked(object sender, System.EventArgs e)
		{
			semanticFocusLabel.SetSemanticFocus();
		}
	}
}