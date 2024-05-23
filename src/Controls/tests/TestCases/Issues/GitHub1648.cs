using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1648, "FlyoutPage throws ArgumentOutOfRangeException", PlatformAffected.UWP)]
	public class GitHub1648 : TestNavigationPage
	{
		protected override void Init()
		{
			Navigation.PushAsync(new FlyoutPage
			{
				Flyout = new NavigationPage(new ContentPage())
				{
					Title = "Flyout"
				},
				Detail = new ContentPage(),
			});
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Navigation.PushModalAsync(new SimplePage());
		}

		class SimplePage : ContentPage
		{
			public SimplePage()
			{
				Content = new StackLayout()
				{
					Children = {
						new Label {
							Text = "Success"
						},
						new Button
						{
							Text = "Reload",
							Command = new Command(() => Navigation.PopModalAsync())
						}
					}
				};
			}
		}
	}
}