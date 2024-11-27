using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25913 : _IssuesUITest
	{
		public override string Issue => "Top Tab Visibility Changes Not Reflected Until Tab Switch";

		public Issue25913(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.Shell)]
		public void DynamicTabSectionVisibility()
		{
			App.WaitForElement("HideTop3");
			App.Tap("HideTop3");
#if WINDOWS
			App.Tap("Tab 1");
#endif
			VerifyScreenshot();
		}
	}
}
