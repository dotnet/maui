using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17490 : _IssuesUITest
	{
		public Issue17490(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Crash using Pinvoke.SetParent to create Window as Child";

		[Test]
		[Category(UITestCategories.Window)]
		public void AppDoesntCrashWhenOpeningWinUIWindowParentedToCurrentWindow()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac, TestDevice.iOS, TestDevice.Android
			});

			App.WaitForElement("SuccessLabel");
		}
	}
}
