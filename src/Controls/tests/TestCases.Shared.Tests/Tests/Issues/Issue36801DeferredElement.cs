#if IOS || MACCATALYST // Validates UIScrollView AdjustedContentInset math with platform instrumentation in the HostApp page
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36801DeferredElement : _IssuesUITest
{
	public Issue36801DeferredElement(TestDevice device) : base(device) { }

	public override string Issue => "Deferred element-mode ScrollToAsync";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void DeferredElementScrollLandsInsideVisibleViewport()
	{
		App.WaitForElement("ResultLabel");

		// The page issues ScrollToAsync(probe, End) before the handler exists; the target can
		// only be resolved once layout has given the ScrollView its geometry.
		var success = App.WaitForTextToBePresentInElement("ResultLabel", "Success", timeout: TimeSpan.FromSeconds(15));
		var resultText = App.FindElement("ResultLabel").GetText();
		Assert.That(success, Is.True, $"Deferred element scroll did not align with the viewport bottom: {resultText}");
	}
}
#endif
