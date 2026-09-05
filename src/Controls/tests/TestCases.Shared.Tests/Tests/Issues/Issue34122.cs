#if TEST_FAILS_ON_WINDOWS // EmptyView elements are not accessible via Automation on Windows. Issue Link: https://github.com/dotnet/maui/issues/28022
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34122 : _IssuesUITest
{
	const string BasicEmptyViewButtonId = "BasicEmptyViewButton";
	const string AdvancedEmptyViewButtonId = "AdvancedEmptyViewButton";

	public override string Issue => "I5_EmptyView_Swap - Continuously turning the Toggle EmptyViews on and off would cause an item from the list to show up";

	public Issue34122(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void EmptyViewSwapShouldNotRevealFilteredOutItems()
	{
		// Verify items are visible initially
		App.WaitForElement("Baboon");

		// Clear all items one-by-one (matches the original ManualTests FilterCommand),
		// which fires multiple CollectionChanged events and puts RecyclerView into
		// the state that triggers the EmptyView-swap bug.
		App.WaitForElement("FilterButton");
		App.Tap("FilterButton");
		AssertEmptyViewState(AdvancedEmptyViewButtonId, BasicEmptyViewButtonId);

		App.WaitForElement("ToggleEmptyViewButton");

		for (int i = 1; i <= 8; i++)
		{
			App.Tap("ToggleEmptyViewButton");

			if (i % 2 == 1) // odd → BasicEmptyView
			{
				AssertEmptyViewState(BasicEmptyViewButtonId, AdvancedEmptyViewButtonId);
			}
			else // even → AdvancedEmptyView
			{
				AssertEmptyViewState(AdvancedEmptyViewButtonId, BasicEmptyViewButtonId);
			}
		}
	}

	void AssertEmptyViewState(string expectedEmptyViewButtonId, string unexpectedEmptyViewButtonId)
	{
		App.WaitForElement(expectedEmptyViewButtonId,
			$"Expected empty view '{expectedEmptyViewButtonId}' was not visible.");

		App.WaitForNoElement(unexpectedEmptyViewButtonId,
			$"'{unexpectedEmptyViewButtonId}' should not be visible while '{expectedEmptyViewButtonId}' is active.");
	}
}
#endif
