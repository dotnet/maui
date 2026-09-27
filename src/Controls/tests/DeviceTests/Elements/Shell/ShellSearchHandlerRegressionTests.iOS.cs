using System;
using System.IO;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests;

public partial class ShellTests
{
	[Theory]
	[InlineData(UISearchBarIcon.Search)]
	[InlineData(UISearchBarIcon.Clear)]
	[InlineData(UISearchBarIcon.Bookmark)]
	public Task SearchHandlerIconsUpdateNativeImages35736(UISearchBarIcon icon) =>
		RunSearchHandlerRegression(async (search, bar, page) =>
		{
			SetSearchIcon(search, icon, CreateSearchIconSource(page, 12, 18));
			await AssertNativeSearchIconSize(bar, icon, new CGSize(12, 18), "Initial custom image");

			UIButton clearButton = null;
			try
			{
				if (icon == UISearchBarIcon.Clear)
				{
					// Materialize UIKit's cached clear button before changing its image.
					bar.Text = "A";
					Assert.True(bar.SearchTextField.BecomeFirstResponder(), "Clear button setup: search field could not receive focus.");
					bar.LayoutIfNeeded();
					clearButton = bar.SearchTextField.ValueForKey(new NSString("clearButton")) as UIButton;
					Assert.NotNull(clearButton);
					Assert.True(await Wait(() => !clearButton.Hidden
						&& clearButton.CurrentImage?.Size == new CGSize(12, 18)),
						$"Initial cached clear button: hidden={clearButton.Hidden}, image={DescribeSearchIcon(clearButton.CurrentImage)}; expected 12x18.");
				}

				SetSearchIcon(search, icon, CreateSearchIconSource(page, 22, 14));
				await AssertNativeSearchIconSize(bar, icon, new CGSize(22, 14), "Updated custom image");

				foreach (var state in new[] { UIControlState.Normal, UIControlState.Highlighted, UIControlState.Selected })
				{
					var image = bar.GetImageForSearchBarIcon(icon, state);
					Assert.True(image is not null && image.Size == new CGSize(22, 14)
						&& image.RenderingMode == UIImageRenderingMode.AlwaysTemplate,
						$"Updated {icon}/{state}: expected 22x14 AlwaysTemplate; actual {DescribeSearchIcon(image)}.");
				}

				if (clearButton is not null)
				{
					Assert.True(await Wait(() => clearButton.CurrentImage?.Size == new CGSize(22, 14)),
						$"Updated cached clear button: expected 22x14; actual {DescribeSearchIcon(clearButton.CurrentImage)}.");

					search.ClearIcon = null;
					Assert.True(await Wait(() => clearButton.CurrentImage is not null
						&& clearButton.CurrentImage.Size != new CGSize(22, 14)),
						$"Reset cached clear button: expected a nonblank default image; actual {DescribeSearchIcon(clearButton.CurrentImage)}.");
				}
			}
			finally
			{
				if (icon == UISearchBarIcon.Clear)
					bar.SearchTextField.ResignFirstResponder();
			}
		});

	[Fact]
	public Task SearchHandlerResetAllRestoresNativeIcons35736() =>
		RunSearchHandlerRegression(async (search, bar, page) =>
		{
			var icons = new[] { UISearchBarIcon.Search, UISearchBarIcon.Clear, UISearchBarIcon.Bookmark };
			var defaults = new CGSize?[icons.Length];
			for (int i = 0; i < icons.Length; i++)
			{
				defaults[i] = bar.GetImageForSearchBarIcon(icons[i], UIControlState.Normal)?.Size;
				Assert.NotEqual<CGSize?>(new CGSize(22, 14), defaults[i]);
				SetSearchIcon(search, icons[i], CreateSearchIconSource(page, 22, 14));
				var icon = icons[i];
				await AssertNativeSearchIconSize(bar, icon, new CGSize(22, 14), "Custom image before reset-all");
			}

			search.QueryIcon = null;
			search.ClearIcon = null;
			search.ClearPlaceholderIcon = null;

			for (int i = 0; i < icons.Length; i++)
			{
				var icon = icons[i];
				var expected = defaults[i];
				await AssertNativeSearchIconSize(bar, icon, expected, "Default image after reset-all");
			}
		});

	async Task RunSearchHandlerRegression(Func<SearchHandler, UISearchBar, ContentPage, Task> assertion)
	{
		SetupBuilder();
		var search = new SearchHandler
		{
			SearchBoxVisibility = SearchBoxVisibility.Expanded,
			ShowsResults = false
		};
		var page = new ContentPage { Title = "Search", Content = new Label { Text = "Content" } };
		Shell.SetSearchHandler(page, search);
		var shell = new Shell { Items = { page }, FlyoutBehavior = FlyoutBehavior.Disabled };

		await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async renderer =>
		{
			await OnLoadedAsync(page);
			var item = Assert.IsType<ShellItemRenderer>(((IShellContext)renderer).CurrentShellItemRenderer);
			var section = Assert.IsType<ShellSectionRenderer>(item.CurrentRenderer);
			var navigationItem = section.ViewControllers[0].NavigationItem;
			var bar = navigationItem.SearchController?.SearchBar ?? navigationItem.TitleView as UISearchBar;
			Assert.NotNull(bar);
			await AssertEventually(() => bar.Window is not null && bar.Bounds.Width > 0 && bar.Bounds.Height > 0,
				message: "SearchHandler fixture: native search bar was not attached and laid out.");
			await assertion(search, bar, page);
		});
	}

	static async Task AssertNativeSearchIconSize(UISearchBar bar, UISearchBarIcon icon, CGSize? expected, string stage)
	{
		var matched = await Wait(() => bar.GetImageForSearchBarIcon(icon, UIControlState.Normal)?.Size == expected);
		Assert.True(matched,
			$"{stage}: {icon}/Normal expected {expected?.ToString() ?? "null"}; actual {DescribeSearchIcon(bar.GetImageForSearchBarIcon(icon, UIControlState.Normal))}.");
	}

	static string DescribeSearchIcon(UIImage image) =>
		image is null ? "null" : $"size={image.Size}, scale={image.CurrentScale}, renderingMode={image.RenderingMode}";

	static ImageSource CreateSearchIconSource(ContentPage page, int width, int height)
	{
		using var format = new UIGraphicsImageRendererFormat { Opaque = false, Scale = 1 };
		using var renderer = new UIGraphicsImageRenderer(new CGSize(width, height), format);
		using var image = renderer.CreateImage(context =>
		{
			UIColor.Black.SetFill();
			context.FillRect(new CGRect(0, 0, width, height));
		});
		using var data = image.AsPNG();
		var bytes = data.ToArray();
		var source = ImageSource.FromStream(() => new MemoryStream(bytes));
		// SearchHandler icons fall back to Application.Current, which this isolated fixture does not set.
		source.Parent = page;
		var mauiContext = source.FindMauiContext();
		Assert.True(mauiContext is not null, $"Image source setup ({width}x{height}): no MAUI context on the loaded page.");
		Assert.Same(page.Handler.MauiContext, mauiContext);
		return source;
	}

	static void SetSearchIcon(SearchHandler search, UISearchBarIcon icon, ImageSource source)
	{
		switch (icon)
		{
			case UISearchBarIcon.Search:
				search.QueryIcon = source;
				break;
			case UISearchBarIcon.Clear:
				search.ClearIcon = source;
				break;
			case UISearchBarIcon.Bookmark:
				search.ClearPlaceholderIcon = source;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(icon));
		}
	}
}
