using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26057 : _IssuesUITest
	{
		public Issue26057(TestDevice device) : base(device) { }

		public override string Issue => "[iOS & Mac] Gradient background size is incorrect when invalidating parent";

		[Test]
		[Category(UITestCategories.Button)]
		public void GradientLayerShouldApplyProperly()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			VerifyScreenshot();
		}
	}
}