using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Shell)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11247,
		"[Bug] Shell FlyoutIsPresented not working if set in \"navigating\" handler",
		PlatformAffected.iOS)]
	public class Issue11247 : TestShell
	{
		protected override void Init()
		{
			var page = CreateContentPage<FlyoutItem>("FlyoutItem 1");
			CreateContentPage<FlyoutItem>("FlyoutItem 2");

			Items.Add(new MenuItem()
			{
				Text = "Click Me To Close Flyout",
				AutomationId = "CloseFlyout",
				Command = new Command(() =>
				{
					FlyoutIsPresented = false;
				})
			});

			page.Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "If the flyout wasn't open when this test started the test has failed"
					},
					new Label()
					{
						Text = "Now, Open the Flyout and Click on FlyoutItem 2. Nothing should happen and flyout should remain open"
					}
				}
			};
		}

		protected override void OnNavigating(ShellNavigatingEventArgs args)
		{
			base.OnNavigating(args);

			if (args.CanCancel)
			{
				args.Cancel();
			}

			FlyoutIsPresented = true;
		}


#if UITEST && __SHELL__
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		public void SettingFlyoutIsPresentedInNavigatingKeepsFlyoutOpen()
		{
			RunningApp.Tap("CloseFlyout");
			ShowFlyout();
			RunningApp.Tap("FlyoutItem 1");
			RunningApp.Tap("FlyoutItem 2");
			RunningApp.WaitForElement("FlyoutItem 1");
			RunningApp.WaitForElement("FlyoutItem 2");

		}
#endif
	}
}
