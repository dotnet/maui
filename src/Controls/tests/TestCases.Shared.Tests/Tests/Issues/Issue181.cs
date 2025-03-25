using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue181 : _IssuesUITest
	{
		public Issue181(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Color not initialized for Label";

		[Test]
		[Category(UITestCategories.Label)]
		[Category(UITestCategories.Compatibility)]
		public void Issue181TestsLabelShouldHaveRedText()
		{
			App.WaitForElement("TestLabel");
			App.Screenshot("Label should have red text");
		}
	}
}