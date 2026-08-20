#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST // This regression is specific to the iOS CollectionView handler.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37427 : _IssuesUITest
{
	public Issue37427(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "CollectionView item content renders with zero width on iOS";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void DynamicallyAddedContentRendersAfterCellRealization()
	{
		App.WaitForElement("37427CollectionView");
		App.ScrollTo("37427Card10");
		App.WaitForElement("37427Card10");

		Assert.Multiple(() =>
		{
			for (var colorIndex = 0; colorIndex < 5; colorIndex++)
			{
				var colorBounds = App.WaitForElement($"37427Card10Color{colorIndex}").GetRect();
				Assert.That(colorBounds.Width, Is.GreaterThan(0), $"Color {colorIndex} should have a non-zero width.");
				Assert.That(colorBounds.Height, Is.GreaterThan(0), $"Color {colorIndex} should have a non-zero height.");
			}
		});
	}
}
#endif
