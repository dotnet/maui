using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

internal class Issue29588 : _IssuesUITest
{
	public override string Issue => "CollectionView RemainingItemsThresholdReachedcommand should trigger on scroll near end";

	public Issue29588(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void RemainingItemsThresholdReachedEventShouldTrigger()
	{
		App.WaitForElement("29588CollectionView");
		for (int i = 0; i < 5; i++)
		{
			App.ScrollDown("29588CollectionView", ScrollStrategy.Gesture, 0.8, 500);
		}
		App.WaitForElement("29588ThresholdLabel");
		Assert.That(App.FindElement("29588ThresholdLabel").GetText(), Is.EqualTo("Threshold reached"));
	}
}
