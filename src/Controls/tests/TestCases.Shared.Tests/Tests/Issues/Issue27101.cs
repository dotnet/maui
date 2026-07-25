using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27101 : _IssuesUITest
{
	public Issue27101(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "PlatformView cannot be null here Exception in Windows";

	[Test]
	[Category(UITestCategories.Button)]
	public void NoCrashWhenNavigatingBackFromPageWithFocusedButton()
	{
		// The crash reproduces only when a Button with VisualStates currently holds focus
		// at the moment the page is navigated away from. Repeat the navigation a few times
		// to give the focus race a chance to surface.
		for (int i = 0; i < 10; i++)
		{
			App.WaitForElement("NavigateButton");
			App.Tap("NavigateButton");

			App.WaitForElement("NavigateBackButton");
			App.Tap("NavigateBackButton");
		}

		// If we got here without an unhandled InvalidOperationException, the regression is fixed.
		App.WaitForElement("NavigateButton");
	}
}
