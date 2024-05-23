using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 30166, "NavigationBar.BarBackgroundColor resets on Lollipop after popping modal page", PlatformAffected.Android)]
	public class Bugzilla30166 : TestNavigationPage
	{
		protected override void Init()
		{
			BarBackgroundColor = Colors.Red;

			Navigation.PushAsync(new ContentPage
			{
				Content = new Button
				{
					Text = "Push Modal",
					Command = new Command(async () => await Navigation.PushModalAsync(new ContentPage
					{
						Content = new Button
						{
							Text = "Back",
							Command = new Command(async () => await Navigation.PopModalAsync()),
						},
					})),
				},
			});
		}
	}
}