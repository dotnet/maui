using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35492 : _IssuesUITest
{
	public override string Issue => "Border.StrokeDashArray leaks dashed Borders when using a shared Application resource";

	public Issue35492(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Border)]
	public void SharedDashArray_PushedPagesAreCollectedAfterForceGC()
	{
		App.WaitForElement("PushPageButton");
		App.Tap("PushPageButton");
		App.WaitForElement("TestCollectionView");
		App.TapBackArrow();
		App.WaitForElement("PushPageButton");
		App.Tap("ForceGCButton");
		bool gcCompleted = App.WaitForTextToBePresentInElement("SummaryLabel", "Alive count: 0/1");
		Assert.That(gcCompleted, Is.True, "Expected all pushed pages to be collectable after forcing GC.");
	}
}
