using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using System.Threading;
using System.ComponentModel;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Flyout Behavior",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class ShellFlyoutBehavior : TestShell
	{
		BackButtonBehavior _behavior;
		const string title = "Basic Test";
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
			AddContentPage(page).Title = title;
		}

		ContentPage GetContentPage(string title)
		{
			ContentPage page = new ContentPage()
			{
				Title = title,
				Content = new StackLayout()
				{
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
						new Button()
						{
							Text = "Open Flyout",
							Command = new Command(() =>
							{
								this.FlyoutIsPresented = true;
							}),
							AutomationId = OpenFlyout
						},
						new Button()
						{
							Text = "Enable Back Button Behavior",
							Command = new Command(() =>
							{
								_behavior.IsEnabled = true;
							}),
							AutomationId = EnableBackButtonBehavior

						},
						new Button()
						{
							Text = "Disable Back Button Behavior",
							Command = new Command(() =>
							{
								_behavior.IsEnabled = false;
							}),
							AutomationId = DisableBackButtonBehavior
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
			RunningApp.WaitForElement(FlyoutIconAutomationId);

			// Flyout Icon is not visible but you can still swipe open
			RunningApp.Tap(DisableFlyoutBehavior);
			RunningApp.WaitForNoElement(FlyoutIconAutomationId);
			ShowFlyout(usingSwipe: true, testForFlyoutIcon: false);
			RunningApp.WaitForElement(title);
			RunningApp.Tap(title);


			// enable flyout and make sure disabling back button behavior doesn't hide icon
			RunningApp.Tap(EnableFlyoutBehavior);
			RunningApp.WaitForElement(FlyoutIconAutomationId);
			RunningApp.Tap(DisableBackButtonBehavior);
			ShowFlyout(usingSwipe: true);
			RunningApp.WaitForElement(title);
			RunningApp.Tap(title);

			// make sure you can still open flyout via code
			RunningApp.Tap(EnableFlyoutBehavior);
			RunningApp.Tap(EnableBackButtonBehavior);
			RunningApp.Tap(OpenFlyout);
			RunningApp.WaitForElement(title);
			RunningApp.Tap(title);

			// make sure you can still open flyout via code if flyout behavior is disabled
			RunningApp.Tap(DisableFlyoutBehavior);
			RunningApp.Tap(EnableBackButtonBehavior);
			RunningApp.Tap(OpenFlyout);
			RunningApp.WaitForElement(title);
			RunningApp.Tap(title);

			// make sure you can still open flyout via code if back button behavior is disabled
			RunningApp.Tap(EnableFlyoutBehavior);
			RunningApp.Tap(DisableBackButtonBehavior);
			RunningApp.Tap(OpenFlyout);
			RunningApp.WaitForElement(title);
			RunningApp.Tap(title);

			// FlyoutLocked ensure that the flyout and buttons are still visible
			RunningApp.Tap(EnableBackButtonBehavior);
			RunningApp.Tap(LockFlyoutBehavior);
			RunningApp.WaitForElement(title);
			RunningApp.Tap(EnableFlyoutBehavior);

		}

#endif
	}
}
