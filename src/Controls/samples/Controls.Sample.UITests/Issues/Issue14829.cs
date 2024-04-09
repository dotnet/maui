using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 14829, "DisplayActionSheet still not working on Windows", PlatformAffected.UWP)]
	public class Issue14829 : TestContentPage
	{
		protected override void Init()
		{
			var navPage = new NavigationPage(new MainPage());
			NavigatedTo += Issue14829_NavigatedTo;

			async void Issue14829_NavigatedTo(object sender, NavigatedToEventArgs e)
			{
				NavigatedTo -= Issue14829_NavigatedTo;

				await Navigation.PushModalAsync(navPage);
			}
		}

		public partial class MainPage : ContentPage
		{
			protected override void OnAppearing()
			{
				Content = new VerticalStackLayout()
				{
					new Button()
					{
						AutomationId = "DisplayActionSheetButton",
						Command = new Command(async () =>
						{
							await DisplayActionSheet("ActionSheet: Send to?", "Cancel", null, "Email", "Twitter", "Facebook");
						}),
						Text = "Click to Display Action Sheet"
					}
				};
				Content.BackgroundColor = Colors.White;

				base.OnAppearing();
			}
		}
	}
}
