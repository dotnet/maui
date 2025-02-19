using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest.Queries;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2577, "Hamburger icon not shown when using FormsAppCompatActivity", PlatformAffected.Android)]
	public class Issue2577 : TestFlyoutPage
	{
		const string NavButton = "NavigateButton";
		const string ToggleBackButton = "ToggleBackButton";
		const string MasterList = "MasterList";
		const string ArrowButton = "OK";

		protected override void Init()
		{
			Flyout = new ContentPage
			{
				Title = "master page",
				Content = new ListView { AutomationId = MasterList }
			};

			Detail = new NavigationPage(new DetailPage());
		}

		class DetailPage : ContentPage
		{


			public NavigationPage ParentPage => Parent as NavigationPage;

			public DetailPage()
			{
				var button = new Button { Text = "Click me", AutomationId = NavButton };
				button.Clicked += async (o, s) =>
				{
					var button2 = new Button { Text = "Toggle back button", AutomationId = ToggleBackButton };

					var page = new ContentPage
					{
						Content = new StackLayout
						{
							Children = {
							new Label { Text = "If there is no hamburger button, this test has failed. If you cannot toggle the back arrow, this test has failed." },
							button2
						}
						}
					};

					button2.Clicked += (o2, s2) =>
					{
						NavigationPage.SetHasBackButton(page, !NavigationPage.GetHasBackButton(page));
					};

					NavigationPage.SetHasBackButton(page, false);
					await ParentPage.PushAsync(page);
				};
				Content = button;
			}
		}

#if UITEST && __ANDROID__
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		public void Issue2577Test()
		{
			RunningApp.WaitForElement(NavButton);
			RunningApp.Tap(NavButton);

			RunningApp.WaitForElement(ToggleBackButton, retryFrequency: System.TimeSpan.FromSeconds(3));

			RunningApp.Screenshot("Hamburger menu icon is visible");

			AppResult[] items = RunningApp.Query(ArrowButton);
			Assert.AreNotEqual(items.Length, 0);
			RunningApp.Tap(ArrowButton);
			RunningApp.WaitForElement(MasterList);

			RunningApp.Screenshot("Flyout menu is showing");
			
			RunningApp.SwipeRightToLeft();
			RunningApp.WaitForNoElement(MasterList);

			RunningApp.Tap(ToggleBackButton);

			items = RunningApp.Query(ArrowButton);
			Assert.AreEqual(items.Length, 0);

			RunningApp.Screenshot("Back arrow is showing");

			var backArrow = RunningApp.Query(e => e.Class("Toolbar").Descendant("AppCompatImageButton")).Last();

			RunningApp.TapCoordinates(backArrow.Rect.CenterX, backArrow.Rect.CenterY);

			RunningApp.WaitForElement(NavButton);

			RunningApp.Screenshot("Back at first screen");
		}
#endif
	}
}
