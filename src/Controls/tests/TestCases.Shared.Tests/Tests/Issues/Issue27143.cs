#if IOS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27143 : _IssuesUITest
	{
		public Issue27143(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Not trigger OnNavigatedTo method when hide the navi bar and using swipe";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void OnNavigatedToShouldTrigger()
		{
			App.WaitForElement("button");
			App.Click("button");
			App.SwipeBackNavigation();
			App.WaitForElement("NavigatedTo event triggers count: 2");
		}
	}
}
#endif