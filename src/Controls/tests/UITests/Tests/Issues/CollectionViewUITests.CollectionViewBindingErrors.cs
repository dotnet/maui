using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
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
		public void NoBindingErrors()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac });

			App.WaitForElement("WaitForStubControl");
			App.WaitForNoElement("Binding Errors: 0");
		}
	}
}