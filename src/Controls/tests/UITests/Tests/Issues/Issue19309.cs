using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19309 : _IssuesUITest
	{
		public Issue19309(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Nightly build 9725 broke boxviews do not get their color set anymore";

		[Test]
		public async Task EditorIsReadOnlyPreventModify()
		{
			App.WaitForElement("WaitForStubControl");

			GC.Collect();
			GC.WaitForPendingFinalizers();
			await Task.Yield();

			App.Screenshot("All BoxViews must render.");
		}
	}
}