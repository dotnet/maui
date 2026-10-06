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

public class Issue38276 : _IssuesUITest
{
	public Issue38276(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CollectionView height does not shrink after replacing ItemsSource on iOS";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView)]
	public void ReplacingTenItemsWithOneItemShrinksRenderedCollectionView()
	{
		var clock = Stopwatch.StartNew();

		TapWithinBudget(clock, "Issue38276OneItem");
		var control = WaitForStableCollection(clock, "one-item control", 1, 1,
			rect => rect.Height <= 80);
		TestContext.Progress.WriteLine($"One-item control native height: {control.Height}");

		TapWithinBudget(clock, "Issue38276TenItems");
		var large = WaitForStableCollection(clock, "ten-item state", 10, 2,
			rect => rect.Height >= 140 && rect.Height <= 182
				&& rect.Height - control.Height >= 80
				&& SameViewport(rect, control));
		TestContext.Progress.WriteLine($"Ten-item native height: {large.Height}");

		TapWithinBudget(clock, "Issue38276OneItem");
		var small = WaitForStableCollection(clock, "one-item replacement", 1, 3,
			rect => SameViewport(rect, control));
		TestContext.Progress.WriteLine($"Replacement one-item native height: {small.Height}");

		Assert.That(small.Height,
			Is.LessThanOrEqualTo(control.Height + 8).And.LessThanOrEqualTo(large.Height - 60),
			$"Replacing ten items with one must restore the measured one-item height within 8 points "
			+ $"and shrink by at least 60 points. Native heights: control={control.Height}, "
			+ $"ten items={large.Height}, replacement={small.Height}.");
	}

	void TapWithinBudget(Stopwatch clock, string id)
	{
		var element = WithinBudget(clock, () => App.WaitForElement(id,
			timeout: TimeSpan.FromSeconds(2), retryFrequency: TimeSpan.FromMilliseconds(150)));
		WithinBudget(clock, () =>
		{
			element.Tap();
			return true;
		});
	}

	RectangleF WaitForStableCollection(Stopwatch clock, string phase, int count, int replacement,
		Func<RectangleF, bool> usable)
	{
		var started = clock.Elapsed;
		var deadline = started + TimeSpan.FromSeconds(6);
		var expectedStatus = $"Items: {count}; replacement: {replacement}";
		RectangleF? previous = null;
		RectangleF? lastRect = null;
		string? lastStatus = null;
		var stableSince = started;
		var samples = 0;

		try
		{
			while (clock.Elapsed < deadline && clock.Elapsed < TimeSpan.FromSeconds(24))
			{
				var snapshot = WithinBudget(clock, ObserveCollection, deadline);
				lastStatus = snapshot.Status;
				lastRect = snapshot.Rect;
				var ready = false;

				if (lastStatus == expectedStatus && snapshot.Rect.HasValue)
				{
					var rect = snapshot.Rect.Value;
					ready = rect.Width > 0 && rect.Height > 0 && usable(rect)
						&& snapshot.FirstItem && snapshot.SecondItem == (count == 10);

					if (ready)
					{
						if (previous.HasValue && SameRect(rect, previous.Value))
							samples++;
						else
						{
							stableSince = clock.Elapsed;
							samples = 1;
							previous = rect;
						}

						if (samples >= 3
							&& clock.Elapsed - started >= TimeSpan.FromSeconds(2)
							&& clock.Elapsed - stableSince >= TimeSpan.FromSeconds(1)
							&& clock.Elapsed < deadline
							&& clock.Elapsed < TimeSpan.FromSeconds(24))
							return rect;
					}
				}

				if (!ready)
				{
					previous = null;
					samples = 0;
				}

				Thread.Sleep(150);
			}
		}
		catch (TimeoutException exception)
		{
			throw new TimeoutException(
				$"Could not establish {phase} within the bounded observation window. "
				+ $"Expected source status: {expectedStatus}; last status: {lastStatus}; "
				+ $"last native rectangle: {lastRect}.", exception);
		}

		throw new TimeoutException(
			$"No usable, stable rendered CollectionView for {phase}. "
			+ $"Expected source status: {expectedStatus}; last status: {lastStatus}; "
			+ $"last native rectangle: {lastRect}. This is not a shrink assertion.");
	}

	(string? Status, RectangleF? Rect, bool FirstItem, bool SecondItem) ObserveCollection()
	{
		var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
		using var reader = XmlReader.Create(new StringReader(App.ElementTree), settings);
		var tree = XDocument.Load(reader);
		var status = tree.Descendants().SingleOrDefault(element =>
			(string?)element.Attribute("name") == "Issue38276SourceStatus"
			&& (string?)element.Attribute("visible") == "true");
		var collection = tree.Descendants().SingleOrDefault(element =>
			(string?)element.Attribute("name") == "Issue38276Collection"
			&& (string?)element.Attribute("visible") == "true");
		RectangleF? rect = collection is null ? null : new RectangleF(
			ReadNativeCoordinate(collection, "x"), ReadNativeCoordinate(collection, "y"),
			ReadNativeCoordinate(collection, "width"), ReadNativeCoordinate(collection, "height"));
		bool HasItem(string label) => collection?.Descendants().Any(element =>
			element.Name.LocalName == "XCUIElementTypeStaticText"
			&& (string?)element.Attribute("label") == label
			&& (string?)element.Attribute("visible") == "true") == true;

		return ((string?)status?.Attribute("value"), rect, HasItem("Appointment 1"), HasItem("Appointment 2"));
	}

	static float ReadNativeCoordinate(XElement element, string attribute)
	{
		if (!float.TryParse((string?)element.Attribute(attribute), NumberStyles.Float,
			CultureInfo.InvariantCulture, out var value) || !float.IsFinite(value))
			throw new TimeoutException($"The native CollectionView exposed an invalid {attribute} coordinate.");
		return value;
	}

	static bool SameViewport(RectangleF current, RectangleF control) =>
		Math.Abs(current.X - control.X) <= 2
		&& Math.Abs(current.Y - control.Y) <= 2
		&& Math.Abs(current.Width - control.Width) <= 2;

	static bool SameRect(RectangleF current, RectangleF previous) =>
		Math.Abs(current.X - previous.X) <= 1
		&& Math.Abs(current.Y - previous.Y) <= 1
		&& Math.Abs(current.Width - previous.Width) <= 1
		&& Math.Abs(current.Height - previous.Height) <= 1;

	static T WithinBudget<T>(Stopwatch clock, Func<T> action, TimeSpan? deadline = null)
	{
		var remaining = TimeSpan.FromSeconds(24) - clock.Elapsed;
		if (deadline.HasValue && deadline.Value - clock.Elapsed < remaining)
			remaining = deadline.Value - clock.Elapsed;
		if (remaining <= TimeSpan.Zero)
			throw new TimeoutException("The iOS canary observation budget has expired.");

		return Task.Run(action).WaitAsync(remaining).GetAwaiter().GetResult();
	}
}
#endif
