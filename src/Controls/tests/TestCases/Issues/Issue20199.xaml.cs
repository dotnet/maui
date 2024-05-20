using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20199, "[iOS] Page titles do not appear until navigating when pushing a modal page at startup", PlatformAffected.iOS)]
	public partial class Issue20199 : Shell
	{
		public Issue20199()
		{
			InitializeComponent();
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			var closeModalPageButton = new Button() { Text = "Hide", AutomationId = "button" };
			closeModalPageButton.Clicked += (s, e) =>  Navigation.PopAsync();

			var modalPage = new ContentPage() { Content = closeModalPageButton };

			await Navigation.PushModalAsync(modalPage);
		}
	}

	public class Issue20199Page : ContentPage { }
}