using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue35667 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task SearchHandlerTransformsEnteredTextToUppercase()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers => SetupShellHandlers(handlers));
			});

			var page = new ContentPage
			{
				Title = "Search",
				Content = new Label { Text = "Search" }
			};
			var searchHandler = new SearchHandler
			{
				SearchBoxVisibility = SearchBoxVisibility.Expanded,
				TextTransform = TextTransform.Uppercase
			};
			Shell.SetSearchHandler(page, searchHandler);

			var shell = new Shell
			{
				FlyoutBehavior = FlyoutBehavior.Disabled,
				CurrentItem = page
			};

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async handler =>
			{
				UISearchController searchController = null;
				await AssertEventually(() =>
					(searchController = FindSearchController(handler.ViewController)) is not null);

				var searchBar = searchController.SearchBar;
				var textField = searchBar.SearchTextField;
				textField.BecomeFirstResponder();

				foreach (var character in "maui")
					textField.InsertText(character.ToString());

				await AssertEventually(() => !string.IsNullOrEmpty(searchHandler.Query));

				Assert.True(
					searchHandler.Query == "MAUI" && searchBar.Text == "MAUI",
					$"Shell SearchHandler should display entered text in uppercase. Query: {searchHandler.Query}; native text: {searchBar.Text}.");
			});
		}

		static UISearchController FindSearchController(UIViewController viewController)
		{
			if (viewController.NavigationItem.SearchController is UISearchController searchController)
				return searchController;

			foreach (var child in viewController.ChildViewControllers)
			{
				searchController = FindSearchController(child);
				if (searchController is not null)
					return searchController;
			}

			return null;
		}
	}
}
