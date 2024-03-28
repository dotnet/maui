using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class ClearGroupedNoCrashUITests : _IssuesUITest
	{
		const string Go = "Go";
		const string Success = "Success";

		public ClearGroupedNoCrashUITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "Clearing Grouped CollectionView crashes application";

		// ClearingGroupedCollectionViewShouldNotCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue8899.cs)
		[Test]
		[Description("Clearing CollectionView IsGrouped=\"True\" no crashes application")]
		public void ClearingGroupedNoCrash()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac });

			App.WaitForElement(Go);
			App.Click(Go);

			App.WaitForNoElement(Success);
		}
	}
}