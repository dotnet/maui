using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35277 : _IssuesUITest
{
	public Issue35277(TestDevice device) : base(device) { }

	public override string Issue => "COMException when restoring a page content after swapping it out";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewContentShouldRestoreWithoutCOMException()
	{
		App.WaitForElement("SwapAndRestoreButton");
		App.Tap("SwapAndRestoreButton");
		// If no COMException is thrown, the original ScrollView content (with the button) restores successfully
		App.WaitForElement("SwapAndRestoreButton");
	}
}
