using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18946 : _IssuesUITest
	{
		public override string Issue => "Shell Toolbar items not displayed";

		public Issue18946(TestDevice device) : base(device)
		{ }

		[Test]
		[Category(UITestCategories.Shell)]
		public void ToolbarItemsShouldWork()
		{
			_ = App.WaitForElement("label");

			// The test passes if the text 'hello' is visible in the toolbar
			VerifyScreenshot();
		}
	}
}
