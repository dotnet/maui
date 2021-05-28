using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.FlyoutPage)]
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 30324, "Detail view of FlyoutPage does not get appearance events on Android when whole FlyoutPage disappears/reappears")]
	public class Bugzilla30324 : TestNavigationPage
	{
		Label _lbl;
		int _count;

		protected override void Init()
		{
			FlyoutPage page = new FlyoutPage();
			page.Flyout = new Page() { Title = "Flyout", BackgroundColor = Colors.Red };
			_lbl = new Label();

			var otherPage = new ContentPage()
			{
				Title = "Other",
				Content = new StackLayout
				{
					Children = {
						new Button () {
							Text = "navigate back",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center,

							Command = new Command (() => Navigation.PopAsync())
						}
					}
				}
			};

			page.Detail = new ContentPage()
			{
				Title = "Detail",
				Content = new StackLayout
				{
					Children = {
						_lbl,
						new Button () {
							Text = "navigate",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center,

							Command = new Command (() => Navigation.PushAsync (otherPage))
						}
					}
				}
			};

			page.Appearing += (sender, e) =>
			{
				System.Diagnostics.Debug.WriteLine("Appear MDP");
			};
			page.Disappearing += (sender, e) =>
			{
				System.Diagnostics.Debug.WriteLine("Disappear MDP");
			};
			page.Detail.Appearing += (sender, args) =>
			{
				if (_count == 2)
					_lbl.Text = "Appear detail";
				System.Diagnostics.Debug.WriteLine("Appear detail");
			};
			page.Detail.Disappearing += (sender, args) =>
			{
				System.Diagnostics.Debug.WriteLine("Disappear detail");
				_lbl.Text = "Disappear detail";
				page.Detail.BackgroundColor = Colors.Green;
				_count++;
			};
			page.Flyout.Appearing += (sender, e) =>
			{
				System.Diagnostics.Debug.WriteLine("Appear master");
			};
			Navigation.PushAsync(page);
		}

#if UITEST
		[Test]
		public void Bugzilla30324Test ()
		{
			RunningApp.Tap (q => q.Marked ("navigate"));
			RunningApp.Tap (q => q.Marked ("navigate back"));
			RunningApp.WaitForElement (q => q.Marked ("Disappear detail"));
			RunningApp.Tap (q => q.Marked ("navigate"));
			RunningApp.Tap (q => q.Marked ("navigate back"));
			RunningApp.WaitForElement (q => q.Marked ("Appear detail"));
		}
#endif
	}


}


