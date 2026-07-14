using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33612 : _IssuesUITest
{
	public Issue33612(TestDevice device) : base(device) { }

	public override string Issue => "Inconsistent Accessibility Behavior Across Platforms - a container with SemanticProperties.Description/Hint should not collapse its independently-accessible children";

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void ContainerDescriptionDoesNotHideIndividuallyAccessibleItems()
	{
		App.WaitForElement("SuggestionsContainer");

		// Each item must still be individually reachable by automation/accessibility tooling —
		// before the fix, a Description/Hint on the outer container collapsed the whole subtree
		// into a single opaque element, hiding these nested items entirely.
		App.WaitForElement("First item");
		App.WaitForElement("Second item");
		App.WaitForElement("Third item");
	}

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void TappingSuggestionItemStillInvokesItsOwnGesture()
	{
		App.WaitForElement("SuggestionsContainer");
		App.WaitForElement("First item");

		App.Tap("First item");

		App.WaitForElement("OK");
		App.Tap("OK");

		var tappedItemText = App.WaitForElement("TappedItemLabel").GetText();
		Assert.That(tappedItemText, Is.EqualTo("First item"));
	}
}
