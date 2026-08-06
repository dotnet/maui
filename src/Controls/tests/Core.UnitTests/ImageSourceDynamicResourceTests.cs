using System;
using System.Linq;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// Follow-up to https://github.com/dotnet/maui/issues/36822 (PR #37065):
	// ImageSource-valued properties are element-valued but were never parented, so
	// DynamicResources inside them only resolved through the removed Application.Current
	// fallback (and only for app-level resources). The image sources now get their
	// Parent assigned to the owning element (the ImageElement pattern) so they resolve
	// through the element tree without becoming logical/visual children.
	public class ImageSourceDynamicResourceTests : BaseTestFixture
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.ClearCurrent();
			}
			base.Dispose(disposing);
		}

		static Application SetUpAppWithAccentColor()
		{
			var app = new MockApplication
			{
				Resources = new ResourceDictionary { { "AccentColor", Colors.Red } }
			};
			Application.Current = app;
			return app;
		}

		[Fact]
		public void IconImageSourceDynamicResourceResolvesThroughTree()
		{
			var app = SetUpAppWithAccentColor();

			var icon = new FontImageSource { Glyph = "x" };
			icon.SetDynamicResource(FontImageSource.ColorProperty, "AccentColor");

			var page = new ContentPage { IconImageSource = icon };
			app.LoadPage(page);

			Assert.Equal(page, icon.Parent);
			Assert.Equal(Colors.Red, icon.Color);
		}

		[Fact]
		public void IconImageSourceDynamicResourceResolvesPageLevelResources()
		{
			var app = SetUpAppWithAccentColor();

			var icon = new FontImageSource { Glyph = "x" };
			icon.SetDynamicResource(FontImageSource.ColorProperty, "AccentColor");

			// Page-level resources shadow the application ones; with the old static
			// Application.Current fallback this resolved the wrong scope
			var page = new ContentPage
			{
				Resources = new ResourceDictionary { { "AccentColor", Colors.Blue } },
				IconImageSource = icon
			};
			app.LoadPage(page);

			Assert.Equal(Colors.Blue, icon.Color);
		}

		[Fact]
		public void BackgroundImageSourceDynamicResourceResolvesThroughTree()
		{
			var app = SetUpAppWithAccentColor();

			var background = new FontImageSource { Glyph = "x" };
			background.SetDynamicResource(FontImageSource.ColorProperty, "AccentColor");

			var page = new ContentPage { BackgroundImageSource = background };
			app.LoadPage(page);

			Assert.Equal(page, background.Parent);
			Assert.Equal(Colors.Red, background.Color);
		}

		[Fact]
		public void ReplacedIconImageSourceIsUnparented()
		{
			var app = SetUpAppWithAccentColor();

			var first = new FontImageSource { Glyph = "a" };
			var second = new FontImageSource { Glyph = "b" };

			var page = new ContentPage { IconImageSource = first };
			app.LoadPage(page);

			page.IconImageSource = second;

			Assert.Null(first.Parent);
			Assert.Equal(page, second.Parent);
		}

		[Fact]
		public void ImageSourceIsNotExposedAsLogicalOrVisualChild()
		{
			var app = SetUpAppWithAccentColor();

			var icon = new FontImageSource { Glyph = "x" };
			var page = new ContentPage { IconImageSource = icon };
			app.LoadPage(page);

			Assert.Equal(page, icon.Parent);
			// Compatibility renderers auto-package visual children; an ImageSource has no
			// view handler, so it must not surface through the visual tree
			Assert.DoesNotContain((IVisualTreeElement)icon, ((IVisualTreeElement)page).GetVisualChildren());
			Assert.DoesNotContain(icon, page.LogicalChildrenInternal);
		}

		[Fact]
		public void SharedImageSourceKeepsMostRecentOwnerWhenClearedFromAnother()
		{
			SetUpAppWithAccentColor();

			var icon = new FontImageSource { Glyph = "x" };
			var pageA = new ContentPage { IconImageSource = icon };
			var pageB = new ContentPage { IconImageSource = icon };

			// The most recent assignment wins
			Assert.Equal(pageB, icon.Parent);

			// Clearing the property on a non-owner must not steal the parent from the owner
			pageA.IconImageSource = null;

			Assert.Equal(pageB, icon.Parent);
		}

		[Fact]
		public void TitleBarIconStaysParentedAfterControlTemplateChange()
		{
			SetUpAppWithAccentColor();

			var icon = new FontImageSource { Glyph = "x" };
			icon.SetDynamicResource(FontImageSource.ColorProperty, "AccentColor");

			var titleBar = new TitleBar
			{
				Resources = new ResourceDictionary { { "AccentColor", Colors.Green } },
				Icon = icon
			};

			Assert.Equal(Colors.Green, icon.Color);

			// Replacing the template clears the TitleBar's logical children; the icon must
			// survive because it is parented directly, not registered as a logical child
			titleBar.ControlTemplate = new ControlTemplate(() =>
			{
				var grid = new Grid();
				Internals.NameScope.SetNameScope(grid, new Internals.NameScope());
				return grid;
			});

			Assert.Equal(titleBar, icon.Parent);
			Assert.Equal(Colors.Green, icon.Color);
		}

		[Fact]
		public void ShellContentIconDynamicResourceResolvesThroughTree()
		{
			var app = SetUpAppWithAccentColor();

			var icon = new FontImageSource { Glyph = "x" };
			icon.SetDynamicResource(FontImageSource.ColorProperty, "AccentColor");

			var shellContent = new ShellContent { Content = new ContentPage() };
			var shell = new Shell();
			shell.Items.Add(shellContent);
			app.LoadPage(shell);

			shellContent.Icon = icon;

			Assert.Equal(shellContent, icon.Parent);
			Assert.Equal(Colors.Red, icon.Color);
		}

		[Fact]
		public void ClearingIconKeepsForwardedFlyoutIconParented()
		{
			SetUpAppWithAccentColor();

			var icon = new FontImageSource { Glyph = "x" };
			var shellContent = new ShellContent { Icon = icon };

			// Icon forwards to FlyoutIcon when FlyoutIcon is not explicitly set
			Assert.Same(icon, shellContent.FlyoutIcon);

			shellContent.Icon = null;

			// Still referenced through FlyoutIcon, so it must stay parented
			Assert.Equal(shellContent, icon.Parent);
		}

		[Fact]
		public void ClearingAnImageSourceFromAnOlderOwnerKeepsTheCurrentOwnersParent()
		{
			SetUpAppWithAccentColor();

			var source = new FontImageSource { Glyph = "x" };

			// ImageElement (Image/Button/ImageButton) parents sources too, and the same
			// source can legitimately be assigned to more than one owner
			var first = new Image { Source = source };
			var second = new Image { Source = source };

			Assert.Equal(second, source.Parent);

			// The older owner no longer holds the parent, so clearing it must not unparent
			// the source from the owner that does
			first.Source = null;

			Assert.Equal(second, source.Parent);
		}

		[Fact]
		public void AllImageSourcePropertiesParentTheirSource()
		{
			SetUpAppWithAccentColor();

			var owners = new (string Name, Func<ImageSource, Element> Assign)[]
			{
				("Page.IconImageSource", src => new ContentPage { IconImageSource = src }),
				("Page.BackgroundImageSource", src => new ContentPage { BackgroundImageSource = src }),
				("NavigationPage.TitleIconImageSource", src => { var page = new ContentPage(); NavigationPage.SetTitleIconImageSource(page, src); return page; }),
				("Slider.ThumbImageSource", src => new Slider { ThumbImageSource = src }),
				("ImageCell.ImageSource", src => new ImageCell { ImageSource = src }),
				("AppLinkEntry.Thumbnail", src => new AppLinkEntry { Thumbnail = src }),
				("TitleBar.Icon", src => new TitleBar { Icon = src }),
				("Shell.FlyoutIcon", src => new Shell { FlyoutIcon = src }),
				("Shell.FlyoutBackgroundImage", src => new Shell { FlyoutBackgroundImage = src }),
				("BaseShellItem.Icon", src => new ShellContent { Icon = src }),
				("BaseShellItem.FlyoutIcon", src => new ShellContent { FlyoutIcon = src }),
			};

			foreach (var (name, assign) in owners)
			{
				var source = new FontImageSource { Glyph = "x" };
				var owner = assign(source);
				Assert.True(ReferenceEquals(source.Parent, owner), $"{name} should parent its ImageSource");
			}
		}
	}
}
