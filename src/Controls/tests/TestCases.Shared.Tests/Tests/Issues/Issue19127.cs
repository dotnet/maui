using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19127 : _IssuesUITest
	{
		public override string Issue => "Triggers are not working on Frame control";

		public Issue19127(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Frame)]
		public void ContentOfFrameShouldChange()
		{
			_ = App.WaitForElement("button");

			var textBeforeClick = App.FindElement("label1").GetText();

			App.Click("button");

			var textAfterClick = App.FindElement("label2").GetText();

			Assert.That(textBeforeClick!, Is.EqualTo("Camera is Disabled"));
			Assert.That(textAfterClick!, Is.EqualTo("Camera is Enabled"));
		}
	}
}
