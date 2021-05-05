using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Flyout Behavior",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class FlyoutBehaviorShell : TestShell
	{
		BackButtonBehavior _behavior;
		const string title = "Basic Test";
		const string FlyoutItem = "Flyout Item";
		const string EnableFlyoutBehavior = "EnableFlyoutBehavior";
		const string DisableFlyoutBehavior = "DisableFlyoutBehavior";
		const string LockFlyoutBehavior = "LockFlyoutBehavior";
		const string OpenFlyout = "OpenFlyout";
		const string EnableBackButtonBehavior = "EnableBackButtonBehavior";
		const string DisableBackButtonBehavior = "DisableBackButtonBehavior";

		protected override void Init()
		{
			_behavior = new BackButtonBehavior();
			var page = GetContentPage(title);
			Shell.SetBackButtonBehavior(page, _behavior);
			AddContentPage(page).Title = FlyoutItem;
			Shell.SetFlyoutBehavior(this.CurrentItem, FlyoutBehavior.Disabled);
		}

		ContentPage GetContentPage(string title)
		{
			ContentPage page = new ContentPage()
			{
				Title = title,
				Content = new StackLayout()
				{
					VerticalOptions = LayoutOptions.FillAndExpand,
					BackgroundColor = Colors.Red,
					Children =
					{
						new Button()
						{
							Text = "Enable Flyout Behavior",
							Command = new Command(() =>
							{
								Shell.SetFlyoutBehavior(this.CurrentItem, FlyoutBehavior.Flyout);
								this.FlyoutIsPresented = false;
							}),
							AutomationId = EnableFlyoutBehavior
						},
						new Button()
						{
							Text = "Disable Flyout Behavior",
							Command = new Command(() =>
							{
								Shell.SetFlyoutBehavior(this.CurrentItem, FlyoutBehavior.Disabled);
								this.FlyoutIsPresented = false;
							}),
							AutomationId = DisableFlyoutBehavior
						},
						new Button()
						{
							Text = "Lock Flyout Behavior",
							Command = new Command(() =>
							{
								Shell.SetFlyoutBehavior(this.CurrentItem, FlyoutBehavior.Locked);
							}),
							AutomationId = LockFlyoutBehavior
						},
						new StackLayout()
						{
							VerticalOptions = LayoutOptions.CenterAndExpand
						},
						new Button()
						{
							Text = "Open Flyout",
							Command = new Command(() =>
							{
								this.FlyoutIsPresented = true;
							}),
							AutomationId = OpenFlyout,
							VerticalOptions = LayoutOptions.End
						},
						new Button()
						{
							Text = "Enable Back Button Behavior",
							Command = new Command(() =>
							{
								_behavior.IsEnabled = true;
							}),
							AutomationId = EnableBackButtonBehavior,
							VerticalOptions = LayoutOptions.End

						},
						new Button()
						{
							Text = "Disable Back Button Behavior",
							Command = new Command(() =>
							{
								_behavior.IsEnabled = false;
							}),
							AutomationId = DisableBackButtonBehavior,
							VerticalOptions = LayoutOptions.End
						}
					}
				}
			};

			return page;
		}

#if UITEST && __SHELL__

		[NUnit.Framework.Category(UITestCategories.Gestures)]
		[Test]
		public void FlyoutTests()
		{
			// Flyout is visible
			RunningApp.WaitForElement(EnableFlyoutBehavior);

			// Starting shell out as disabled correctly disables flyout
			RunningApp.WaitForNoElement(FlyoutIconAutomationId, "Flyout Icon Visible on Startup");
			ShowFlyout(usingSwipe: true, testForFlyoutIcon: false);
			RunningApp.WaitForNoElement(FlyoutItem, "Flyout Visible on Startup");

			// Enable Flyout Test
			RunningApp.Tap(EnableFlyoutBehavior);
			ShowFlyout(usingSwipe: true);
			RunningApp.WaitForElement(FlyoutItem, "Flyout Not Visible after Enabled");
			RunningApp.Tap(FlyoutItem);

			// Flyout Icon is not visible but you can still swipe open
			RunningApp.Tap(DisableFlyoutBehavior);
			RunningApp.WaitForNoElement(FlyoutIconAutomationId, "Flyout Icon Visible after being Disabled");
			ShowFlyout(usingSwipe: true, testForFlyoutIcon: false);
			RunningApp.WaitForNoElement(FlyoutItem, "Flyout Visible after being Disabled");


			// enable flyout and make sure disabling back button behavior doesn't hide icon
			RunningApp.Tap(EnableFlyoutBehavior);
			RunningApp.WaitForElement(FlyoutIconAutomationId);
			RunningApp.Tap(DisableBackButtonBehavior);
			ShowFlyout(usingSwipe: true);
			RunningApp.WaitForElement(FlyoutItem, "Flyout swipe not working after Disabling Back Button Behavior");
			RunningApp.Tap(FlyoutItem);

			// make sure you can still open flyout via code
			RunningApp.Tap(EnableFlyoutBehavior);
			RunningApp.Tap(EnableBackButtonBehavior);
			RunningApp.Tap(OpenFlyout);
			RunningApp.WaitForElement(FlyoutItem, "Flyout not opening via code");
			RunningApp.Tap(FlyoutItem);

			// make sure you can still open flyout via code if flyout behavior is disabled
			RunningApp.Tap(DisableFlyoutBehavior);
			RunningApp.Tap(EnableBackButtonBehavior);
			RunningApp.Tap(OpenFlyout);
			RunningApp.WaitForElement(FlyoutItem, "Flyout not opening via code when flyout behavior disabled");
			RunningApp.Tap(FlyoutItem);

			// make sure you can still open flyout via code if back button behavior is disabled
			RunningApp.Tap(EnableFlyoutBehavior);
			RunningApp.Tap(DisableBackButtonBehavior);
			RunningApp.Tap(OpenFlyout);
			RunningApp.WaitForElement(FlyoutItem, "Flyout not opening via code when back button behavior is disabled");
			RunningApp.Tap(FlyoutItem);

		}

		[Test]
		public void WhenFlyoutIsLockedButtonsAreStillVisible()
		{
			// FlyoutLocked ensure that the flyout and buttons are still visible
			RunningApp.Tap(EnableBackButtonBehavior);
			RunningApp.Tap(LockFlyoutBehavior);
			RunningApp.WaitForElement(title, "Flyout Locked hiding content");
			RunningApp.Tap(EnableFlyoutBehavior);
			RunningApp.WaitForNoElement(FlyoutItem);
		}
#endif
	}
}
