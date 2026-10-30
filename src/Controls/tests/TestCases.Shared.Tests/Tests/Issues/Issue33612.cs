#if !WINDOWS
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

		// AutomationIds are intentionally distinct from the visible text so this can't be satisfied
		// by the inner Label alone — it proves the Border itself is reachable.
		App.WaitForElement("Item1");
		App.WaitForElement("Item2");
		App.WaitForElement("Item3");
	}

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void TappingSuggestionItemStillInvokesItsOwnGesture()
	{
		App.WaitForElement("SuggestionsContainer");
		var firstItem = App.WaitForElement("Item1");

		if (Device == TestDevice.Mac)
		{
			var itemBounds = firstItem.GetRect();
			App.TapCoordinates(itemBounds.CenterX(), itemBounds.CenterY());
		}
		else
		{
			App.Tap("Item1");
		}

		App.WaitForElement("OK");
		App.Tap("OK");
		var tappedItemText = App.WaitForElement("TappedItemLabel").GetText();
		Assert.That(tappedItemText, Is.EqualTo("First item"));
	}
}
#endif
