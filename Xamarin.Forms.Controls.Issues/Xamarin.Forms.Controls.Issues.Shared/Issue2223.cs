using System;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2223, "Possibility to change IsPassword", PlatformAffected.macOS)]
	public class Issue2223 : TestContentPage
	{
		protected override void Init ()
		{
			var checkEntry = new Entry
			{
				HeightRequest = 100,
				FontSize = 50,
				Placeholder = "I can be both secure and non-secure"
			};
			Content = new StackLayout
			{
				Padding = new Thickness(100),
				Children = {
					checkEntry,
					new Button
					{
						HeightRequest = 80,
						FontSize = 40,
						BackgroundColor = Color.LightBlue,
						TextColor = Color.Black,
						Text = "Click me to change IsPassword of the entry",
						Command = new Command(() => checkEntry.IsPassword = !checkEntry.IsPassword)
					}
				}
			};
		}

#if UITEST
#endif

	}
}


