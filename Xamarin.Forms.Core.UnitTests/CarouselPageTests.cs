using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
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

		[Test]
		public void TestConstructor()
		{
			var page = new CarouselPage();
			Assert.That(page.Children, Is.Empty);
		}
	}
}