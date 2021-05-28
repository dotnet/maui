using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{

#if UITEST
	public static class Issue1461Helpers
	{
		public static bool ShouldRunTest(Xamarin.UITest.IApp app)
		{
			return app.IsTablet();
		}
	}
#endif
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1461, "1461 - (Popover in Portrait and Landscape)", PlatformAffected.iOS)]
	public class Issue1461 : TestContentPage
	{
		protected override async void Init()
		{
			await Navigation.PushModalAsync(new Issue1461Page(FlyoutLayoutBehavior.Popover, false));
		}

		//#if UITEST
		//		[Test]
		//		[UiTest (typeof (FlyoutPage), "IsPresented")]
		//		[UiTest (typeof (FlyoutPage), "Flyout")]
		//		public void Test1 ()
		//		{
		//			if (Issue1461Helpers.ShouldRunTest (RunningApp)) {
		//				RunningApp.SetOrientationLandscape ();
		//				var query = RunningApp.Query (q => q.Marked ("Master_Label"));
		//				Assert.IsTrue (!query.Any (), "Flyout should not present");
		//				RunningApp.Screenshot ("Flyout should not present");
		//				RunningApp.SetOrientationPortrait ();
		//				query = RunningApp.Query (q => q.Marked ("Master_Label"));
		//				Assert.IsTrue (!query.Any (), "Flyout should not present");
		//				RunningApp.Screenshot ("Flyout should not present");
		//				RunningApp.Tap (q => q.Marked ("Go Back"));
		//			} else {
		//				Assert.Inconclusive ("Only run on iOS Tablet");
		//			}
		//		}
		//#endif
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1461, "1461 - Default", PlatformAffected.iOS)]
	public class Issue1461A : TestContentPage
	{
		protected override async void Init()
		{
			await Navigation.PushModalAsync(new Issue1461Page(FlyoutLayoutBehavior.Default, null));
		}

		//#if UITEST
		//		[Test]
		//		[UiTest (typeof (FlyoutPage), "IsPresented")]
		//		[UiTest (typeof (FlyoutPage), "Flyout")]
		//		public void Test2 ()
		//		{
		//			if (Issue1461Helpers.ShouldRunTest(RunningApp)) {
		//				RunningApp.SetOrientationLandscape ();
		//				var query = RunningApp.Query (q => q.Marked ("Master_Label"));
		//				Assert.IsTrue (query.Any (), "Flyout should be present");
		//				RunningApp.Screenshot ("Flyout should not present");
		//				RunningApp.SetOrientationPortrait ();
		//				query = RunningApp.Query (q => q.Marked ("Master_Label"));
		//				Assert.IsTrue (!query.Any (), "Flyout should not present");
		//				RunningApp.Screenshot ("Flyout should not present");
		//				RunningApp.Tap (q => q.Marked ("Go Back"));
		//			} else {
		//				Assert.Inconclusive ("Only run on iOS Tablet");
		//			}
		//		}

		//		#if UITEST
		//		[Test]
		//		[UiTest (typeof (FlyoutPage), "Button")]
		//		public void TestButton ()
		//		{
		//			if (Issue1461Helpers.ShouldRunTest(RunningApp)) {
		//				RunningApp.SetOrientationLandscape ();
		//				RunningApp.WaitForNoElement (q => q.Marked ("bank"));
		//				RunningApp.SetOrientationPortrait ();
		//				RunningApp.WaitForElement (q => q.Marked ("bank"));
		//			} else {
		//				Assert.Inconclusive ("Only run on iOS Tablet");
		//			}
		//		}
		//		#endif
		//#endif
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1461, "1461 - (Splitview in Landscape)", PlatformAffected.iOS)]
	public class Issue1461B : TestContentPage
	{
		protected override async void Init()
		{
			await Navigation.PushModalAsync(new Issue1461Page(FlyoutLayoutBehavior.SplitOnLandscape, null));
		}

		//#if UITEST
		//		[Test]
		//		[UiTest (typeof (FlyoutPage), "IsPresented")]
		//		[UiTest (typeof (FlyoutPage), "Flyout")]
		//		public void Test3 ()
		//		{
		//			if (Issue1461Helpers.ShouldRunTest(RunningApp)) {
		//				RunningApp.SetOrientationLandscape ();
		//				var query = RunningApp.Query (q => q.Marked ("Master_Label"));
		//				Assert.IsTrue (query.Any (), "Flyout should be present");
		//				RunningApp.Screenshot ("Flyout should not present");
		//				RunningApp.SetOrientationPortrait ();
		//				query = RunningApp.Query (q => q.Marked ("Master_Label"));
		//				Assert.IsTrue (!query.Any (), "Flyout should not present");
		//				RunningApp.Screenshot ("Flyout should not present");
		//				RunningApp.Tap (q => q.Marked ("Go Back"));
		//			} else {
		//				Assert.Inconclusive ("Only run on iOS Tablet");
		//			}
		//		}
		//#endif
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1461, "1461 - (Splitview in Portrait)", PlatformAffected.iOS)]
	public class Issue1461C : TestContentPage
	{
		protected override async void Init()
		{
			await Navigation.PushModalAsync(new Issue1461Page(FlyoutLayoutBehavior.SplitOnPortrait, null));
		}

		//#if UITEST
		//		[Test]
		//		[UiTest (typeof (FlyoutPage), "IsPresented")]
		//		[UiTest (typeof (FlyoutPage), "Flyout")]
		//		public void Test4 ()
		//		{
		//			if (Issue1461Helpers.ShouldRunTest(RunningApp)) {
		//				RunningApp.SetOrientationPortrait ();
		//				var s = RunningApp.Query (q => q.Marked ("Master_Label"));
		//				Assert.IsTrue (s.Any (), "Flyout should be present");
		//				RunningApp.Screenshot ("Flyout should  present");

		//				RunningApp.SetOrientationLandscape ();
		//				s = RunningApp.Query (q => q.Marked ("Master_Label"));
		//				Assert.IsTrue (!s.Any (), "Flyout should not present on landscape");
		//				RunningApp.Screenshot ("Flyout should not present");
		//				RunningApp.Tap (q => q.Marked ("Go Back"));
		//			} else {
		//				Assert.Inconclusive ("Only run on iOS Tablet");
		//			}
		//		}
		//#endif
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1461, "1461 - (Splitview in Portrait and Landscape)", PlatformAffected.iOS)]
	public class Issue1461D : TestContentPage
	{
		protected override async void Init()
		{
			await Navigation.PushModalAsync(new Issue1461Page(FlyoutLayoutBehavior.Split, null));
		}

		//#if UITEST
		//		[Test]
		//		[UiTest (typeof (FlyoutPage), "IsPresented")]
		//		[UiTest (typeof (FlyoutPage), "Flyout")]
		//		public void Test5 ()
		//		{
		//			if (Issue1461Helpers.ShouldRunTest(RunningApp)) {
		//				RunningApp.SetOrientationPortrait ();
		//				var s = RunningApp.Query (q => q.Marked ("Master_Label"));
		//				Assert.IsTrue (s.Any (), "Flyout should be present");
		//				RunningApp.Screenshot ("Flyout should be present");

		//				RunningApp.SetOrientationLandscape ();
		//				s = RunningApp.Query (q => q.Marked ("Master_Label"));

		//				Assert.IsTrue (s.Any (), "Flyout should  be present");
		//				RunningApp.Screenshot ("Flyout should be present");
		//				RunningApp.Tap (q => q.Marked ("Go Back"));
		//			} else {
		//				Assert.Inconclusive ("Only run on iOS Tablet");
		//			}
		//		}
		//#endif
	}

	internal sealed class Issue1461Page : FlyoutPage
	{
		public Issue1461Page()
			: this(FlyoutLayoutBehavior.Default, null)
		{ }

		bool? _showButton;
		public Issue1461Page(FlyoutLayoutBehavior state, bool? initState)
		{

			var btn = new Button { Text = "hide me" };
			btn.Clicked += bnToggle_Clicked;
			Flyout = new ContentPage
			{
				Title = string.Format("Flyout sample for {0}", state),
				IconImageSource = "bank.png",
				Padding = Device.RuntimePlatform == Device.iOS ? new Thickness(5, 60, 5, 5) : new Thickness(5),
				Content =
					new StackLayout
					{
						Children = {
						new Label {
							Text = "Flyout Label",
							AutomationId = "Master_Label",
							BackgroundColor = Colors.Gray
						},
						btn
					},
						BackgroundColor = Colors.WhiteSmoke
					},
				BackgroundColor = Colors.Gray
			};

			if (initState.HasValue)
				_showButton = initState.Value;

			var lbl = new Label
			{
				HorizontalOptions = LayoutOptions.End,
				BindingContext = this
			};
			lbl.SetBinding(Label.TextProperty, "IsPresented");

			var bnToggle = new Button
			{
				Text = "Toggle IsPresented",
			};

			var bnGoBack = new Button
			{
				Text = "Go Back",
			};

			bnGoBack.Clicked += bnGoBack_Clicked;
			bnToggle.Clicked += bnToggle_Clicked;

			Detail = new NavigationPage(new ContentPage
			{
				Title = "Detail Title",
				Content = new StackLayout { Spacing = 10, Children = { lbl, bnToggle, bnGoBack } }
			});

			FlyoutLayoutBehavior = state;
		}

		public override bool ShouldShowToolbarButton()
		{
			if (_showButton.HasValue)
				return _showButton.Value;
			return base.ShouldShowToolbarButton();
		}

		async void bnGoBack_Clicked(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}

		async void bnToggle_Clicked(object sender, EventArgs e)
		{
			try
			{
				IsPresented = !IsPresented;
			}
			catch (InvalidOperationException ex)
			{
				await DisplayAlert("Error", ex.Message, "ok");
			}

		}
	}
}