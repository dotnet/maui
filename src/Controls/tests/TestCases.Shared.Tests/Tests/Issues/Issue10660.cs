using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10660 : _IssuesUITest
	{
		public override string Issue => "Inconsistent toolbar text color on interaction";

		public Issue10660(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarTextColorOnInteraction()
		{
			App.WaitForElement("ChangeText");
			App.Tap("ChangeText");

			VerifyScreenshot();
		}
	}
}
