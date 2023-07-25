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

		[Fact]
		public void LogicalAndInternalChildrenMaintainOrder()
		{
			TabbedPage tabbedPage = new TabbedPage();

			ContentPage page1 = new ContentPage();
			ContentPage page2 = new ContentPage();

			tabbedPage.Children.Add(page2);
			tabbedPage.Children.Insert(0, page1);
			tabbedPage.Children.Remove(page1);
			tabbedPage.Children.Insert(0, page1);

			Assert.Equal(tabbedPage.LogicalChildren[0], tabbedPage.InternalChildren[0]);
			Assert.Equal(tabbedPage.LogicalChildren[1], tabbedPage.InternalChildren[1]);
		}
	}
}
