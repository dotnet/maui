#if TEST_FAILS_ON_WINDOWS // ItemSpacing on CarouselView is not applied on Windows https://github.com/dotnet/maui/issues/29772
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29609 : _IssuesUITest
{
	public Issue29609(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ItemSpacing on CarouselView resizes items";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifySpacingAffectsItemSize()
	{
		App.WaitForElement("29609DescriptionLabel");
		VerifyScreenshot();
	}
}
#endif