using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.UITests
{
	public enum Speed
	{
		Slow,
		Fast
	}

	public static class TestHelpers
	{
		public static int ControlGalleryMaxScrolls = 30;

		public static void NavigateBack (this IApp app) 
		{
			app.Tap (PlatformQueries.Back);
		}

		public static void NavigateToTestCases (this IApp app)
		{
			app.Tap (q => q.Marked ("Test Cases"));
			app.WaitForElement (q => q.Marked ("Carousel Async Add Page Issue"));
		}

		public static AppRect MainScreenBounds (this IApp app)
		{
			return app.Query (q => q.Raw ("*"))[0].Rect;
		}

	}
}

