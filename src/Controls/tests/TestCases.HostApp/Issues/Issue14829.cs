using System;
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
			Loaded += OnLoaded;

			async void OnLoaded(object sender, EventArgs e)
			{
				Loaded -= OnLoaded;
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
