#if TEST_FAILS_ON_ANDROID // https://github.com/dotnet/maui/issues/24504
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24520 : _IssuesUITest
{
	public Issue24520(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Change the LineStackingStrategy to BlockLineHeight for Labels on Windows";

	[Test]
	[Category(UITestCategories.Label)]
	public void VerifyLineHeightRendering()
	{
		VerifyScreenshot();
	}
}
#endif