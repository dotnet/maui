using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CollectionViewDynamicallyLoadUITests : _IssuesUITest
	{
		const string Success = "Success";

		public CollectionViewDynamicallyLoadUITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "Often fails to draw dynamically loaded collection view content";

		// CollectionViewShouldSourceShouldUpdateWhileInvisible (src\Compatibility\ControlGallery\src\Issues.Shared\Issue13126.cs)
		[Fact]
		[Category(UITestCategories.CollectionView)]
		public void DynamicallyLoadCollectionView()
		{
			App.WaitForElement(Success);
		}
	}
}