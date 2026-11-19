#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
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
		var home = RequireHome(null, elapsed);
		TapRendered(home, "Issue38361OpenTransient", elapsed);
		var modal = RequireModalContent("Transient control",
			"Issue38361TransientContent", "Issue38361TransientSecond", elapsed);
		var firstControlInstance = ReadInstance(modal);
		home = DismissByNativeSwipe(modal, "Transient control", "transient open 1", elapsed);

		TapRendered(home, "Issue38361OpenTransient", elapsed);
		modal = RequireModalContent("Transient control",
			"Issue38361TransientContent", "Issue38361TransientSecond", elapsed);
		var secondControlInstance = ReadInstance(modal);
		if (string.IsNullOrWhiteSpace(firstControlInstance) ||
			string.IsNullOrWhiteSpace(secondControlInstance) || firstControlInstance == secondControlInstance)
			throw new TimeoutException("The independently rendered transient control did not prove a new page instance.");
		home = DismissByNativeSwipe(modal, "Transient control", "transient open 2", elapsed);

		TapRendered(home, "Issue38361OpenSingleton", elapsed);
		modal = RequireModalContent("SingletonPage",
			"Issue38361SingletonContent", "Issue38361SingletonSecond", elapsed);
		var singletonInstance = ReadInstance(modal);
		if (string.IsNullOrWhiteSpace(singletonInstance))
			throw new TimeoutException("The first singleton presentation has no readable instance marker.");
		home = DismissByNativeSwipe(modal, "SingletonPage", "singleton open 1", elapsed);

		TapRendered(home, "Issue38361OpenSingleton", elapsed);
		RequireSnapshot(tree => FindModalBar(tree, "SingletonPage") is not null,
			"The native reopened singleton sheet is not presented.", elapsed);

		// Missing content is an observation only after the native reopened sheet is confirmed.
		var observationTimeout = Remaining(elapsed, 3);
		if (observationTimeout < TimeSpan.FromSeconds(3))
			throw new TimeoutException("Insufficient canary budget remains for the full reopened-content observation.");
		XDocument? reopened = null;
		var reopenedContentVisible = SpinWait.SpinUntil(() =>
		{
			reopened = ObserveSnapshot(elapsed);
			if (FindModalBar(reopened, "SingletonPage") is null)
				throw new TimeoutException("The reopened singleton sheet disappeared before content could be observed.");
			return FindRendered(reopened, "Issue38361SingletonContent") is not null &&
				FindRendered(reopened, "Issue38361SingletonSecond") is not null;
		}, observationTimeout);
		Remaining(elapsed, 3);

		Assert.That(reopenedContentVisible, Is.True,
			"The singleton sheet reopened after an actual swipe-down dismissal, but its label/button did not have displayed native elements with positive bounds. The fresh-instance control rendered twice.");
		if (reopened is null || ReadInstance(reopened) != singletonInstance)
			throw new TimeoutException("The reopened modal did not confirm the original singleton page instance.");
	}

	XDocument ObserveSnapshot(Stopwatch elapsed)
	{
		return WithinBudget(elapsed, () =>
		{
			var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
			using var reader = XmlReader.Create(new StringReader(App.ElementTree), settings);
			return XDocument.Load(reader);
		});
	}

	XDocument RequireSnapshot(Func<XDocument, bool> ready, string description, Stopwatch elapsed)
	{
		var deadline = elapsed.Elapsed + Remaining(elapsed, 3);
		while (elapsed.Elapsed < deadline)
		{
			var tree = ObserveSnapshot(elapsed);
			var isReady = ready(tree);
			Remaining(elapsed, 26);
			if (elapsed.Elapsed >= deadline)
				Console.WriteLine($"Native modal readiness read crossed its retry window: ready={isReady}; body elapsed={elapsed.Elapsed.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture)}s; {description}");
			if (isReady)
				return tree;
			Thread.Sleep(100);
		}
		throw new TimeoutException($"Precondition incomplete: {description}");
	}

	static XElement? FindRendered(XDocument tree, string id) =>
		tree.Descendants().SingleOrDefault(element =>
			(string?)element.Attribute("name") == id && IsRendered(element));

	static bool IsRendered(XElement element)
	{
		if ((string?)element.Attribute("visible") != "true")
			return false;
		var bounds = ReadBounds(element);
		return bounds.Width > 0 && bounds.Height > 0;
	}

	static RectangleF ReadBounds(XElement element)
	{
		float Coordinate(string attribute)
		{
			if (!float.TryParse((string?)element.Attribute(attribute), NumberStyles.Float,
				CultureInfo.InvariantCulture, out var value) || !float.IsFinite(value))
				throw new TimeoutException($"The native modal exposed an invalid {attribute} coordinate.");
			return value;
		}
		return new RectangleF(Coordinate("x"), Coordinate("y"), Coordinate("width"), Coordinate("height"));
	}

	static XElement? FindModalBar(XDocument tree, string title) =>
		tree.Descendants("XCUIElementTypeNavigationBar").SingleOrDefault(element =>
			(string?)element.Attribute("name") == title && IsRendered(element));

	static string? ReadInstance(XDocument tree) =>
		(string?)FindRendered(tree, "Issue38361Instance")?.Attribute("value");

	XDocument RequireModalContent(string title, string contentId, string buttonId, Stopwatch elapsed) =>
		RequireSnapshot(tree => FindModalBar(tree, title) is not null
			&& FindRendered(tree, contentId) is not null
			&& FindRendered(tree, buttonId) is not null
			&& !string.IsNullOrWhiteSpace(ReadInstance(tree)),
			$"The '{title}' sheet has no rendered content and instance marker.", elapsed);

	XDocument RequireHome(string? stage, Stopwatch elapsed) =>
		RequireSnapshot(tree =>
		{
			var transient = FindRendered(tree, "Issue38361OpenTransient");
			var singleton = FindRendered(tree, "Issue38361OpenSingleton");
			var phase = FindRendered(tree, "Issue38361Phase");
			return transient is not null && singleton is not null
				&& (string?)transient.Attribute("enabled") == "true"
				&& (string?)singleton.Attribute("enabled") == "true"
				&& (stage is null || (string?)phase?.Attribute("value") == $"Home after {stage}")
				&& FindModalBar(tree, "Transient control") is null
				&& FindModalBar(tree, "SingletonPage") is null;
		}, $"Shell did not return to the rendered home controls after '{stage}'.", elapsed);

	void TapRendered(XDocument tree, string id, Stopwatch elapsed)
	{
		var element = FindRendered(tree, id);
		if (element is null || (string?)element.Attribute("enabled") != "true")
			throw new TimeoutException($"Precondition incomplete: '{id}' is not rendered and enabled.");
		var bounds = ReadBounds(element);
		WithinBudget(elapsed, () =>
		{
			App.TapCoordinates(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
			return true;
		});
	}

	XDocument DismissByNativeSwipe(XDocument tree, string title, string stage, Stopwatch elapsed)
	{
		var barElement = FindModalBar(tree, title);
		var windowElement = barElement?.Ancestors("XCUIElementTypeWindow").FirstOrDefault();
		if (barElement is null || windowElement is null || !IsRendered(windowElement))
			throw new TimeoutException("Precondition incomplete: the presented modal's native window is unavailable.");
		var bar = ReadBounds(barElement);
		var window = ReadBounds(windowElement);
		var centerX = bar.X + bar.Width / 2f;
		var startY = bar.Y + bar.Height / 2f;
		var endY = window.Bottom - 24f;
		if (endY - startY < 180 || centerX < window.Left || centerX >= window.Right)
			throw new TimeoutException("The modal window has insufficient bounds for an interactive swipe-down dismissal.");

		WithinBudget(elapsed, () =>
		{
			App.DragCoordinates(centerX, startY, centerX, endY);
			return true;
		});
		return RequireHome(stage, elapsed);
	}

	static T WithinBudget<T>(Stopwatch elapsed, Func<T> action)
	{
		var remaining = Remaining(elapsed, 26);
		return Task.Run(action).WaitAsync(remaining).GetAwaiter().GetResult();
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
