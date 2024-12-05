#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11333 : _IssuesUITest
	{
		public Issue11333(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] SwipeView does not work on Android if child has TapGestureRecognizer";

		const string SwipeViewId = "SwipeViewId";

		[Test]
		[Category(UITestCategories.SwipeView)]
		[Category(UITestCategories.Compatibility)]
		public void SwipeWithChildGestureRecognizer()
		{
			App.WaitForElement(SwipeViewId);
			App.SwipeRightToLeft();
			App.Tap(SwipeViewId);
			App.WaitForElement("ResultLabel");
		}
	}
}
#endif