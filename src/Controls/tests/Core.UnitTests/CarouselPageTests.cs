using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class CarouselPageTests : MultiPageTests<ContentPage>
	{
		protected override MultiPage<ContentPage> CreateMultiPage()
		{
			return new CarouselPage();
		}

		protected override ContentPage CreateContainedPage()
		{
			return new ContentPage { Content = new View() };
		}

		protected override int GetIndex(ContentPage page)
		{
			return CarouselPage.GetIndex(page);
		}

		[Fact]
		public void TestConstructor()
		{
			var page = new CarouselPage();
			Assert.Empty(page.Children);
		}
	}
}
