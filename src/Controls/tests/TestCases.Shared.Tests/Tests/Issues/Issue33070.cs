using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33070 : _IssuesUITest
{
	public override string Issue => "Fix Android drawable mutation crash";

	public Issue33070(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.ActivityIndicator)]
	public void ActivityIndicatorColorChangeShouldNotCrash()
	{
		App.WaitForElement("ActivityIndicator");

		// Change color multiple times
		for (int i = 0; i < 5; i++)
		{
			App.Tap("ChangeActivityIndicatorButton");
			App.WaitForElement("ActivityIndicatorStatus");

			var status = App.FindElement("ActivityIndicatorStatus").GetText();
			Assert.That(status, Does.Contain("Color changed to"), $"Iteration {i + 1}: Color change failed");
		}

		// Verify no crash occurred
		var finalStatus = App.FindElement("ActivityIndicatorStatus").GetText();
		Assert.That(finalStatus, Does.Not.Contain("ERROR"));
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void EntryTextColorChangeShouldNotCrash()
	{
		App.WaitForElement("Entry");

		// Change color multiple times
		for (int i = 0; i < 5; i++)
		{
			App.Tap("ChangeEntryButton");
			App.WaitForElement("EntryStatus");

			var status = App.FindElement("EntryStatus").GetText();
			Assert.That(status, Does.Contain("Color changed to"), $"Iteration {i + 1}: Color change failed");
		}

		// Verify no crash occurred
		var finalStatus = App.FindElement("EntryStatus").GetText();
		Assert.That(finalStatus, Does.Not.Contain("ERROR"));
	}

	[Test]
	[Category(UITestCategories.Switch)]
	public void SwitchColorChangeShouldNotCrash()
	{
		App.WaitForElement("Switch");

		// Change colors multiple times
		for (int i = 0; i < 5; i++)
		{
			App.Tap("ChangeSwitchButton");
			App.WaitForElement("SwitchStatus");

			var status = App.FindElement("SwitchStatus").GetText();
			Assert.That(status, Does.Contain("Thumb:"), $"Iteration {i + 1}: Color change failed");
		}

		// Verify no crash occurred
		var finalStatus = App.FindElement("SwitchStatus").GetText();
		Assert.That(finalStatus, Does.Not.Contain("ERROR"));
	}

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void SearchBarColorChangeShouldNotCrash()
	{
		App.WaitForElement("SearchBar");

		// Change colors multiple times
		for (int i = 0; i < 5; i++)
		{
			App.Tap("ChangeSearchBarButton");
			App.WaitForElement("SearchBarStatus");

			var status = App.FindElement("SearchBarStatus").GetText();
			Assert.That(status, Does.Contain("Colors changed successfully"), $"Iteration {i + 1}: Color change failed");
		}

		// Verify no crash occurred
		var finalStatus = App.FindElement("SearchBarStatus").GetText();
		Assert.That(finalStatus, Does.Not.Contain("ERROR"));
	}

	[Test]
	[Category(UITestCategories.ActivityIndicator)]
	public void RapidColorChangesShouldNotCrash()
	{
		App.WaitForElement("ScrollViewContent");
		App.ScrollDownTo("RunRapidChangesButton", "ScrollViewContent");

		// Run the stress test
		App.Tap("RunRapidChangesButton");

		// Wait for completion (50 iterations with small delays)
		App.WaitForElement("RapidChangesStatus", timeout: TimeSpan.FromSeconds(30));

		// Check that test completed successfully
		var status = App.FindElement("RapidChangesStatus").GetText();
		Assert.That(status, Does.Contain("Completed"), "Rapid changes test did not complete");
		Assert.That(status, Does.Contain("50 iterations"), "Expected 50 iterations");

		// Verify no crashes
		var progress = App.FindElement("RapidChangesProgress").GetText();
		Assert.That(progress, Does.Contain("No crashes!"));
	}
}
