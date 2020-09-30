using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Ignore("check if the last one fails")]
	[Category(UITestCategories.Cells)]
	internal class UnevenListTests : BaseTestFixture
	{
		public UnevenListTests()
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.UnevenListGalleryLegacy);
		}

		[Test]
		public void UnevenListCellTest()
		{
			if (UnevenListTests.ShouldRunTest(App))
			{
				var element = App.Query(q => q.Marked("unevenCellListGalleryDynamic").Descendant(("UITableViewCellContentView")))[0];

				Assert.GreaterOrEqual(element.Rect.Height, 100);
			}
		}

		public static bool ShouldRunTest(IApp app)
		{
			return app.IsPhone();
		}
	}
}