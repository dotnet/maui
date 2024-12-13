using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class EmptyViewNoCrashUITests : _IssuesUITest
	{
		public EmptyViewNoCrashUITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "CollectionView EmptyView causes the application to crash";

		// EmptyViewShouldNotCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue9196.xaml.cs)
		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnIOSWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void EmptyViewShouldNotCrash()
		{
			App.WaitForNoElement("Success");
		}
	}
}