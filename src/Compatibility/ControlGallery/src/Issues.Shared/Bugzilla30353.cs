using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest.iOS;
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 30353, "FlyoutPage.IsPresentedChanged is not raised")]
#if UITEST
	[Category(UITestCategories.UwpIgnore)]
	[Category(UITestCategories.FlyoutPage)]
	[Category(UITestCategories.Bugzilla)]
#endif
	public class Bugzilla30353 : TestFlyoutPage
	{
		protected override void Init()
		{
			var lbl = new Label
			{
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				Text = "Detail"
			};

#if !UITEST
			if (App.IOSVersion == 7 || Device.RuntimePlatform == Device.macOS)
			{
				lbl.Text = "Don't run";
			}
#endif

			var lblMaster = new Label
			{
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				Text = "Flyout"
			};
			var btn = new Button()
			{
				Text = "Toggle"
			};
			var btn1 = new Button()
			{
				Text = "Toggle"
			};

			btn.Clicked += (object sender, EventArgs e) => IsPresented = !IsPresented;
			btn1.Clicked += (object sender, EventArgs e) => IsPresented = !IsPresented;

			var stacklayout = new StackLayout();
			stacklayout.Children.Add(lbl);
			stacklayout.Children.Add(btn);

			var stacklayout1 = new StackLayout();
			stacklayout1.Children.Add(lblMaster);
			stacklayout1.Children.Add(btn1);

			Flyout = new ContentPage
			{
				Title = "IsPresentedChanged Test",
				BackgroundColor = Colors.Green,
				Content = stacklayout1
			};
			Detail = new ContentPage
			{
				BackgroundColor = Colors.Gray,
				Content = stacklayout
			};
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
			IsPresentedChanged += (s, e) =>
				lblMaster.Text = lbl.Text = string.Format("The Flyout is now {0}", IsPresented ? "visible" : "invisible");
		}

#if UITEST
		[Test]
		public void FlyoutPageIsPresentedChangedRaised()
		{
			var dontRun = RunningApp.Query(q => q.Marked("Don't run"));
			if (dontRun.Length > 0)
			{
				return;
			}
			RunningApp.SetOrientationPortrait();
			RunningApp.Screenshot("Portrait");
			RunningApp.Tap(q => q.Marked("Toggle"));
			RunningApp.Screenshot("Portrait Visible");
			RunningApp.WaitForElement(q => q.Marked("The Flyout is now visible"));
			Back();
			RunningApp.Screenshot("Portrait Invisible");
			RunningApp.WaitForElement(q => q.Marked("The Flyout is now invisible"));
			RunningApp.SetOrientationLandscape();
			RunningApp.Screenshot("Landscape Invisible");
			RunningApp.WaitForElement(q => q.Marked("The Flyout is now invisible"));
			RunningApp.Tap(q => q.Marked("Toggle"));
			RunningApp.Screenshot("Landscape Visible");
			RunningApp.WaitForElement(q => q.Marked("The Flyout is now visible"));
			Back();
			RunningApp.Screenshot("Landscape InVisible");
			RunningApp.WaitForElement(q => q.Marked("The Flyout is now invisible"));
			RunningApp.SetOrientationPortrait();
			RunningApp.Tap(q => q.Marked("Toggle"));
			RunningApp.Screenshot("Portrait Visible");
			RunningApp.WaitForElement(q => q.Marked("The Flyout is now visible"));
			Back();
			RunningApp.Screenshot("Portrait Invisible");
			RunningApp.WaitForElement(q => q.Marked("The Flyout is now invisible"));
			RunningApp.SetOrientationLandscape();
		}

		[TearDown]
		public override void TearDown()
		{
			RunningApp.SetOrientationPortrait();

			base.TearDown();
		}

		void Back()
		{
#if __IOS__ || WINDOWS
			RunningApp.Tap(q => q.Marked("Toggle"));
#else
			RunningApp.Back();
#endif
		}
#endif
	}
}
