using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17347 : _IssuesUITest
	{
		public Issue17347(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Setting a new TitleView on an already created page crashes iOS";

		[Test]
		[Category(UITestCategories.Page)]
		public void AppDoesntCrashWhenSettingNewTitleViewOnExistingPage()
		{
			try
			{
				App.WaitForElement("TitleViewLabel4", timeout: TimeSpan.FromSeconds(10));
			}
			finally
			{
				App.WaitForElement("PopMeButton", timeout: TimeSpan.FromSeconds(4)).Click();
			}
		}
	}
}
