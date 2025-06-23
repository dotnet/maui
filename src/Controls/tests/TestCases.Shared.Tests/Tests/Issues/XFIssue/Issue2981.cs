using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2981 : _IssuesUITest
{
	public Issue2981(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Long Press on ListView causes crash";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Issue2981Test()
	{
		App.WaitForElement("Cell1");

#if MACCATALYST
		App.LongPress("Cell1");
		App.LongPress("Cell2");
#else
		App.TouchAndHold("Cell1");
		App.TouchAndHold("Cell2");
#endif
	}
}