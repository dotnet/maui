using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17886 : _IssuesUITest
	{
		public Issue17886(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Shadow wrong scaling";

		[Test]
		public void Issue17886Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac },
				"See https://github.com/dotnet/maui/issues/17886");

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
