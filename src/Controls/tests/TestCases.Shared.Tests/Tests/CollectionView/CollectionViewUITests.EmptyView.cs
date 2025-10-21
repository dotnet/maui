#if TEST_FAILS_ON_WINDOWS // EmptyView Elements not accessible via Automation on Windows, Issue Link: https://github.com/dotnet/maui/issues/28022
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{

	public class CollectionViewEmptyViewTests : CollectionViewUITests
	{
		protected override bool ResetAfterEachTest => true;

		public CollectionViewEmptyViewTests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void EmptyViewItemsSourceNullStringWorks()
		{
			VisitInitialGallery("EmptyView");

			VisitSubGallery("EmptyView (null ItemsSource)");

			// EmptyView string
			App.WaitForElement("Nothing to display.");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void EmptyViewItemsSourceNullViewWorks()
		{
			VisitInitialGallery("EmptyView");

			VisitSubGallery("EmptyView (null ItemsSource) View");

			// EmptyView string
			App.WaitForElement("Nothing to display.");
		}
	}

}
#endif