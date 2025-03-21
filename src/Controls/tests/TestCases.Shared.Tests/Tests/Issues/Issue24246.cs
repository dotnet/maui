using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24246 : _IssuesUITest
	{
		public Issue24246(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SafeArea arrange insets are currently insetting based on an incorrect Bounds";

		[Test]
		[Category(UITestCategories.Layout)]
		public void SafeAreaInsetsCorrectlyForMeasureAndArrangePass()
		{
			App.WaitForElement("entry");
			App.EnterText("entry", "Hello, World!");

			var result = App.WaitForElement("entry").GetText();
			Assert.That(result, Is.EqualTo("Hello, World!"));

			App.WaitForElement("button").Tap();
			App.WaitForElement("Success");
		}
	}
}