using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	class Issue16919 : _IssuesUITest
	{
		string buttonId = "TestButton";

		public Issue16919(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Shapes without handlers shouldn't be added as LogicalChildren";

		[Test]
		public void StrokeShapeHandlerIsNotNull()
		{
			App.WaitForElement(buttonId);
			App.Click(buttonId);
			var result = App.FindElement(buttonId).GetText();

			Assert.AreEqual("Passed", result);
		}
	}
}
