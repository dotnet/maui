#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1685 : _IssuesUITest
	{
		const string ButtonId = "Button1685";
		const string Success = "Success";

		public Issue1685(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Entry clears when upadting text from native with one-way binding";

		[Test]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		public void EntryOneWayBindingShouldUpdate()
		{
			App.WaitForElement(ButtonId);
			App.Tap(ButtonId);
			App.WaitForNoElement(Success);
		}
	}
}
#endif