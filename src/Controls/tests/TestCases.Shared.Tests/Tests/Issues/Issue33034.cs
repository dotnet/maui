#if ANDROID || IOS  // SafeAreaEdges not supported on Catalyst and Windows

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33034 : _IssuesUITest
{
	const string FirstEdgeLabel = "FirstEdgeLabel";
	const string SecondEdgeLabel = "SecondEdgeLabel";

	public override string Issue => "SafeAreaEdges works correctly only on the first tab in Shell. Other tabs have content colliding with the display cutout in the landscape mode.";

	public Issue33034(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SafeAreaShouldWorkOnAllShellTabs()
	{
		App.WaitForElement(FirstEdgeLabel);
		App.SetOrientationLandscape();
		//Adding delay to allow for orientation change to complete
		Thread.Sleep(2000);
		var initialRect = App.WaitForElement(FirstEdgeLabel).GetRect();

		IUIElement WaitForSettledEdgeLabel(string automationId) =>
			App.WaitForElement(() =>
			{
				try
				{
					var element = App.FindElement(automationId);
					if (element is null)
						return null;

					// Wait for BOTH X and Width to settle: capturing the rect while width is still
					// mid-layout would let the later Within(5) assertions flake.
					var rect = element.GetRect();
					return Math.Abs(rect.X - initialRect.X) <= 5 && Math.Abs(rect.Width - initialRect.Width) <= 5
						? element
						: null;
				}
				catch
				{
					// WaitForElement(Func<>) rethrows query exceptions, so swallow transient/stale-element
					// failures from FindElement/GetRect here to keep polling instead of failing immediately.
					return null;
				}
			}, $"Timed out waiting for {automationId} to settle");

		App.TapTab("Second Tab");
		var secondTabRect = WaitForSettledEdgeLabel(SecondEdgeLabel).GetRect();
		App.TapTab("First Tab");
		var afterSwitchRect = WaitForSettledEdgeLabel(FirstEdgeLabel).GetRect();

		Assert.That(secondTabRect.X, Is.EqualTo(initialRect.X).Within(5));
		Assert.That(secondTabRect.Width, Is.EqualTo(initialRect.Width).Within(5));
		Assert.That(afterSwitchRect.X, Is.EqualTo(initialRect.X).Within(5));
		Assert.That(afterSwitchRect.Width, Is.EqualTo(initialRect.Width).Within(5));
	}
}
#endif
