using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30249 : _IssuesUITest
{
	public Issue30249(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Grouped CollectionView does not trigger the Scrolled event for empty groups";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrolledEventForEmptyGroups()
	{
		App.WaitForElement("CollectionViewWithEmptyGroups");
		App.ScrollDown("CollectionViewWithEmptyGroups", ScrollStrategy.Gesture, 0.2, 500);

		var scrolledEventStatus = App.FindElement("ScrolledEventStatusLabel").GetText();
		Assert.That(scrolledEventStatus, Is.EqualTo("Success"));
	}
}