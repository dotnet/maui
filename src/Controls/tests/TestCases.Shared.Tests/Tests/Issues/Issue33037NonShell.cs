#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33037NonShell : _IssuesUITest
{
	const string Title = "Large Title Demo";

	public Issue33037NonShell(TestDevice device) : base(device) { }

	public override string Issue => "iOS Large Title display disappears when scrolling in non-Shell NavigationPage";

	[TestCase("Reporter")]
	[TestCase("Nested")]
	[TestCase("Table")]
	[Category(UITestCategories.Navigation)]
	public void LargeTitleCollapsesAndRestoresWithoutMovingTheList(string scenario)
	{
		Open(scenario);
		try
		{
			var expanded = GetTitle(expanded: true);
			var list = App.WaitForElement("Scroller").GetRect();
			Assert.That(App.WaitForElement("Item 0").GetRect().Y, Is.InRange(expanded.Bottom - 2, expanded.Bottom + 50));

			App.ScrollDown("Scroller", ScrollStrategy.Gesture, swipePercentage: 0.3, withInertia: false);
			Assert.That(GetTitle().Height, Is.LessThan(30));
			AssertStableList(list);

			for (var i = 0; i < 3; i++)
			{
				App.ScrollUp("Scroller", ScrollStrategy.Gesture, swipePercentage: 0.05, withInertia: false);
				AssertValidTitle(expanded);
				AssertStableList(list);
				App.ScrollDown("Scroller", ScrollStrategy.Gesture, swipePercentage: 0.05, withInertia: false);
				AssertValidTitle(expanded);
				AssertStableList(list);
			}

			ReturnToTop(list, expanded);
		}
		finally
		{
			App.WaitForElement("CloseScenario").Click();
		}
	}

	[TestCase(false)]
	[TestCase(true)]
	[Category(UITestCategories.Navigation)]
	public void LargeTitleSurvivesRotationUnderAFullScreenCover(bool collapse)
	{
		Open("Nested");
		try
		{
			var expanded = GetTitle(expanded: true);
			var list = App.WaitForElement("Scroller").GetRect();
			if (collapse)
				App.ScrollDown("Scroller", ScrollStrategy.Gesture, swipePercentage: 0.8, withInertia: false);
			var before = GetTitle();
			Assert.That(before.Height, collapse ? Is.LessThan(30) : Is.GreaterThan(30));

			App.SetOrientationLandscape();
			App.WaitForElement("Cover").Click();
			App.WaitForElement("DismissCover");
			App.SetOrientationPortrait();
			App.WaitForElement("DismissCover").Click();

			var after = GetTitle();
			Assert.That(after.Height, Is.EqualTo(before.Height).Within(2));
			Assert.That(after.Y, Is.EqualTo(before.Y).Within(2));
			ReturnToTop(list, expanded);
		}
		finally
		{
			App.SetOrientationPortrait();
			if (App.FindElements("DismissCover").Any())
				App.WaitForElement("DismissCover").Click();
			App.WaitForElement("CloseScenario").Click();
		}
	}

	[TestCase("Explicit")]
	[TestCase("NestedExplicit")]
	[TestCase("Header")]
	[TestCase("Multiple")]
	[TestCase("Opaque")]
	[TestCase("Margin")]
	[Category(UITestCategories.Navigation)]
	public void IneligibleLayoutsKeepTheirExistingSafeArea(string scenario)
	{
		Open(scenario);
		try
		{
			var expanded = GetTitle(expanded: true);
			var list = App.WaitForElement("Scroller").GetRect();
			var gap = list.Y - GetNavigationBar(expanded).Bottom;
			Assert.That(list.Y, Is.GreaterThanOrEqualTo(expanded.Bottom - 2));
			if (scenario == "Header")
				Assert.That(list.Y, Is.GreaterThanOrEqualTo(App.WaitForElement("Header").GetRect().Bottom - 2));
			App.ScrollDown("Scroller", ScrollStrategy.Gesture, swipePercentage: 0.3, withInertia: false);
			var after = GetTitle();
			Assert.That(after.Height, Is.EqualTo(expanded.Height).Within(2));
			Assert.That(App.WaitForElement("Scroller").GetRect().Y - GetNavigationBar(after).Bottom,
				Is.EqualTo(gap).Within(2));
		}
		finally
		{
			App.WaitForElement("CloseScenario").Click();
		}
	}

	[TestCase("Never")]
	[TestCase("Hidden")]
	[Category(UITestCategories.Navigation)]
	public void OptingOutOfTheLargeNavigationBarKeepsContentVisible(string scenario)
	{
		Open(scenario);
		try
		{
			var first = App.WaitForElement("Item 0").GetRect();
			if (scenario == "Never")
			{
				var title = GetTitle();
				Assert.That(title.Height, Is.LessThan(30));
				Assert.That(first.Y, Is.GreaterThanOrEqualTo(title.Bottom - 2));
			}
			else
			{
				Assert.That(App.FindElements(Title), Is.Empty);
				Assert.That(first.Y, Is.GreaterThan(0));
			}
		}
		finally
		{
			App.WaitForElement("CloseScenario").Click();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void ExplicitSafeAreaTakesOwnershipAfterDelegation()
	{
		Open("Dynamic");
		try
		{
			var expanded = GetTitle(expanded: true);
			Assert.That(App.WaitForElement("Scroller").GetRect().Y, Is.LessThanOrEqualTo(2));
			App.WaitForElement("ToggleSafeArea").Click();
			App.WaitForElement("Explicit safe area");
			Assert.That(App.WaitForElement("Scroller").GetRect().Y, Is.GreaterThanOrEqualTo(expanded.Bottom - 2));
			Assert.That(App.WaitForElement("Item 0").GetRect().Y, Is.GreaterThanOrEqualTo(expanded.Bottom - 2));
		}
		finally
		{
			App.WaitForElement("CloseScenario").Click();
		}
	}

	void Open(string scenario)
	{
		if (App is not AppiumIOSApp iosApp || !HelperExtensions.IsIOS26OrHigher(iosApp))
			Assert.Ignore("The fix is specific to iOS 26 and later.");
		App.WaitForElement(scenario).Click();
		App.WaitForElement("Scroller");
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void MovingAListToAnotherPageReleasesTheCachedPagesOwnership()
	{
		Open("Transfer");
		try
		{
			GetTitle(expanded: true);
			App.WaitForElement("TransferList").Click();
			var expanded = GetTitle(expanded: true, title: "Transferred list");
			Assert.That(App.WaitForElement("Scroller").GetRect().Y, Is.LessThanOrEqualTo(2));
			Assert.That(App.WaitForElement("Item 0").GetRect().Y, Is.GreaterThanOrEqualTo(expanded.Bottom - 2));
			App.ScrollDown("Scroller", ScrollStrategy.Gesture, swipePercentage: 0.3, withInertia: false);
			Assert.That(GetTitle(title: "Transferred list").Height, Is.LessThan(30));
			App.Back();
		}
		finally
		{
			if (App.FindElements("Transferred list").Any())
				App.Back();
			App.WaitForElement("CloseScenario").Click();
		}
	}

	void ReturnToTop(System.Drawing.Rectangle list, System.Drawing.Rectangle expanded)
	{
		var x = list.X + list.Width / 2;
		for (var i = 0; i < 3; i++)
			App.DragCoordinates(x, expanded.Bottom + 50, x, list.Bottom - 50);
		var restored = GetTitle(expanded: true);
		Assert.That(restored.Y, Is.EqualTo(expanded.Y).Within(2));
		Assert.That(restored.Height, Is.EqualTo(expanded.Height).Within(2));
		Assert.That(App.WaitForElement("Item 0").GetRect().Y, Is.InRange(restored.Bottom - 2, restored.Bottom + 50));
		AssertStableList(list);
	}

	void AssertStableList(System.Drawing.Rectangle expected)
	{
		var actual = App.WaitForElement("Scroller").GetRect();
		Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(2));
		Assert.That(actual.Height, Is.EqualTo(expected.Height).Within(2));
	}

	void AssertValidTitle(System.Drawing.Rectangle expanded)
	{
		var title = GetTitle();
		Assert.That(title.Height, Is.GreaterThan(0));
		Assert.That(title.Height < 30 && title.Y < expanded.Y - 5 ||
			title.Height > 30 && Math.Abs(title.Y - expanded.Y) <= 5, Is.True,
			$"Malformed title geometry near the collapse threshold: {title}");
	}

	System.Drawing.Rectangle GetTitle(bool expanded = false, string title = Title)
	{
		var element = App.WaitForElement(
			() => App.FindElements(title).OrderBy(e => e.GetRect().Height)
				.FirstOrDefault(e => expanded ? e.GetRect().Height is > 30 and < 50 : e.GetRect().Height < 50),
			"The navigation title must remain visible in the expected state.");
		return element.GetRect();
	}

	System.Drawing.Rectangle GetNavigationBar(System.Drawing.Rectangle title)
	{
		return App.Query.ByClass("XCUIElementTypeNavigationBar").Select(e => e.GetRect())
			.Where(rect => rect.Y <= title.Y && rect.Width > title.Width)
			.OrderBy(rect => rect.Height).First();
	}
}
#endif
