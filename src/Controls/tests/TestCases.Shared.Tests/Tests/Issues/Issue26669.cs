using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26669(TestDevice device) : _IssuesUITest(device)
{
	public override string Issue => "[Windows] CollectionView ScrollTo method crashes on with invalid values";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void InvalidScrollToGroupIndexShouldNotCrash()
	{
		App.WaitForElement("GoButton");
		App.Click("GoButton");
		App.WaitForElement("CollectionView");
	}
}