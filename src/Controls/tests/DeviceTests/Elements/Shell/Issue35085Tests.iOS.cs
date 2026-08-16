using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue35085 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task SearchHandlerTextMovesAfterDynamicAlignmentChanges()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
				});
			});

			var searchHandler = new SearchHandler
			{
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				Query = "Alignment sample",
				SearchBoxVisibility = SearchBoxVisibility.Expanded
			};
			var page = new ContentPage
			{
				Content = new Label { Text = "Search alignment" },
				Title = "Search alignment"
			};
			Shell.SetSearchHandler(page, searchHandler);

			var shell = new Shell
			{
				FlyoutBehavior = FlyoutBehavior.Disabled,
				Items =
				{
					new ShellContent { Content = page }
				}
			};

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async handler =>
			{
				await OnLoadedAsync(page);
				await OnNavigatedToAsync(page);

				UISearchBar searchBar = null;
				await AssertEventually(() =>
				{
					searchBar = handler.ViewController.View.FindDescendantView<UISearchBar>();
					return searchBar?.SearchTextField.Bounds.Width > 0 &&
						searchBar.SearchTextField.Text == searchHandler.Query;
				});

				var textField = searchBar.SearchTextField;
				var initialCaret = textField.GetCaretRectForPosition(textField.EndOfDocument);

				searchHandler.HorizontalTextAlignment = TextAlignment.End;
				searchHandler.VerticalTextAlignment = TextAlignment.Start;

				await AssertEventually(() =>
				{
					var updatedCaret = textField.GetCaretRectForPosition(textField.EndOfDocument);
					return updatedCaret.X > initialCaret.X + (textField.Bounds.Width / 4) &&
						updatedCaret.Y < initialCaret.Y - 1;
				}, message: "SearchHandler query text should visibly move to horizontal End and vertical Start.");
			});
		}
	}
}
