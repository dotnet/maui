using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;
#endif

namespace Xamarin.Forms.Controls.Issues
{

#if UITEST
	public static class Issue1461Helpers
	{
		public static bool ShouldRunTest (IApp app)
		{
			var appAs = app as iOSApp;
			return (appAs != null && appAs.Device.IsTablet);
		}
	}
#endif

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1461, "1461 - (Popover in Portrait and Landscape)", PlatformAffected.iOS)]
	public class Issue1461 : TestContentPage
	{
		protected override async void Init ()
		{
			await Navigation.PushModalAsync (new Issue1461Page (MasterBehavior.Popover, false));
		}

#if UITEST
		[Test]
		[UiTest (typeof (MasterDetailPage), "IsPresented")]
		[UiTest (typeof (MasterDetailPage), "Master")]
		public void Test1 ()
		{
			if (Issue1461Helpers.ShouldRunTest (RunningApp)) {
				RunningApp.SetOrientationLandscape ();
				var query = RunningApp.Query (q => q.Marked ("Master_Label"));
				Assert.IsTrue (!query.Any (), "Master should not present");
				RunningApp.Screenshot ("Master should not present");
				RunningApp.SetOrientationPortrait ();
				query = RunningApp.Query (q => q.Marked ("Master_Label"));
				Assert.IsTrue (!query.Any (), "Master should not present");
				RunningApp.Screenshot ("Master should not present");
				RunningApp.Tap (q => q.Marked ("Go Back"));
			} else {
				Assert.Inconclusive ("Only run on iOS Tablet");
			}
		}
#endif
	}

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1461, "1461 - Default", PlatformAffected.iOS)]
	public class Issue1461A : TestContentPage
	{
		protected override async void Init ()
		{
			await Navigation.PushModalAsync (new Issue1461Page (MasterBehavior.Default, null));
		}

#if UITEST
		[Test]
		[UiTest (typeof (MasterDetailPage), "IsPresented")]
		[UiTest (typeof (MasterDetailPage), "Master")]
		public void Test2 ()
		{
			if (Issue1461Helpers.ShouldRunTest(RunningApp)) {
				RunningApp.SetOrientationLandscape ();
				var query = RunningApp.Query (q => q.Marked ("Master_Label"));
				Assert.IsTrue (query.Any (), "Master should be present");
				RunningApp.Screenshot ("Master should not present");
				RunningApp.SetOrientationPortrait ();
				query = RunningApp.Query (q => q.Marked ("Master_Label"));
				Assert.IsTrue (!query.Any (), "Master should not present");
				RunningApp.Screenshot ("Master should not present");
				RunningApp.Tap (q => q.Marked ("Go Back"));
			} else {
				Assert.Inconclusive ("Only run on iOS Tablet");
			}
		}

		#if UITEST
		[Test]
		[UiTest (typeof (MasterDetailPage), "Button")]
		public void TestButton ()
		{
			if (Issue1461Helpers.ShouldRunTest(RunningApp)) {
				RunningApp.SetOrientationLandscape ();
				RunningApp.WaitForNoElement (q => q.Marked ("bank"));
				RunningApp.SetOrientationPortrait ();
				RunningApp.WaitForElement (q => q.Marked ("bank"));
			} else {
				Assert.Inconclusive ("Only run on iOS Tablet");
			}
		}
		#endif
#endif
	}

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1461, "1461 - (Splitview in Landscape)", PlatformAffected.iOS)]
	public class Issue1461B : TestContentPage
	{
		protected override async void Init ()
		{
			await Navigation.PushModalAsync (new Issue1461Page (MasterBehavior.SplitOnLandscape, null));
		}

#if UITEST
		[Test]
		[UiTest (typeof (MasterDetailPage), "IsPresented")]
		[UiTest (typeof (MasterDetailPage), "Master")]
		public void Test3 ()
		{
			if (Issue1461Helpers.ShouldRunTest(RunningApp)) {
				RunningApp.SetOrientationLandscape ();
				var query = RunningApp.Query (q => q.Marked ("Master_Label"));
				Assert.IsTrue (query.Any (), "Master should be present");
				RunningApp.Screenshot ("Master should not present");
				RunningApp.SetOrientationPortrait ();
				query = RunningApp.Query (q => q.Marked ("Master_Label"));
				Assert.IsTrue (!query.Any (), "Master should not present");
				RunningApp.Screenshot ("Master should not present");
				RunningApp.Tap (q => q.Marked ("Go Back"));
			} else {
				Assert.Inconclusive ("Only run on iOS Tablet");
			}
		}
#endif
	}

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1461, "1461 - (Splitview in Portrait)", PlatformAffected.iOS)]
	public class Issue1461C : TestContentPage
	{
		protected override async void Init ()
		{
			await Navigation.PushModalAsync (new Issue1461Page (MasterBehavior.SplitOnPortrait, null));
		}

#if UITEST
		[Test]
		[UiTest (typeof (MasterDetailPage), "IsPresented")]
		[UiTest (typeof (MasterDetailPage), "Master")]
		public void Test4 ()
		{
			if (Issue1461Helpers.ShouldRunTest(RunningApp)) {
				RunningApp.SetOrientationPortrait ();
				var s = RunningApp.Query (q => q.Marked ("Master_Label"));
				Assert.IsTrue (s.Any (), "Master should be present");
				RunningApp.Screenshot ("Master should  present");

				RunningApp.SetOrientationLandscape ();
				s = RunningApp.Query (q => q.Marked ("Master_Label"));
				Assert.IsTrue (!s.Any (), "Master should not present on landscape");
				RunningApp.Screenshot ("Master should not present");
				RunningApp.Tap (q => q.Marked ("Go Back"));
			} else {
				Assert.Inconclusive ("Only run on iOS Tablet");
			}
		}
#endif
	}

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1461, "1461 - (Splitview in Portrait and Landscape)", PlatformAffected.iOS)]
	public class Issue1461D : TestContentPage
	{
		protected override async void Init ()
		{
			await Navigation.PushModalAsync (new Issue1461Page (MasterBehavior.Split, null));
		}

#if UITEST
		[Test]
		[UiTest (typeof (MasterDetailPage), "IsPresented")]
		[UiTest (typeof (MasterDetailPage), "Master")]
		public void Test5 ()
		{
			if (Issue1461Helpers.ShouldRunTest(RunningApp)) {
				RunningApp.SetOrientationPortrait ();
				var s = RunningApp.Query (q => q.Marked ("Master_Label"));
				Assert.IsTrue (s.Any (), "Master should be present");
				RunningApp.Screenshot ("Master should be present");

				RunningApp.SetOrientationLandscape ();
				s = RunningApp.Query (q => q.Marked ("Master_Label"));

				Assert.IsTrue (s.Any (), "Master should  be present");
				RunningApp.Screenshot ("Master should be present");
				RunningApp.Tap (q => q.Marked ("Go Back"));
			} else {
				Assert.Inconclusive ("Only run on iOS Tablet");
			}
		}
#endif
	}

	internal sealed class Issue1461Page : MasterDetailPage
	{
		public Issue1461Page ()
			: this (MasterBehavior.Default,null)
		{ }

		bool? _showButton;
		public Issue1461Page (MasterBehavior state, bool? initState)
		{

			var btn = new Button { Text = "hide me" };
			btn.Clicked += bnToggle_Clicked;
			Master = new ContentPage {
				Title = string.Format ("Master sample for {0}", state),
				Icon = "bank.png",
				Padding = Device.RuntimePlatform == Device.iOS ? new Thickness(5, 60, 5, 5) : new Thickness(5),
				Content = 
					new StackLayout { Children = {
						new Label {
							Text = "Master Label",
							AutomationId = "Master_Label",
							BackgroundColor = Color.Gray
						},
						btn
					}
				},
				//BackgroundColor = Color.Red
			};

			if(initState.HasValue)
				_showButton = initState.Value;

			var lbl = new Label {
				HorizontalOptions =   LayoutOptions.End, 
				BindingContext = this
			};
			lbl.SetBinding (Label.TextProperty, "IsPresented");

			var bnToggle = new Button {
				Text = "Toggle IsPresented",
			};

			var bnGoBack = new Button {
				Text = "Go Back",
			};

			bnGoBack.Clicked += bnGoBack_Clicked;
			bnToggle.Clicked += bnToggle_Clicked;

			Detail = new NavigationPage( new ContentPage {
				Title = "Detail Title",
				Content = new StackLayout{ Spacing=10, Children= { lbl, bnToggle, bnGoBack} }
			});

			MasterBehavior = state;
		}

		public override bool ShouldShowToolbarButton ()
		{
			if (_showButton.HasValue)
				return _showButton.Value;
			return base.ShouldShowToolbarButton ();
		}

		async void bnGoBack_Clicked (object sender, EventArgs e)
		{
			await Navigation.PopModalAsync ();
		}

		async void bnToggle_Clicked (object sender, EventArgs e)
		{
			try {
				IsPresented = !IsPresented;
			} catch (InvalidOperationException ex) {
				await DisplayAlert ("Error", ex.Message, "ok");
			}
		
		}
	}
}