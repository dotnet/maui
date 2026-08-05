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

		void AssertEdgeLabelSettled(string automationId) =>
			App.RetryAssert(() =>
			{
				var rect = App.WaitForElement(automationId).GetRect();
				Assert.That(rect.X, Is.EqualTo(initialRect.X).Within(5));
				Assert.That(rect.Width, Is.EqualTo(initialRect.Width).Within(5));
			});

		App.TapTab("Second Tab");
		AssertEdgeLabelSettled(SecondEdgeLabel);
		App.TapTab("First Tab");
		AssertEdgeLabelSettled(FirstEdgeLabel);
	}
}
#endif
