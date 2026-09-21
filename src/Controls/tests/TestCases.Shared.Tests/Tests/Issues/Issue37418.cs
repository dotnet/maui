#if ANDROID || IOS          // Issue related to SafeAreaEdges, which is only applicable to mobile platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37418 : _IssuesUITest
{
	public override string Issue => "TranslationY outside the screen adds incorrect top padding";

	public Issue37418(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void TranslatedBottomSheetDoesNotAddTopPadding()
	{
		App.WaitForElement("OpenBottomSheetButton");
		App.Tap("OpenBottomSheetButton");

		var borderRect = App.WaitForElement("BottomSheetBorder").GetRect();
		var closeButtonRect = App.WaitForElement("CloseBottomSheetButton").GetRect();

		Assert.That(closeButtonRect.Y - borderRect.Y, Is.LessThanOrEqualTo(5),
			"The translated bottom sheet should not receive top safe-area padding.");
	}
}
#endif
