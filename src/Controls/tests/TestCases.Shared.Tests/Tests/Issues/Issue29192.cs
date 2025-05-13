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