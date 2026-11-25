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

		App.RetryAssert(() =>
		{
			var contentAfter = App.WaitForElement("TopEdgeIndicator").GetRect();

			Assert.That(contentAfter.Y, Is.GreaterThan(contentBefore.Y + 10),
				$"ScrollView content did not move down after updating Padding via binding. " +
				$"Before: {contentBefore.Y}, After: {contentAfter.Y}.");

			Assert.That(contentAfter.X, Is.GreaterThan(contentBefore.X + 10),
				$"ScrollView content did not move right after updating Padding via binding. " +
				$"Before: {contentBefore.X}, After: {contentAfter.X}.");
		});
	}
}
