using Maui.Controls.Sample.Controls;
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
		}

		private async void PopButton_Clicked(object sender, System.EventArgs e)
		{
			await Navigation.PopAsync();
		}

		private async void PushButton_Clicked(object sender, System.EventArgs e)
		{
			await Navigation.PushAsync(new SemanticsPage());
		}
	}
}