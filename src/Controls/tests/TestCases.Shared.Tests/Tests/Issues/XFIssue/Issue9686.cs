#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID  //App crashes on Android and Windows, Issue: https://github.com/dotnet/maui/issues/26427
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9686 : _IssuesUITest
{
	const string Success = "Success";
	const string Run = "Run";
	public Issue9686(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug, CollectionView,iOS] Foundation.Monotouch Exception in Grouped CollectionView";

	[Test]
	[Category(UITestCategories.CollectionView)]

	public void AddRemoveEmptyGroupsShouldNotCrashOnInsert()
	{
		App.WaitForElement(Run);
		App.Tap(Run);
		App.WaitForElement("Item 1");
		App.Tap(Run);
		App.WaitForElement(Success, timeout: TimeSpan.FromSeconds(1));
	}
}
#endif