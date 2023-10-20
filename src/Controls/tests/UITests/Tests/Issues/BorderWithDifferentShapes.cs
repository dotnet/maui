using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	class BorderWithDifferentShapes : _IssuesUITest
	{
		public BorderWithDifferentShapes(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Validate Border using different shapes";

		[Test]
		public void BorderWithDifferentShapesTest()
		{
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
