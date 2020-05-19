using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7898, "navigation page doesn't hide previous page", PlatformAffected.macOS)]
	public class Issue7898 : TestNavigationPage
	{
		protected override void Init()
		{
			Navigation.PushAsync(new ContentPage
			{
				BackgroundColor = Color.Yellow,
				Content = new StackLayout()
				{
					Children = {
						new Button
						{
							HorizontalOptions = LayoutOptions.Start,
							Text = "push page",
							Command = new Command(async () => await Navigation.PushAsync(new PageWithTransparency(),false))
						},
						new Label
						{
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center,
							Text = "This text should be invisible after second page pushed",
						}
					}
				},
			});
		}
		class PageWithTransparency : ContentPage
		{
			public PageWithTransparency()
			{
				this.BackgroundColor = Color.Red.MultiplyAlpha(0.2);
				Content = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.CenterAndExpand,
					Text = "Text on second page",
				};
			}
		}
	}
}