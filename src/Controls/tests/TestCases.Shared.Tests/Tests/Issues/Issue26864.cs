#if MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26864 : _IssuesUITest
{
	public Issue26864(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell Content Title Not Rendering in Full-Screen Mode on Mac Catalyst";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellContentTitleNotRendering()
	{
		App.WaitForElement("Settings");
		App.EnterFullScreen();
		// Wait a little bit to complete the system animation moving the App Window to FullScreen.
        Thread.Sleep(500);
		VerifyScreenshot();
	}
}
#endif