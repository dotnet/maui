using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

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

		[Fact]
		public void TestConstructor()
		{
			TabbedPage page = new TabbedPage();

			Assert.Empty(page.Children);
		}
	}
}
