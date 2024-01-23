using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using CommunityToolkit.Maui.Views;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20092, "[Android] LineBreakMode=\"WordWrap\" not working", PlatformAffected.Android)]
	public partial class Issue20092 : ContentPage
	{
		public Issue20092()
		{
			InitializeComponent();
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			await this.ShowPopupAsync(new Issue20092Popup());
		}
	}
}