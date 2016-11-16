using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 38658, "Rotation causes app containing CarouselPage to freeze", PlatformAffected.iOS)]
	public class Bugzilla38658 : TestTabbedPage // or TestMasterDetailPage, etc ...
	{
		public class TestCarouselPage : CarouselPage
		{
			public TestCarouselPage ()
			{
				Children.Add (new ContentPage () {
					Content = new StackLayout {
						Children = {
							new Label { Text = "Rotate the device to Landscape and back to Portrait. If the app hangs, this test has failed." },
							new BoxView {  Color = Color.Red, HeightRequest = 200, WidthRequest = 200 }
						}
					}
				});
				Children.Add (new ContentPage () {
					Content = new StackLayout {
						Children = {
							new Label { Text = "Rotate the device to Landscape and back to Portrait. If the app hangs, this test has failed." },
							new BoxView {  Color = Color.Green, HeightRequest = 200, WidthRequest = 200 }
						}
					}
				});
			}
		}
		public class StartPage : ContentPage
		{
			public StartPage ()
			{
				Button button = new Button {
					AutomationId = "btn",
					Text = "Click"
				};
				button.Clicked += button_Clicked;
				Content = button;
			}

			async void button_Clicked (object sender, EventArgs e)
			{
				await Navigation.PushAsync (new TestCarouselPage ());
			}
		}

		protected override void Init ()
		{
			Children.Add (new NavigationPage (new StartPage () { Title = "Page" }));
		}

#if UITEST
		[Test]
		public void Bugzilla38658Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("btn"));
			RunningApp.Tap (q => q.Marked ("btn"));
			RunningApp.SetOrientationLandscape ();
			RunningApp.SetOrientationPortrait ();
			RunningApp.Back ();
			RunningApp.WaitForElement (q => q.Marked ("btn"));
		}

		[TearDown]
		public void TearDown() 
		{
			RunningApp.SetOrientationPortrait ();
		}
#endif
	}
}