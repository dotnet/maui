using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue55555 : _IssuesUITest
{
	public Issue55555(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Header problem";

	//[Test]
	//[Category(UITestCategories.ListView)]
	//[FailsOnIOS]
	//public void TGroupDisplayBindingPresentRecycleElementTest()
	//{
	//	App.WaitForElement(q => q.Marked("vegetables"));
	//}
}