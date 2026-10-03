#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37323 : _IssuesUITest
{
	public Issue37323(TestDevice device) : base(device)
	{
	}

	public override string Issue => "ScrollView padding updates through binding";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void UpdatingBoundPaddingMovesRenderedScrollViewContent()
	{
		var scrollViewBounds = App.WaitForElement("TestScrollView").GetRect();
		var initialContentBounds = App.WaitForElement("ScrollContent").GetRect();
		var initialHorizontalInset = initialContentBounds.X - scrollViewBounds.X;
		var initialVerticalInset = initialContentBounds.Y - scrollViewBounds.Y;
		App.Tap("ApplyPadding");
		App.WaitForTextToBePresentInElement("Status", "Padding: 40");
		var updatedScrollViewBounds = App.WaitForElement("TestScrollView").GetRect();
		var updatedContentBounds = App.WaitForElement("ScrollContent").GetRect();
		var updatedHorizontalInset = updatedContentBounds.X - updatedScrollViewBounds.X;
		var updatedVerticalInset = updatedContentBounds.Y - updatedScrollViewBounds.Y;
		Assert.Multiple(() =>
		{
			Assert.That(updatedHorizontalInset, Is.GreaterThan(initialHorizontalInset + 10),
				"The rendered content should move right when ScrollView padding changes.");
			Assert.That(updatedVerticalInset, Is.GreaterThan(initialVerticalInset + 10),
				"The rendered content should move down when ScrollView padding changes.");
		});
	}
}
#endif
