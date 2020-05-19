using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 25234, "Use of CustomMessageBox resets SystemTray BackgroundColor to black", PlatformAffected.WinPhone)]
	public class Bugzilla25234 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Button
					{
						Text = "Click for Alert",
						Command = new Command(() =>
						{
							DisplayAlert("Display Alert",
								"If the theme is set to light on WP8, the status bar should return to the white color when closed", "OK");
						})
					}
				}
			};
		}
	}
}
