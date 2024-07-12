#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22386 : _IssuesUITest
{
	public Issue22386(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "[WinUI] Application.Current.CloseWindow Crash When AppWindow.Hide is called in .Net Maui 9.0 Prev 3";

	[Test]
	[Category(UITestCategories.Window)]
	public void CloseHiddenWindowNoCrash()
	{
		App.WaitForElement("UpdateWindowButton");
		App.Tap("UpdateWindowButton");
		App.WaitForElement("Success");
	}
}
#endif