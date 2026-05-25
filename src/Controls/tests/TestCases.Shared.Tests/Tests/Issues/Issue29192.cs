#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // CollectionView MeasureFirstItem sizing not applied on Windows, iOS and macOS) https://github.com/dotnet/maui/issues/29130
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29192 : _IssuesUITest
{
	public override string Issue => "[Android] CollectionView MeasureFirstItem ItemSizingStrategy Not Applied in Horizontal Layouts";

	public Issue29192(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ShouldMeasureFirstItemInHorizontalLayouts()
	{
		App.WaitForElement("CollectionView");
		VerifyScreenshot();
	}
}
#endif