using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24864 : _IssuesUITest
	{
		public override string Issue => "Button control: inconsistent border visibility behaviour iOS vs Android when BackgroundColor is Transparent";

		public Issue24864(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Button)]
		public void ButtonShouldHaveTransparentBorder()
		{
			App.WaitForElement("label");
			VerifyScreenshot();
		}
	}
}