#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30403SmallImages : _IssuesUITest
{
	public Issue30403SmallImages(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue =>
		"Image under WinUI does not respect VerticalOptions and HorizontalOptions with AspectFit (Small Images)";

	[Test, Order(1)]
	[Category(UITestCategories.Image)]
	public void SmallImageInLargeContainer_ShouldNotExceedIntrinsicSize()
	{
		AssertScenarioPassed("SmallImageCenterResult");
	}

	[Test, Order(2)]
	[Category(UITestCategories.Image)]
	public void SmallImageStartAlignment_ShouldRespectAlignment()
	{
		AssertScenarioPassed("SmallImageStartResult");
	}

	[Test, Order(3)]
	[Category(UITestCategories.Image)]
	public void SmallImageEndAlignment_ShouldRespectAlignment()
	{
		AssertScenarioPassed("SmallImageEndResult");
	}

	[Test, Order(4)]
	[Category(UITestCategories.Image)]
	public void ConstrainedContainers_ShouldHandleSmallImages()
	{
		foreach (var resultId in new[]
		{
			"SmallImageConstrained30Result",
			"SmallImageConstrained40Result",
			"SmallImageConstrained60Result"
		})
		{
			AssertScenarioPassed(resultId);
		}
	}

	[Test, Order(5)]
	[Category(UITestCategories.Image)]
	public void TinyImage_ShouldRemainTiny()
	{
		AssertScenarioPassed("TinyImageResult");
	}

	[Test, Order(6)]
	[Category(UITestCategories.Image)]
	public void SizeComparison_ShouldShowDifferentSizes()
	{
		foreach (var resultId in new[]
		{
			"ComparisonTinyResult",
			"ComparisonSmallResult",
			"ComparisonMediumResult",
			"ComparisonLargeResult"
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
