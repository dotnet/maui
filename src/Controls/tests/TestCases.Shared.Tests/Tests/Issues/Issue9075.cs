using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9075 : _IssuesUITest
{
	public override string Issue => "FlexLayout trigger Cycle GUI exception";

	public Issue9075(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Layout)]

	public void FlexLayoutCycleException()
	{
		// First verify the status label loaded (simple element outside CarouselView)
		App.WaitForElement("TestStatusLabel");

		// The test passes if the page loads without exceptions from FlexLayout.
		// We don't need to verify nested items - the bug was about layout exceptions.
	}
}