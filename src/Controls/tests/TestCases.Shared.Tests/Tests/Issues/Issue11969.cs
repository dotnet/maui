
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11969 : _IssuesUITest
	{
		const string SwipeViewId = "SwipeViewId";
		const string SwipeButtonId = "SwipeButtonId";
		const string TestPassId = "TestPassId";
		const string SwipeViewCheckBoxId = "SwipeViewCheckBoxId";
		const string SwipeViewContentCheckBoxId = "SwipeViewContentCheckBoxId";

		const string Failed = "SwipeView Button not tapped";
		const string Success = "SUCCESS";

		public Issue11969(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Disabling Swipe view not handling tap gesture events on the content in iOS of Xamarin Forms";

		[Test]
		[Category(UITestCategories.SwipeView)]
		[Category(UITestCategories.Compatibility)]
		public void SwipeDisableChildButtonTest()
		{
			App.WaitForElement(TestPassId);
			App.WaitForElement("Item 1");
			App.WaitForElement(SwipeViewCheckBoxId);
			App.Tap(SwipeViewCheckBoxId);

			App.WaitForElement(SwipeViewContentCheckBoxId);
			App.Tap(SwipeViewContentCheckBoxId);

			App.WaitForElement(SwipeButtonId);
			App.Tap(SwipeButtonId);

			App.WaitForElement("Ok");
			App.Tap("Ok");
			App.WaitForElement(TestPassId);
		}
	}
}
