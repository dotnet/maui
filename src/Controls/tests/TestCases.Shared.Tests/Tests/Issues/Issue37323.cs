using System.Threading;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37323 : _IssuesUITest
{
	public override string Issue => "Setting the padding value through binding or by using x:Name does not update the ScrollView padding";

	public Issue37323(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void UpdatingPaddingViaBindingUpdatesScrollViewLayout()
	{
		App.WaitForElement("TestScrollView");

		var contentBefore = App.WaitForElement("TopEdgeIndicator").GetRect();

		App.Tap("Padding");

		var contentAfter = App.WaitForElement("TopEdgeIndicator").GetRect();

		// Setting Padding to 20 on all sides via a binding should push the ScrollView's
		// content down and to the right by roughly 20 units. If the bug is present, the
		// content position does not change because the ScrollView doesn't re-layout.
		Assert.That(contentAfter.Y, Is.GreaterThan(contentBefore.Y + 10),
			$"ScrollView content did not move down after updating Padding via binding. " +
			$"Before: {contentBefore.Y}, After: {contentAfter.Y}.");
		Assert.That(contentAfter.X, Is.GreaterThan(contentBefore.X + 10),
			$"ScrollView content did not move right after updating Padding via binding. " +
			$"Before: {contentBefore.X}, After: {contentAfter.X}.");
	}
}
