using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
	[Category(UITestCategories.Image)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 57758, "ObjectDisposedException for Microsoft.Maui.Controls.Platform.Android.FastRenderers.ImageRenderer", PlatformAffected.Android)]
	public class Bugzilla57758 : TestContentPage
	{
		const string ImageId = "TestImageId";

		protected override void Init()
		{
			var testImage = new Image { Source = "coffee.png", AutomationId = ImageId };

			var layout = new StackLayout
			{
				Padding = new Thickness(0, 20, 0, 0),
				Children =
				{
					testImage
				}
			};

			var tapGesture = new TapGestureRecognizer
			{
				NumberOfTapsRequired = 1,
				Command = new Command(() => layout.Children.Remove(testImage))
			};

			testImage.GestureRecognizers.Add(tapGesture);

			Content = layout;
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void RemovingImageWithGestureFromLayoutWithinGestureHandlerDoesNotCrash()
		{
			RunningApp.WaitForElement(ImageId);
			RunningApp.Tap(ImageId);
		}
#endif
	}
}