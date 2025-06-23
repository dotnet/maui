#if TEST_FAILS_ON_WINDOWS // For more information, see : https://github.com/dotnet/maui/issues/27638
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12079 : _IssuesUITest
	{
		const string SwipeViewId = "SwipeViewId";

		public Issue12079(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SwipeView crash if Text not is set on SwipeItem";

		[Test]
		[Category(UITestCategories.SwipeView)]
		[Category(UITestCategories.Compatibility)]
		public void SwipeItemNoTextWindows()
		{
			App.WaitForElement(SwipeViewId);
			App.SwipeLeftToRight(SwipeViewId);
			App.Tap(SwipeViewId);
			App.WaitForElement("Success");
		}
	}
}
#endif