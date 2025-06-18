#if TEST_FAILS_ON_WINDOWS //Test failing on windows for more information see https://github.com/dotnet/maui/issues/18481
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25514 : _IssuesUITest
	{
		public Issue25514(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Grouped CollectionView with header template and templateselector crashes";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest("Flaky test on Windows https://github.com/dotnet/maui/issues/27059")]
		public void AppShouldNotCrashAfterLoadingGroupedCollectionView()
		{
			App.WaitForElement("button");
			App.Click("button");
		}
	}
}
#endif