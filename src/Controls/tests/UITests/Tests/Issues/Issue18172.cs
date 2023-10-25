using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	class Issue18172 : _IssuesUITest
	{
		public Issue18172(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Shadows not drawing/updating correctly in Windows & cover entire screen";

		[Test]
		public void Issue18172Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS },	
				"Issue only happens on Windows");

			App.WaitForElement("BorderControl");
			VerifyScreenshot();
		}
	}
}
