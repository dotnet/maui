using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16967 : _IssuesUITest
	{
		public Issue16967(TestDevice device) : base(device) { }

		public override string Issue => "Existing TextDecorations applied to a Label are not removed when a new TextDecoration value is set after the Label's Text has been modified";

		[Test]
		[Category(UITestCategories.Label)]
		public void VerifyTextDecorationAppliedProperly()
		{
			App.WaitForElement("CheckBox");
			App.Tap("CheckBox");
			App.Tap("Button");
			App.Tap("CheckBox");
			VerifyScreenshot();
		}
	}
}