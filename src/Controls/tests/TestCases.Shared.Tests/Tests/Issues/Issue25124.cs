#if TEST_FAILS_ON_WINDOWS
// CollectionView Header is not visible with OnPlatform in HostApp, but works in Sandbox (fixed in https://github.com/dotnet/maui/pull/28935)
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25124 : _IssuesUITest
{
	public Issue25124(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "OnPlatform does not work in the Header and Footer of CollectionView";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ShouldDisplayHeaderBasedOnOnPlatform()
	{
		App.WaitForElement("CollectionView");
		VerifyScreenshot();
	}
}
#endif