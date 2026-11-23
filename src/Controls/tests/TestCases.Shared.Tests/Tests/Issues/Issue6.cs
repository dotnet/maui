using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6 : _IssuesUITest
{
	public Issue6(TestDevice device) : base(device) { }

	public override string Issue => "Page transitions: a shared element lands where it belongs and pages return to rest";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void SharedElementLandsAndSourceIsRestoredAfterPop()
	{
		var before = App.WaitForElement("Tile2").GetRect();

		App.Tap("Tile2");

		// NavigatedTo is raised once the transition has finished, so this text means the flight is over.
		Assert.That(App.WaitForTextToBePresentInElement("DetailStatus", "Arrived at 2"), Is.True);
		// GetRect is in physical pixels on Android, so the landed size is compared as a ratio to the source tile
		// (HeightRequest 240 over 80), which is the same in any unit.
		var hero = App.WaitForElement("DetailTile").GetRect();
		Assert.That(hero.Height / (double)before.Height, Is.EqualTo(3.0).Within(0.1), "The shared element should be laid out at its own size once it lands, not at the source's.");

		App.WaitForElement("BackButton");
		App.Tap("BackButton");

		Assert.That(App.WaitForTextToBePresentInElement("Status", "Returned from 2"), Is.True);
		var after = App.WaitForElement("Tile2").GetRect();
		Assert.That(after.X, Is.EqualTo(before.X).Within(1), "The source element should be back where it was.");
		Assert.That(after.Y, Is.EqualTo(before.Y).Within(1), "The source element should be back where it was.");
		Assert.That(after.Width, Is.EqualTo(before.Width).Within(1));
		Assert.That(after.Height, Is.EqualTo(before.Height).Within(1));

		// The list still navigates after a transition has run on it.
		App.WaitForElement("Tile3");
		App.Tap("Tile3");
		Assert.That(App.WaitForTextToBePresentInElement("DetailStatus", "Arrived at 3"), Is.True);
	}
}
