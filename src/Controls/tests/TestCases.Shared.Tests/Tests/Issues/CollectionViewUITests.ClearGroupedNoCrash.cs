using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
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
		[Category(UITestCategories.CollectionView)]
		public void ClearingGroupedNoCrash()
		{
			App.WaitForElement(Go);
			App.Click(Go);

			App.WaitForElement(Success);
		}
	}
}