using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
	[Category(UITestCategories.Image)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 57758, "ObjectDisposedException for System.Maui.Platform.Android.FastRenderers.ImageRenderer", PlatformAffected.Android)]
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
		public void RemovingImageWithGestureFromLayoutWithinGestureHandlerDoesNotCrash()
		{
			RunningApp.WaitForElement(ImageId);
			RunningApp.Tap(ImageId);
		}
#endif
	}
}