#if IOS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23195 : _IssuesUITest
	{
		public override string Issue => "NavigationBarColors from NavigationPage not changing on AppTheme changing";

		public Issue23195(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Page)]
		public void Issue23195Test()
		{
			App.WaitForElement("button");
			App.Click("button");
			App.WaitForElement("label");
			VerifyScreenshot();
		}
	}
}
#endif