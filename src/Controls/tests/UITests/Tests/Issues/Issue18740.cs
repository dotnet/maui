using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18740 : _IssuesUITest
	{
		public Issue18740(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Virtual keyboard appears with focus on Entry";

		[Test]
		public async Task Issue18740Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows });
			
			App.WaitForElement("WaitForStubControl");

			// 1.Make sure keyboard starts out closed.
			VerifyScreenshot("Issue18740Unfocused");

			// 2. Focus the Entry.
			App.Click("TestEntry");

			await Task.Delay(500);

			// 3. Verify that the virtual keyboard appears.
			VerifyScreenshot("Issue18740Focused");
		}
	}
}
