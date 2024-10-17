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
	// 	RunningApp.WaitForElement(_titleLabelId);
	// 	RunningApp.WaitForElement(_withoutCompressionBtnId);
	// 	RunningApp.WaitForElement(_withCompressionBtnId);

	// 	RunningApp.Tap(_withoutCompressionBtnId);
	// 	RunningApp.WaitForElement(DoneLabelId);
	// 	RunningApp.Screenshot("Without Layout Compression");

	// 	int elapsedWithoutCompression = GetMs(RunningApp.Query(ElapsedLabelId).First().Text);

	// 	RunningApp.Tap(BackButtonId);
	// 	RunningApp.WaitForElement(_withCompressionBtnId);

	// 	RunningApp.Tap(_withCompressionBtnId);
	// 	RunningApp.WaitForElement(DoneLabelId);
	// 	RunningApp.Screenshot("With Layout Compression");

	// 	int elapsedWithCompression = GetMs(RunningApp.Query(ElapsedLabelId).First().Text);
	// 	var delta = elapsedWithCompression - elapsedWithoutCompression;

	// 	//if layoutcompressions is slower than 100 then there is a problem.
	// 	//it should be at least very similar and no more than 100ms slower i guess...
	// 	Assert.LessOrEqual(delta, 100);
	// }
}