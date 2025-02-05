using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3475 : _IssuesUITest
{
	const string _withoutCompressionBtnId = "button1";
	const string _withCompressionBtnId = "button2";
	const string _titleLabelId = "Label1";
	const int ItemsCount = 150;
	const string ElapsedLabelId = "elapsed";
	const string DoneLabelId = "done";

	public Issue3475(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] LayoutCompression Performance Issues";

	[Test]
	[Category(UITestCategories.Layout)]
	public void Issue3475TestsLayoutCompressionPerformance()
	{
		App.WaitForElement(_titleLabelId);
		App.WaitForElement(_withoutCompressionBtnId);
		App.WaitForElement(_withCompressionBtnId);

		App.Tap(_withoutCompressionBtnId);
		App.WaitForElement(DoneLabelId);

		int elapsedWithoutCompression = GetMs(App.WaitForElement(ElapsedLabelId).GetText()!);

		App.TapBackArrow();
		App.WaitForElement(_withCompressionBtnId);

		App.Tap(_withCompressionBtnId);
		App.WaitForElement(DoneLabelId);


		int elapsedWithCompression = GetMs(App.WaitForElement(ElapsedLabelId).GetText()!);
		var delta = elapsedWithCompression - elapsedWithoutCompression;

		//if layoutcompressions is slower than 100 then there is a problem.
		//it should be at least very similar and no more than 100ms slower i guess...
		Assert.That(delta, Is.LessThanOrEqualTo(100));
	}

	public int GetMs(string text)
	{
		text = text.Replace($"Showing {ItemsCount} items took: ", "", StringComparison.OrdinalIgnoreCase).Replace(" ms", "", StringComparison.OrdinalIgnoreCase);
		return int.TryParse(text, out int elapsed) ? elapsed : 0;
	}
}