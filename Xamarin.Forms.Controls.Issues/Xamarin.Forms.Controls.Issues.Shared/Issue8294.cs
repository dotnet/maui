using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8294, "[Bug] UIStatusBarStyle does not appear to change when device is in dark mode",
		PlatformAffected.iOS)]
	public class Issue8294 : TestNavigationPage
	{
		protected override void Init()
		{
			var contentPage = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Button()
						{
							Text = "Light",
							Command = new Command(() =>
							{
								LoadLight();
							}),
						},
						new Button()
						{
							Text = "Dark",
							Command = new Command(() =>
							{
								LoadDark();
							}),
						},
						new Label()
						{
							Text = "To test this UIViewControllerBasedStatusBarAppearance needs to be set to false in Info.plist",
						},
					}
				}
			};
			PushAsync(contentPage);
			LoadDark();
		}

		void LoadDark()
		{
			BarTextColor = Color.White;
			BarBackgroundColor = Color.Black;
			CurrentPage.Title = "Dark NavBar";

			System.Diagnostics.Debug.WriteLine($"BarTextColor.Luminosity: {BarTextColor.Luminosity}");
		}

		void LoadLight()
		{
			BarTextColor = Color.Black;
			BarBackgroundColor = Color.White;
			CurrentPage.Title = "Light NavBar";

			System.Diagnostics.Debug.WriteLine($"BarTextColor.Luminosity: {BarTextColor.Luminosity}");
		}
	}
}
