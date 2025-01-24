using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13126_2 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue13126_2(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Regression: 5.0.0-pre5 often fails to draw dynamically loaded collection view content";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewShouldSourceShouldResetWhileInvisible()
		{
			App.WaitForElement(Success);
		}
	}
}