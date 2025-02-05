using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22682 : _IssuesUITest
	{
		public override string Issue => "[iOS] FontIconSource crash";

		public Issue22682(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.ImageButton)]
		public void AppShouldNotCrash()
		{
			_ = App.WaitForElement("imageButton");

			// The test passes if no crash is observed
		}
	}
}