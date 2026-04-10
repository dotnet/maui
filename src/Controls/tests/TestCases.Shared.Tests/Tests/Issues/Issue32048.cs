#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/31670
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32048 : _IssuesUITest
{
	public Issue32048(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CurrentItem does not update when ItemSpacing is set";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCurrentItemUpdatesWithItemSpacing()
	{
		App.WaitForElement("CarouselViewWithItemSpacing");
		App.ScrollRight("CarouselViewWithItemSpacing");

#if MACCATALYST
		Thread.Sleep(1000);
#endif
		var resultLabel = App.WaitForElement("Issue32048StatusLabel").GetText();
		Assert.That(resultLabel, Is.EqualTo("Success"));
	}
}
#endif