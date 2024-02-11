using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19127 : _IssuesUITest
	{
		public override string Issue => "[iOS] Triggers are not working on Frame control";

		public Issue19127(TestDevice device) : base(device)
		{
		}

		[Test]
		public void ContentOfFrameShouldChange()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });

			_ = App.WaitForElement("button");

			App.Click("button");

			var text = App.FindElement("label").GetText();

			Assert.AreEqual(text, "Camera is Enabled");
		}
	}
}
