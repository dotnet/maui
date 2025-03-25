using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3139 : _IssuesUITest
{
	public Issue3139(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "DisplayActionSheet is hiding behind Dialogs";

	[Test]
	[Category(UITestCategories.ActionSheet)]
	public void Issue3139Test()
	{
		App.TapDisplayAlertButton("Yes", buttonIndex: 2);
		App.TapDisplayAlertButton("Yes", buttonIndex: 2);
		Assert.That(App.WaitForElementTillPageNavigationSettled("StatusLabel")?.GetText(), Is.EqualTo("Test passed"));
	}
}
