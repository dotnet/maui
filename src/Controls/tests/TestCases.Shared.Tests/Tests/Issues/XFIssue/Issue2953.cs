using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2953 : _IssuesUITest
{
	public Issue2953(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "GroupHeaderCells disappear when item is removed from a group in ListView (iOS only) ";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Issue2953Test()
	{
		App.WaitForElement("Header 3");
		App.Tap("btnRemove");
		App.WaitForElement("Header 3");
	}
}