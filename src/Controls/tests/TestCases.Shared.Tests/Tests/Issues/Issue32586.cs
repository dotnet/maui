#if IOS || ANDROID
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32586 : _IssuesUITest
{
	public override string Issue => "[iOS] Layout issue using TranslateToAsync causes infinite property changed cycle";

	public Issue32586(TestDevice device)
	: base(device)
	{ }

	void WaitForText(string elementId, string expectedText, int timeoutSec = 5)
	{
		var endTime = DateTime.Now.AddSeconds(timeoutSec);
		while (DateTime.Now < endTime)
		{
			var text = App.WaitForElement(elementId).GetText();
			if (text == expectedText) return;
			Thread.Sleep(100);
		}
		// Final check - will fail if text doesn't match
		var finalText = App.WaitForElement(elementId).GetText();
		Assert.That(finalText, Is.EqualTo(expectedText), $"Timed out waiting for {elementId} text to be '{expectedText}'");
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyLayoutWithTranslateToAsync()
	{
		var label = App.WaitForElement("TestLabel");
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now visible");
		
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now hidden");
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyRuntimeSafeAreaEdgesChange()
	{
		// Step 1: Default state - Parent Grid handles safe area (Container)
		// Content should be pushed below the safe area (TopMarker.Y > 0)
		var statusLabel = App.WaitForElement("SafeAreaStatusLabel");
		Assert.That(statusLabel.GetText(), Is.EqualTo("Parent: Container, Child: Container"));

		var topMarkerRect = App.WaitForElement("TopMarker").GetRect();
		var initialY = topMarkerRect.Y;
		Assert.That(initialY, Is.GreaterThan(0), "Content should be below safe area when parent handles it");

		// Step 2: Set parent Grid SafeAreaEdges to None
		// Child StackLayout should take over safe area responsibility
		// Content should still be pushed below the safe area
		App.Tap("ParentSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: None, Child: Container");

		topMarkerRect = App.WaitForElement("TopMarker").GetRect();
		var childHandlingY = topMarkerRect.Y;
		Assert.That(childHandlingY, Is.GreaterThan(0), "Child should handle safe area when parent doesn't");

		// Step 3: Set child StackLayout SafeAreaEdges to None too
		// No one handles safe area - content should go under the safe area (Y closer to 0)
		App.Tap("ChildSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: None, Child: None");

		topMarkerRect = App.WaitForElement("TopMarker").GetRect();
		var noSafeAreaY = topMarkerRect.Y;
		Assert.That(noSafeAreaY, Is.LessThan(childHandlingY), "Content should move up under safe area when no one handles it");

		// Step 4: Restore parent to Container - parent should take over again
		App.Tap("ParentSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: Container, Child: None");

		topMarkerRect = App.WaitForElement("TopMarker").GetRect();
		var restoredY = topMarkerRect.Y;
		Assert.That(restoredY, Is.GreaterThan(noSafeAreaY), "Parent should push content below safe area again");

		// Step 5: Verify UI is still responsive - no infinite cycle
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now visible");
	}
}
#endif