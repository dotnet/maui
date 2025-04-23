using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6260 : _IssuesUITest
	{
		const string Text = "If this number keeps increasing test has failed: ";
		readonly string success = Text + "0";

		public Issue6260(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] infinite layout loop";

		[Test]
		[Category(UITestCategories.Layout)]
		public void NonAppCompatBasicSwitchTest()
		{
			App.WaitForElement(success);
		}
	}
}