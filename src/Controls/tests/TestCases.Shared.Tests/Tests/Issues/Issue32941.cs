#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // SafeAreaEdges not supported on Catalyst and Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32941 : _IssuesUITest
{
	public Issue32941(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Label Overlapped by Android Status Bar When Using SafeAreaEdges=Container in .NET MAUI";

	[Test]
	[Category(UITestCategories.Shell)]
	[Category(UITestCategories.SafeAreaEdges)]
	public void ShellContentShouldRespectSafeAreaEdges_After_Navigation()
	{
		App.WaitForElement("NavigateToNextPageBtn");
		App.Tap("NavigateToNextPageBtn");
		VerifyScreenshot();
	}
}
#endif