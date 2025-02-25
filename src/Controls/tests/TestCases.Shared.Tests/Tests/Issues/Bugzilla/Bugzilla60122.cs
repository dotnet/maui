using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla60122 : _IssuesUITest
{
	public Bugzilla60122(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "LongClick on image not working";


	const string ImageId = "60122Image";
	const string Success = "Success";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void LongClickFiresOnCustomImageRenderer()
	{
		App.WaitForElement(ImageId);
#if MACCATALYST
		App.LongPress(ImageId);
#else
		App.TouchAndHold(ImageId);
#endif
		App.WaitForElement(Success);
	}
}