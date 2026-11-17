#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39599 : _IssuesUITest
{
	public Issue39599(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell Navigation Bar Background Color Wrong";

	[Test]
	[Category(UITestCategories.Material3)]
	public void ShellBackgroundColorIsApplied()
	{
		App.WaitForElement("BackgroundColorLabel");
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
