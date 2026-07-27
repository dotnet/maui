using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33333 : _IssuesUITest
{
	public override string Issue => "CollectionView Scrolled event is triggered on the initial app load";

	public Issue33333(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewScrolledEventShouldNotFireOnInitialLoad()
	{
		App.WaitForElement("ScrollCountLabel");

		var scrollCountText = App.FindElement("ScrollCountLabel").GetText();
		Assert.That(scrollCountText, Is.EqualTo("Scrolled Event Count: 0"),
			"Scrolled event should not fire on initial load without user interaction");
	}
}