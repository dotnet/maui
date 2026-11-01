using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Platform;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Image paints (<c>ImageBrush</c>, <c>Page.BackgroundImageSource</c>) as the background of any view: CSS <c>background-image</c> cover.</summary>
	public class ImagePaintBackgroundTests
	{
		static string Css(global::Gtk.Widget widget) => GtkStyle.For(widget).ToCss();

		static bool IsRed((byte R, byte G, byte B) pixel) => pixel.R > 200 && pixel.G < 60 && pixel.B < 60;

		static ImageSource Red() => ImageSource.FromStream(() => new MemoryStream(TestImages.CreatePng(16, 16, 255, 0, 0)));

		static ImageSource Green() => ImageSource.FromStream(() => new MemoryStream(TestImages.CreatePng(16, 16, 0, 255, 0)));

		[GtkFact]
		public void LayoutBackgroundImageBrushPaintsTheImage() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var inner = new Label { Text = "x" };
			var grid = new Grid { Background = new ImageBrush(Red()), Children = { inner } };
			using var host = new WindowHost(grid, 120, 80);
			var widget = host.Widget(grid);

			Assert.True(GtkTestHost.PumpUntil(() => Css(widget).Contains("data:image/png;base64,"), host.PlatformWindow), "the background image was never applied");
			Assert.Contains("background-size: cover", Css(widget));
			host.Pump();
			var bounds = GtkTestHost.BoundsIn(widget, host.PlatformWindow.Child);
			Assert.True(GtkTestHost.PumpUntil(() => IsRed(GtkTestHost.PixelAt(host.PlatformWindow.Child!, bounds.X + bounds.Width - 5, bounds.Y + bounds.Height - 5)), host.PlatformWindow), "the background image was never painted");
			GtkTestHost.AssertNoComplaints();
		});

		[GtkFact]
		public void BackgroundImageIsReplacedAndClearedByLaterPaints() => GtkTestHost.Run(() =>
		{
			var entry = new Entry { Background = new ImageBrush(Red()) };
			using var host = new WindowHost(new Grid { Children = { entry } }, 120, 60);
			var widget = host.Widget(entry);
			Assert.True(GtkTestHost.PumpUntil(() => Css(widget).Contains("data:image/png"), host.PlatformWindow));
			var first = Css(widget);

			entry.Background = new ImageBrush(Green());
			Assert.True(GtkTestHost.PumpUntil(() => Css(widget).Contains("data:image/png") && Css(widget) != first, host.PlatformWindow));

			entry.Background = new SolidColorBrush(Colors.Blue);
			Assert.DoesNotContain("data:image", Css(widget));
			Assert.Contains("background-color: rgba(0,0,255,1)", Css(widget));

			entry.Background = new ImageBrush(Red());
			Assert.True(GtkTestHost.PumpUntil(() => Css(widget).Contains("data:image/png"), host.PlatformWindow));
			entry.Background = null!;
			GtkTestHost.PumpUntil(() => false, host.PlatformWindow, 200);
			Assert.DoesNotContain("data:image", Css(widget));
			Assert.DoesNotContain("background", Css(widget));
		});

		[GtkFact]
		public void PageBackgroundImageSourcePaintsTheImage() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x" };
			var page = new ContentPage { BackgroundImageSource = Red(), Content = label };
			var widget = GtkTestApp.CreateWidget<global::Gtk.Widget>(page);
			Assert.True(GtkTestHost.PumpUntil(() => Css(widget).Contains("data:image/png;base64,")), "Page.BackgroundImageSource was never applied");
		});
	}
}
