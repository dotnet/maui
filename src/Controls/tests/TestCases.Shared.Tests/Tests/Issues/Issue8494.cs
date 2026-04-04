using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8494 : _IssuesUITest
{
	public Issue8494(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Margin doesn't work inside CollectionView EmptyView";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CheckEmptyViewMargin()
	{
		App.WaitForElement("EmptyViewDescriptionLabel");
		VerifyScreenshot();
	}
}