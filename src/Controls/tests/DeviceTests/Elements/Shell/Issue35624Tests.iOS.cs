using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue35624 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task SearchHandlerAppliesCharacterSpacingToNativeText()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers => SetupShellHandlers(handlers));
			});

			const double expectedCharacterSpacing = 12;
			var searchHandler = new SearchHandler
			{
				CharacterSpacing = expectedCharacterSpacing,
				SearchBoxVisibility = SearchBoxVisibility.Expanded,
			};
			var page = new ContentPage();
			Shell.SetSearchHandler(page, searchHandler);

			var shell = new Shell
			{
				FlyoutBehavior = FlyoutBehavior.Disabled,
				Items = { page },
			};

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, handler =>
			{
				var shellContext = (IShellContext)handler;
				var shellItemRenderer = Assert.IsType<ShellItemRenderer>(shellContext.CurrentShellItemRenderer);
				var sectionRenderer = Assert.IsType<ShellSectionRenderer>(shellItemRenderer.CurrentRenderer);
				var navigationItem = sectionRenderer.TopViewController.NavigationItem;
				var platformSearchBar = navigationItem.SearchController?.SearchBar ?? navigationItem.TitleView;
				var searchBar = Assert.IsType<UISearchBar>(platformSearchBar);

				searchBar.Text = "SPACING";

				var actualCharacterSpacing = searchBar.SearchTextField.AttributedText.GetCharacterSpacing();
				Assert.True(
					Math.Abs(expectedCharacterSpacing - actualCharacterSpacing) < 0.001,
					$"SearchHandler should apply CharacterSpacing to the native search field. Expected {expectedCharacterSpacing}, but found {actualCharacterSpacing}.");

				return Task.CompletedTask;
			});
		}
	}
}
