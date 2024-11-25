using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19127 : _IssuesUITest
	{
		public override string Issue => "Triggers are not working on Frame control";

		public Issue19127(TestDevice device) : base(device)
		{
		}

		[Test]
		public void ContentOfFrameShouldChange()
		{
			_ = App.WaitForElement("button");

			var textBeforeClick = App.FindElement("label1").GetText();

			App.Click("button");

			var textAfterClick = App.FindElement("label2").GetText();

			Assert.AreEqual(textBeforeClick, "Camera is Disabled");
			Assert.AreEqual(textAfterClick, "Camera is Enabled");
		}
	}
}
