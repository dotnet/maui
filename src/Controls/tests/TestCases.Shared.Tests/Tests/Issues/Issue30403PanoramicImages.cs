#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30403PanoramicImages : _IssuesUITest
{
	public Issue30403PanoramicImages(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue =>
		"Image under WinUI does not respect VerticalOptions and HorizontalOptions with AspectFit (Panoramic)";

	[Test, Order(1)]
	[Category(UITestCategories.Image)]
	public void PanoramicImageCenterAlignment_ShouldRespectLayoutOptions()
	{
		AssertScenarioPassed("PanoramicCenterResult");
	}

	[Test, Order(2)]
	[Category(UITestCategories.Image)]
	public void PanoramicImageStartAlignment_ShouldAlignToTopLeft()
	{
		AssertScenarioPassed("PanoramicStartResult");
	}

	[Test, Order(3)]
	[Category(UITestCategories.Image)]
	public void PanoramicImageEndAlignment_ShouldAlignToBottomRight()
	{
		AssertScenarioPassed("PanoramicEndResult");
	}

	[Test, Order(4)]
	[Category(UITestCategories.Image)]
	public void AllPanoramicImages_ShouldLoadAndDisplay()
	{
		foreach (var resultId in new[] { "PanoramicCenterResult", "PanoramicStartResult", "PanoramicEndResult" })
			AssertScenarioPassed(resultId);
	}

	[Test, Order(5)]
	[Category(UITestCategories.Image)]
	public void ConstrainedWidthContainer_ShouldHandleNarrowSpace()
	{
		AssertScenarioPassed("PanoramicNarrowResult");
	}

	[Test, Order(6)]
	[Category(UITestCategories.Image)]
	public void ConstrainedHeightContainer_ShouldHandleShortSpace()
	{
		AssertScenarioPassed("PanoramicShortResult");
	}

	[Test, Order(7)]
	[Category(UITestCategories.Image)]
	public void MultipleImagesInGrid_ShouldRespectIndividualAlignments()
	{
		foreach (var resultId in new[]
		{
			"MultiPanoramicTopLeftResult",
			"MultiPanoramicTopRightResult",
			"MultiPanoramicBottomLeftResult",
			"MultiPanoramicBottomRightResult"
		})
		{
			AssertScenarioPassed(resultId);
		}
	}

	void AssertScenarioPassed(string automationId)
	{
		App.ScrollTo(automationId);

		var passed = App.WaitForTextToBePresentInElement(automationId, "PASS", TimeSpan.FromSeconds(10));
		var result = App.WaitForElement(automationId).GetText();

		Assert.That(passed, Is.True, result);
		Assert.That(result, Does.StartWith("PASS"), result);
	}
}
#endif
