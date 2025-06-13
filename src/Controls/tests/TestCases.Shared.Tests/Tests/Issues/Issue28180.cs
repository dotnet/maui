#if IOSUITEST || MACUITEST

using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28180 : _IssuesUITest
{
	public Issue28180(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Labels with Padding are truncated on iOS";

	[Fact]
	[Category(UITestCategories.Label)]
	public void LabelWithPaddingIsNotTruncated()
	{
		App.WaitForElement("LongTextLabel");
		VerifyScreenshot();
	}
}

#endif