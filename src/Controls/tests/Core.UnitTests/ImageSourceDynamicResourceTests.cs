using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// Follow-up to https://github.com/dotnet/maui/issues/36822 (PR #37065):
	// ImageSource-valued properties are element-valued but were never parented, so
	// DynamicResources inside them only resolved through the removed Application.Current
	// fallback (and only for app-level resources). The image sources are now added as
	// logical children of their owner so they resolve through the element tree.
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
	}
}
