using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ToolbarTests : BaseTestFixture
	{
		[Test]
		public void ToolbarExistsForNavigationPage()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var startingPage = new NavigationPage(new ContentPage());
			window.Page = startingPage;
			Assert.IsNotNull(toolbarElement.Toolbar);
		}

		[Test]
		public void ToolbarEmptyForContentPage()
		{
			Window window = new Window();
			IToolbarElement toolbarElement = window;
			var startingPage = new ContentPage();
			window.Page = startingPage;
			Assert.IsNull(toolbarElement.Toolbar);
		}

		[Test]
		public void ToolbarClearsWhenNavigationPageRemoved()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var startingPage = new NavigationPage(new ContentPage());
			window.Page = startingPage;
			window.Page = new ContentPage();
			Assert.IsNull(toolbarElement.Toolbar);
		}
	}
}