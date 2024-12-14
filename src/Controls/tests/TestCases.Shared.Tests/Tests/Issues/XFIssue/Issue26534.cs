using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26534 : _IssuesUITest
{
	public Issue26534(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Exception occurred when using GroupShortNameBinding in Grouped ListView";

	[Test]
	[Category(UITestCategories.ListView)]
	public void VerifyListViewWithGroupShortNameBinding()
	{
		App.WaitForElement("listview");
	}
}