using System.Linq;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TabbedPageTests : MultiPageTests<Page>
	{
		protected override MultiPage<Page> CreateMultiPage()
		{
			return new TabbedPage();
		}

		protected override Page CreateContainedPage()
		{
			return new ContentPage { Content = new View() };
		}

		protected override int GetIndex(Page page)
		{
			return TabbedPage.GetIndex(page);
		}

		[Test]
		public void TestConstructor()
		{
			TabbedPage page = new TabbedPage();

			Assert.That(page.Children, Is.Empty);
		}
	}
}