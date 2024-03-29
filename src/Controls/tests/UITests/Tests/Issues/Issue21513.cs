using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue21513 : _IssuesUITest
	{
		public Issue21513(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Buttons with images don't cover text";

		[Test]
		public void Issue21513Test()
		{
			this.IgnoreIfPlatforms(new [] { TestDevice.Mac, TestDevice.iOS });

			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}
