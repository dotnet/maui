using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CollectionViewBindingErrorsUITests : _IssuesUITest
	{
		public CollectionViewBindingErrorsUITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "Binding errors when CollectionView ItemsSource is set with a binding";

		// CollectionViewBindingErrorsShouldBeZero (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewBindingErrors.xaml.cs)
		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void NoBindingErrors()
		{
			App.WaitForElement("WaitForStubControl");
			App.WaitForNoElement("Binding Errors: 0");
		}
	}
}