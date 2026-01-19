using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

// These tests verify that theme changes during window close do not cause ObjectDisposedException.
// The bug (Issue #33352) occurred when TraitCollectionDidChange accessed a disposed service provider during window disposal.
// The fix removes the problematic TraitCollectionDidChange override from ShellSectionRootRenderer.
// The underlying code (ShellSectionRootRenderer.cs) applies to both iOS and MacCatalyst.
public class Issue33352 : _IssuesUITest
{
	public override string Issue => "Intermittent crash on exit on MacCatalyst - ObjectDisposedException in ShellSectionRootRenderer";

	public Issue33352(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void TraitCollectionDidChangeAfterDisposeDoesNotCrash()
	{
		// Wait for the page to load
		App.WaitForElement("TestDisposedRendererButton");

		// This test directly reproduces the bug by:
		// 1. Opening a Shell window and capturing a reference to the ShellSectionRootRenderer
		// 2. Closing the window (which disposes the renderer and services)
		// 3. Calling TraitCollectionDidChange on the disposed renderer
		// Without the fix, this throws ObjectDisposedException.
		App.Tap("TestDisposedRendererButton");

		// Wait for the test to complete (opens window, captures renderer, closes window, calls TraitCollectionDidChange)
		Task.Delay(3000).Wait();

		// Verify the result - check the ResultLabel
		App.WaitForElement("ResultLabel");
		var resultText = App.FindElement("ResultLabel").GetText();

		Console.WriteLine($"Test result: {resultText}");

		// The test should show SUCCESS if the fix is working
		// It should show FAILED with ObjectDisposedException if the bug is present
		Assert.That(resultText, Does.Not.Contain("FAILED"), 
			$"TraitCollectionDidChange on disposed renderer should not throw. Result: {resultText}");
		
		// Also verify the status label doesn't show an error
		var statusText = App.FindElement("StatusLabel").GetText();
		Assert.That(statusText, Does.Not.Contain("ObjectDisposed"),
			$"Status should not show ObjectDisposedException. Status: {statusText}");
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellThemeChangeDoesNotCrash()
	{
		// Wait for the page to load
		App.WaitForElement("ChangeThemeButton");

		// Change theme - this triggers TraitCollectionDidChange
		App.Tap("ChangeThemeButton");

		// Wait a moment for theme change to propagate
		Task.Delay(500).Wait();

		// Verify the app didn't crash - check that status label is still accessible
		App.WaitForElement("StatusLabel");
		var statusText = App.FindElement("StatusLabel").GetText();

		// Ensure we don't have an ObjectDisposedException message
		Assert.That(statusText, Does.Not.Contain("ObjectDisposed"));
		Assert.That(statusText, Does.Not.Contain("FAILED"));

		// Change theme again to ensure it works both ways
		App.Tap("ChangeThemeButton");
		Task.Delay(500).Wait();

		statusText = App.FindElement("StatusLabel").GetText();
		Assert.That(statusText, Does.Not.Contain("ObjectDisposed"));
		Assert.That(statusText, Does.Not.Contain("FAILED"));

		// Verify the app is still responsive
		App.WaitForElement("SuccessLabel");
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void RapidThemeChangesDoNotCrashShell()
	{
		// Wait for the page to load
		App.WaitForElement("TriggerTraitChangeButton");

		// Trigger rapid theme changes
		App.Tap("TriggerTraitChangeButton");

		// Wait for the rapid changes to complete
		Task.Delay(1000).Wait();

		// Verify the app didn't crash
		App.WaitForElement("StatusLabel");
		var statusText = App.FindElement("StatusLabel").GetText();

		Assert.That(statusText, Does.Not.Contain("ObjectDisposed"));
		Assert.That(statusText, Does.Not.Contain("FAILED"));

		App.WaitForElement("SuccessLabel");
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void ThemeChangeDuringWindowCloseDoesNotCrash()
	{
		// Wait for the page to load
		App.WaitForElement("OpenCloseWindowButton");

		// This test opens a new window with Shell, changes theme, closes window
		// This should trigger ShellSectionRootRenderer disposal while TraitCollectionDidChange is being called
		
		// Run the test multiple times to increase chances of hitting the race condition
		for (int iteration = 0; iteration < 3; iteration++)
		{
			Console.WriteLine($"Test iteration {iteration + 1}");
			
			App.Tap("OpenCloseWindowButton");

			// Wait for the window open/close cycle to complete
			Task.Delay(2000).Wait();

			// Verify the app didn't crash - check status label
			App.WaitForElement("StatusLabel");
			var statusText = App.FindElement("StatusLabel").GetText();

			Console.WriteLine($"Status after iteration {iteration + 1}: {statusText}");

			// Check for failure indicators
			Assert.That(statusText, Does.Not.Contain("ObjectDisposed"), 
				$"ObjectDisposedException occurred on iteration {iteration + 1}");
			Assert.That(statusText, Does.Not.Contain("FAILED"),
				$"Test failed on iteration {iteration + 1}: {statusText}");

			// Verify success label is still visible (app didn't crash)
			var successText = App.FindElement("SuccessLabel").GetText();
			Assert.That(successText, Does.Not.Contain("FAILED"),
				$"Success label shows FAILED on iteration {iteration + 1}");

			// Small delay before next iteration
			Task.Delay(500).Wait();
		}

		// Final verification
		App.WaitForElement("WindowCloseCountLabel");
		var windowCloseText = App.FindElement("WindowCloseCountLabel").GetText();
		Console.WriteLine($"Final window close count: {windowCloseText}");
		
		// Verify at least some windows were closed
		Assert.That(windowCloseText, Does.Contain("Window closes:"));
	}
}