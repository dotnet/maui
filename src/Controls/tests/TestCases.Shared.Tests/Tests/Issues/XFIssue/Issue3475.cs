using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3475 : _IssuesUITest
{
	public Issue3475(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] LayoutCompression Performance Issues";

	// [Test]
	// [Category(UITestCategories.Layout)]
	// [FailsOnAndroid]
	// [FailsOnIOS]
	// public void Issue3475TestsLayoutCompressionPerformance()
	// {
	// 	App.WaitForElement(_titleLabelId);
	// 	App.WaitForElement(_withoutCompressionBtnId);
	// 	App.WaitForElement(_withCompressionBtnId);

	// 	App.Tap(_withoutCompressionBtnId);
	// 	App.WaitForElement(DoneLabelId);
	// 	App.Screenshot("Without Layout Compression");

	// 	int elapsedWithoutCompression = GetMs(App.Query(ElapsedLabelId).First().Text);

	// 	App.Tap(BackButtonId);
	// 	App.WaitForElement(_withCompressionBtnId);

	// 	App.Tap(_withCompressionBtnId);
	// 	App.WaitForElement(DoneLabelId);
	// 	App.Screenshot("With Layout Compression");

	// 	int elapsedWithCompression = GetMs(App.Query(ElapsedLabelId).First().Text);
	// 	var delta = elapsedWithCompression - elapsedWithoutCompression;

	// 	//if layoutcompressions is slower than 100 then there is a problem.
	// 	//it should be at least very similar and no more than 100ms slower i guess...
	// 	Assert.LessOrEqual(delta, 100);
	// }
}