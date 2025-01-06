#if TEST_FAILS_ON_WINDOWS
// A timeout exception occurred while running this test on Windows. 
// It appears that the ListView is not detectable in the CI environment.
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
#endif