using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40092, "Ensure android devices with fractional scale factors (3.5) don't have a white line around the border"
		, PlatformAffected.Android)]
#if UITEST
	[Category(UITestCategories.BoxView)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	public class Bugzilla40092 : TestContentPage
	{
		const string Black = "black";
		const string White = "white";
		const string Ok = "Ok";
		protected override void Init()
		{
			AbsoluteLayout mainLayout = new AbsoluteLayout()
			{
				BackgroundColor = Colors.White,
				AutomationId = White
			};


			// The root page of your application

			var thePage = new ContentView
			{
				BackgroundColor = Colors.Red,
				Content = mainLayout
			};

			BoxView view = new BoxView()
			{
				Color = Colors.Black,
				AutomationId = Black
			};

			mainLayout.Children.Add(view, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
			Content = thePage;

		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await DisplayAlert("Instruction", "If you see just the black color, the test pass. (Ignore the navigation bar)", Ok);
		}


#if UITEST && __ANDROID__
		[Test]
		public void AllScreenIsBlack()
		{
			RunningApp.WaitForElement(Ok);
			RunningApp.Tap(Ok);
			var box = RunningApp.WaitForElement(Black)[0];
			var layout = RunningApp.WaitForElement(White)[0];

			var assert = box.Rect.Height == layout.Rect.Height &&
				box.Rect.Width == layout.Rect.Width;

			Assert.IsTrue(assert);
		}
#endif
	}
}
