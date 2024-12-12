#if WINDOWS //TitleBar is only available on Windows platforms.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26396 : _IssuesUITest
	{
		public Issue26396(TestDevice device): base(device)
		{ 
		}

		public override string Issue => "Setting Window.TitleBar to null does not remove the customization";

		[Test]
		[Category(UITestCategories.Shell)]
		public void VerifyTitleBarUpdatesOnPageChange() 
		{
			App.WaitForElement("FirstPageButton");
			App.Tap("FirstPageButton");
			App.Tap("SecondPageButton");
			VerifyScreenshot();
		}
	}
}
#endif