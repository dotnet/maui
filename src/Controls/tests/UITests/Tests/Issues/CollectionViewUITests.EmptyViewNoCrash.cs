using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
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
		public void EmptyViewShouldNotCrash()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac });

			App.WaitForNoElement("Success");
		}
	}
}