#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25067 : _IssuesUITest
{
	public Issue25067(TestDevice device) : base(device) { }

	public override string Issue => "[Android] SearchHandler default/custom icons";

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerRendersDefaultSearchIconAndClearIcon()
	{
		App.WaitForElement("GoToDefault");

		App.Tap("GoToDefault");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerRendersCustomSearchIconAndClearIcon()
	{
		App.WaitForElement("GoToCustom");

		App.Tap("GoToCustom");

		VerifyScreenshot();
	}
}
#endif
