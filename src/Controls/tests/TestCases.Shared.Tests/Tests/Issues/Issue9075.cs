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

#if WINDOWS
	[Test]
	[Category(UITestCategories.Layout)]
#endif
	public void FlexLayoutCycleException()
	{
		App.WaitForElement("Item2");

		// Without exceptions, the test has passed.
	}
}