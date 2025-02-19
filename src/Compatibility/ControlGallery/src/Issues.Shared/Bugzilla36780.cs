
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
	[Category(UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 36780, "[iOS] Multiple TapGestureRecognizers on an Object Are Not Fired", PlatformAffected.iOS)]
	public class Bugzilla36780 : TestContentPage
	{
		const string Gesture1Success = "Gesture1Success";
		const string Gesture2Success = "Gesture2Success";
		const string Gesture3Success = "Gesture3Success";
		const string Waiting = "Waiting";
		const string TestImage = "TestImage";

		protected override void Init()
		{
			var gesture1Label = new Label { FontSize = 18, Text = Waiting };
			var gesture2Label = new Label { FontSize = 18, Text = Waiting };
			var gesture3Label = new Label { FontSize = 18, Text = Waiting };

			var testImage = new Image { Source = "coffee.png", AutomationId = TestImage, HeightRequest = 75 };

			testImage.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					gesture1Label.Text = Gesture1Success;
				})
			});

			testImage.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					gesture2Label.Text = Gesture2Success;
				})
			});

			testImage.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					gesture3Label.Text = Gesture3Success;
				})
			});

			Content = new StackLayout
			{
				Padding = new Thickness(0, 20, 0, 0),
				Children =
				{
					gesture1Label,
					gesture2Label,
					gesture3Label,
					testImage
				}
			};
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void MultipleTapGestures()
		{
			RunningApp.WaitForElement(TestImage);
			RunningApp.Tap(TestImage);

			RunningApp.WaitForElement(Gesture1Success);
			RunningApp.WaitForElement(Gesture2Success);
			RunningApp.WaitForElement(Gesture3Success);
		}
#endif
	}
}