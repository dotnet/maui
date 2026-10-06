#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38361 : _IssuesUITest
{
	public Issue38361(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell singleton modal content remains visible after interactive dismissal";

	[Test]
	[Category(UITestCategories.Shell)]
	public void SingletonModalContentRemainsVisibleAfterNativeSwipeDismissalAndReopen()
	{
		var elapsed = Stopwatch.StartNew();
		RequireRendered("Issue38361OpenTransient", elapsed);
		App.Tap("Issue38361OpenTransient");
		RequireModalBar("Transient control", elapsed);
		RequireRendered("Issue38361TransientContent", elapsed);
		RequireRendered("Issue38361TransientSecond", elapsed);
		var firstControlInstance = RequireRendered("Issue38361Instance", elapsed).GetText();
		DismissByNativeSwipe("Transient control", "transient open 1", elapsed);

		App.Tap("Issue38361OpenTransient");
		RequireModalBar("Transient control", elapsed);
		RequireRendered("Issue38361TransientContent", elapsed);
		RequireRendered("Issue38361TransientSecond", elapsed);
		var secondControlInstance = RequireRendered("Issue38361Instance", elapsed).GetText();
		if (string.IsNullOrWhiteSpace(firstControlInstance) ||
			string.IsNullOrWhiteSpace(secondControlInstance) || firstControlInstance == secondControlInstance)
			throw new TimeoutException("The independently rendered transient control did not prove a new page instance.");
		DismissByNativeSwipe("Transient control", "transient open 2", elapsed);

		App.Tap("Issue38361OpenSingleton");
		RequireModalBar("SingletonPage", elapsed);
		RequireRendered("Issue38361SingletonContent", elapsed);
		RequireRendered("Issue38361SingletonSecond", elapsed);
		var singletonInstance = RequireRendered("Issue38361Instance", elapsed).GetText();
		if (string.IsNullOrWhiteSpace(singletonInstance))
			throw new TimeoutException("The first singleton presentation has no readable instance marker.");
		DismissByNativeSwipe("SingletonPage", "singleton open 1", elapsed);

		App.Tap("Issue38361OpenSingleton");
		RequireModalBar("SingletonPage", elapsed);

		// Missing content is an observation only after the native reopened sheet is confirmed.
		var observationTimeout = Remaining(elapsed, 3);
		if (observationTimeout < TimeSpan.FromSeconds(3))
			throw new TimeoutException("Insufficient canary budget remains for the full reopened-content observation.");
		var reopenedContentVisible = SpinWait.SpinUntil(() =>
		{
			if (FindModalBar("SingletonPage") is null)
				throw new TimeoutException("The reopened singleton sheet disappeared before content could be observed.");
			return FindRendered("Issue38361SingletonContent") is not null &&
				FindRendered("Issue38361SingletonSecond") is not null;
		}, observationTimeout);
		Remaining(elapsed, 3);

		Assert.That(reopenedContentVisible, Is.True,
			"The singleton sheet reopened after an actual swipe-down dismissal, but its label/button did not have displayed native elements with positive bounds. The fresh-instance control rendered twice.");
		if (RequireRendered("Issue38361Instance", elapsed).GetText() != singletonInstance)
			throw new TimeoutException("The reopened modal did not confirm the original singleton page instance.");
	}

	IUIElement RequireRendered(string id, Stopwatch elapsed)
	{
		return App.WaitForElement(() => FindRendered(id),
			$"Precondition incomplete: {id} has no displayed native element with positive bounds.",
			timeout: Remaining(elapsed, 3), retryFrequency: TimeSpan.FromMilliseconds(100));
	}

	IUIElement? FindRendered(string id)
	{
		var element = App.FindElements(id).FirstOrDefault();
		if (element is null || !element.IsDisplayed())
			return null;
		var bounds = element.GetRect();
		return bounds.Width > 0 && bounds.Height > 0 ? element : null;
	}

	static AppiumQuery ModalBarQuery(string title) =>
		AppiumQuery.ByXPath($"//XCUIElementTypeNavigationBar[@name='{title}']");

	IUIElement? FindModalBar(string title)
	{
		var element = App.FindElements(ModalBarQuery(title)).FirstOrDefault();
		if (element is null || !element.IsDisplayed())
			return null;
		var bounds = element.GetRect();
		return bounds.Width > 0 && bounds.Height > 0 ? element : null;
	}

	IUIElement RequireModalBar(string title, Stopwatch elapsed)
	{
		return App.WaitForElement(() => FindModalBar(title),
			$"Precondition incomplete: the native navigation bar for '{title}' is not presented.",
			timeout: Remaining(elapsed, 3), retryFrequency: TimeSpan.FromMilliseconds(100));
	}

	void DismissByNativeSwipe(string title, string stage, Stopwatch elapsed)
	{
		var bar = RequireModalBar(title, elapsed).GetRect();
		var window = App.WaitForElement(
			AppiumQuery.ByXPath($"//XCUIElementTypeWindow[.//XCUIElementTypeNavigationBar[@name='{title}']]"),
			"Precondition incomplete: the presented modal's native window is unavailable.",
			timeout: Remaining(elapsed, 3)).GetRect();
		var centerX = bar.X + bar.Width / 2f;
		var startY = bar.Y + bar.Height / 2f;
		var endY = window.Bottom - 24f;
		if (endY - startY < 180 || centerX < window.Left || centerX >= window.Right)
			throw new TimeoutException("The modal window has insufficient bounds for an interactive swipe-down dismissal.");

		App.DragCoordinates(centerX, startY, centerX, endY);
		App.WaitForNoElement(() => FindModalBar(title),
			$"Precondition incomplete: swiping the '{title}' navigation bar did not dismiss the sheet.",
			timeout: Remaining(elapsed, 3), retryFrequency: TimeSpan.FromMilliseconds(100));
		App.WaitForElement(() =>
		{
			var phase = FindRendered("Issue38361Phase");
			var open = FindRendered("Issue38361OpenSingleton");
			return phase is not null && phase.GetText() == $"Home after {stage}" &&
				open is not null && open.IsEnabled() ? phase : null;
		}, $"Precondition incomplete: Shell did not return home after swiping {stage}.",
			timeout: Remaining(elapsed, 3), retryFrequency: TimeSpan.FromMilliseconds(100));
		RequireRendered("Issue38361OpenTransient", elapsed);
	}

	static TimeSpan Remaining(Stopwatch elapsed, int maximumSeconds)
	{
		var remaining = TimeSpan.FromSeconds(26) - elapsed.Elapsed;
		if (remaining <= TimeSpan.Zero)
			throw new TimeoutException("The 26-second canary observation budget expired before the scenario completed.");
		var maximum = TimeSpan.FromSeconds(maximumSeconds);
		return remaining < maximum ? remaining : maximum;
	}
}
#endif
