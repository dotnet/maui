using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19081 : _IssuesUITest
	{
		public override string Issue => "[iOS] RadioButton text related properties not working";

		public Issue19081(TestDevice device) : base(device)
		{
		}

		[Test]
		public void Issue19081Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Windows });

			_ = App.WaitForElement("radioButton");

			//Both radio buttons should look the same
			VerifyScreenshot();
		}
	}
}
