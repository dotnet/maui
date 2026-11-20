using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue40332 : _IssuesUITest
{
	public Issue40332(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[CollectionView] ItemTapped event - Parity with ListView";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void ItemTappedIsRaisedWhenTheSelectedItemIsTappedAgain()
	{
		ResetPage();

		App.Tap("ItemB");
		WaitForLog("Selection:B;Tapped:B@1");

		TapAgain("ItemB");
		WaitForLog("Selection:B;Tapped:B@1;Tapped:B@1");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void TappingAnotherItemInSelectionModeSingleReportsOnlyTheTappedItem()
	{
		ResetPage();

		App.Tap("ItemA");
		WaitForLog("Selection:A;Tapped:A@0");

		App.Tap("ItemB");
		WaitForLog("Selection:A;Tapped:A@0;Selection:B;Tapped:B@1");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void ItemTappedIsRaisedWithoutSelectionInSelectionModeNone()
	{
		ResetPage();
		App.Tap("ModeNone");

		App.Tap("ItemC");
		WaitForLog("Tapped:C@2");

		TapAgain("ItemC");
		WaitForLog("Tapped:C@2;Tapped:C@2");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void ItemTappedFollowsSelectionAndDeselectionInSelectionModeMultiple()
	{
		ResetPage();
		App.Tap("ModeMultiple");

		App.Tap("ItemA");
		WaitForLog("Selection:A;Tapped:A@0");

#if MACCATALYST
		// On Mac Catalyst a plain click replaces the selection (https://github.com/dotnet/maui/issues/18028): UIKit
		// deselects A and selects B, which is still a single tap
		App.Tap("ItemB");
		WaitForLog("Selection:A;Tapped:A@0;Selection:;Selection:B;Tapped:B@1");
#else
		App.Tap("ItemB");
		WaitForLog("Selection:A;Tapped:A@0;Selection:A,B;Tapped:B@1");

		// Deselecting is also a tap
		App.Tap("ItemA");
		WaitForLog("Selection:A;Tapped:A@0;Selection:A,B;Tapped:B@1;Selection:B;Tapped:A@0");
#endif
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void ItemTappedIsNotRaisedWhenAControlInsideTheItemHandlesTheTap()
	{
		ResetPage();
		App.Tap("ModeNone");

		App.Tap("InnerB");
		WaitForLog("Inner:B");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void GroupedItemIndexMatchesListView()
	{
		ResetPage();
		App.Tap("Grouped");

		// G1 header = 0, A = 1, B = 2, G2 header = 3, C = 4, D = 5
		App.Tap("ItemD");
		WaitForLog("Selection:D;Tapped:D@5/G2");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void ItemsAreNotInteractiveInSelectionModeNoneWithoutItemTappedHandlers()
	{
		ResetPage();
		App.Tap("ModeNone");
		App.Tap("Unsubscribe");

		App.Tap("ItemA");
		App.Tap("InnerB");
		WaitForLog("Inner:B");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void RepeatedItemsReportTheTappedRow()
	{
		ResetPage();
		App.Tap("Duplicates");

		App.WaitForElement("ItemA");
		var rows = App.FindElements("ItemA");
		Assert.That(rows, Has.Count.EqualTo(2));
		rows.Last().Tap();
		WaitForLog("Selection:A;Tapped:A@2");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void ItemTappedIsRaisedWithoutAnItemTemplate()
	{
		ResetPage();
		App.Tap("NoTemplate");

		// Without an item template the rows only show the items' text
		App.WaitForElement("Plain2").Tap();
		WaitForLog("Selection:Plain2;Tapped:Plain2@1");

		TapAgain("Plain2");
		WaitForLog("Selection:Plain2;Tapped:Plain2@1;Tapped:Plain2@1");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void ItemTappedCommandReceivesTheTappedItem()
	{
		ResetPage();
		App.Tap("ModeNone");
		App.Tap("UseCommand");

		App.Tap("ItemB");
		WaitForLog("Command:B");
	}

	// Wait out the double-click interval, so that tapping the same item again is a second tap rather than a
	// double click; on Mac Catalyst UIKit reports a double click on an item as three selection changes
	void TapAgain(string element)
	{
		Thread.Sleep(600);
		App.WaitForElement(element).Tap();
	}

	void ResetPage()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");
		WaitForLog(string.Empty);
	}

	void WaitForLog(string expected)
	{
		expected = "Log:" + expected;
		var log = string.Empty;

		for (var attempt = 0; attempt < 20; attempt++)
		{
			log = App.FindElement("EventLog").GetText() ?? string.Empty;

			if (log == expected)
			{
				return;
			}

			Thread.Sleep(250);
		}

		Assert.That(log, Is.EqualTo(expected));
	}
}
