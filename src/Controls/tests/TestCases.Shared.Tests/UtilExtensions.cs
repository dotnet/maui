using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Appium.NUnit;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public static class UtilExtensions
	{
		const string goToTestButtonId = "GoToTestButton";

		public static void Back(this UITestContextBase testBase)
		{
			if (testBase.Device == TestDevice.Android)
			{
				var query = testBase.App.Query.ByAccessibilityId("Navigate up").First();
				query.Click();
			}
			else if (testBase.Device == TestDevice.iOS || testBase.Device == TestDevice.Mac)
			{
				// Get the first NavigationBar we can find and the first button in it (the back button), index starts at 1
				var queryBy = testBase.App.Query.ByClass("XCUIElementTypeNavigationBar").First().ByClass("XCUIElementTypeButton").First();
				queryBy.Click();
			}
			else
			{
				testBase.App.FindElement("NavigationViewBackButton").Click();
			}
		}

		public static void NavigateToGallery(this IApp app, string page)
		{
			// For Catalyst app directly go the test page while opening.
			if (app is not AppiumCatalystApp)
			{
				app.WaitForGoToTestButtonWithRecovery();
				NavigateTo(app, page);
			}
		}

		// Waits for the gallery's "Go To Test button" and, if it never appears (the usual
		// symptom of a HostApp crash — on Android a "keeps stopping"/ANR dialog blocks the
		// relaunched app), tries to break the crash loop and retry instead of letting a
		// single crash cascade into every following fixture failing OneTimeSetUp.
		public static void WaitForGoToTestButtonWithRecovery(this IApp app, string? timeoutMessage = null)
		{
			var message = timeoutMessage ?? "Timed out waiting for Go To Test button to appear";
			try
			{
				app.WaitForElement(goToTestButtonId, message, TimeSpan.FromMinutes(2));
				return;
			}
			catch (TimeoutException)
			{
				// Recovery is only meaningful on Android, where the system crash dialog
				// blocks relaunch. Elsewhere, surface the original failure unchanged.
				if (app.GetTestDevice() != TestDevice.Android)
					throw;
			}

			// Tiered recovery: first a soft reset (dismiss dialog + force-stop + relaunch),
			// then a hard reset (also clears app data) if the soft reset didn't bring the
			// gallery back. Use a short retry timeout — a recovered app shows the gallery
			// within seconds, so we don't want to pay another full 2-minute wait per tier.
			var retryTimeout = TimeSpan.FromSeconds(45);
			foreach (var hard in new[] { false, true })
			{
				try
				{
					app.CommandExecutor.Execute("recoverFromCrash", new Dictionary<string, object> { ["hard"] = hard });
				}
				catch
				{
					// best effort — fall through to the retry wait
				}

				try
				{
					app.WaitForElement(goToTestButtonId, message, retryTimeout);
					return;
				}
				catch (TimeoutException)
				{
					// escalate to the next (harder) tier
				}
			}

			// Could not recover (e.g. a deterministic startup crash or a broken emulator) —
			// surface the failure so it isn't silently swallowed.
			throw new TimeoutException($"{message} (the app did not recover after crash-recovery attempts)");
		}

		public static void NavigateTo(this IApp app, string text)
		{
			app.WaitForElement("SearchBar");
			app.ClearText("SearchBar");
			if (!string.IsNullOrWhiteSpace(text))
			{
				app.EnterText("SearchBar", text);
			}
			app.Tap(goToTestButtonId);

			app.WaitForNoElement(goToTestButtonId, "Timed out waiting for Go To Test button to disappear", TimeSpan.FromMinutes(1));
		}

		public static int CenterX(this Rectangle rect)
		{
			return rect.X + rect.Width / 2;
		}

		public static int CenterY(this Rectangle rect)
		{
			return rect.Y + rect.Height / 2;
		}

		public static void AssertMemoryTest(this IApp app)
		{
			try
			{
				app.WaitForElement("Success", timeout: TimeSpan.FromSeconds(10));
			}
			catch
			{
				var failure = app.FindElement("Failed")?.GetText();
				if (failure is not null)
				{
					Assert.Fail(failure);
				}

				throw;
			}
		}

		public static void TapFirstSearchResult(this IApp app, UITestContextBase context, IUIElement searchHandler, string searchResultIdentifier = "SearchResultName")
		{
			if (context.Device == TestDevice.Android)
			{
				// Android does not support selecting elements in SearchHandler results
				var y = searchHandler.GetRect().Y + searchHandler.GetRect().Height;
				app.TapCoordinates(searchHandler.GetRect().X + 10, y + 10);
			}
			else
			{
				var searchResults = app.FindElements(searchResultIdentifier);
				searchResults.First().Tap();
			}
		}
	}
}