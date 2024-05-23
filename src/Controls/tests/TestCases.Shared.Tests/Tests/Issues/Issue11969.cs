#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11969 : _IssuesUITest
	{
		const string SwipeViewId = "SwipeViewId";
		const string SwipeButtonId = "SwipeButtonId";

		const string Failed = "SwipeView Button not tapped";
		const string Success = "SUCCESS";

		public Issue11969(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Disabling Swipe view not handling tap gesture events on the content in iOS of Xamarin Forms";

		[Test]
		[Category(UITestCategories.SwipeView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void SwipeDisableChildButtonTest()
		{
			App.WaitForNoElement(Failed);
			App.WaitForElement(SwipeViewId);
			App.Tap("SwipeViewCheckBoxId");
			App.Tap("SwipeViewContentCheckBoxId");
			App.Tap(SwipeButtonId);
			App.WaitForNoElement(Success);
		}
	}
}
#endif