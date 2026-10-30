using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Media;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="IScreenshot"/> for GTK: renders a widget through <c>Gtk.WidgetPaintable</c> into a <c>Gdk.Texture</c> using the
/// renderer of the widget's native surface, then encodes it. The widget must be realized and mapped (have a non-zero size).
/// </summary>
/// <remarks>
/// The same capture is exposed to <c>ViewExtensions.CaptureAsync</c>/<c>WindowExtensions.CaptureAsync</c> through the keyed
/// services described in <c>ScreenshotDispatch</c> (see <c>GtkEssentialsExtensions.UseMauiGtkEssentials</c>).
/// </remarks>
sealed class GtkScreenshot : IScreenshot
{
	public bool IsCaptureSupported => true;

	/// <summary>Captures the active window (the parent window of the Essentials options).</summary>
	public Task<IScreenshotResult> CaptureAsync() =>
		GtkDispatch.InvokeAsync<IScreenshotResult>(() =>
		{
			var window = GtkEssentialsContext.GetParentWindow()
				?? throw new InvalidOperationException("There is no GTK window to capture.");

			return CaptureWidget(window)
				?? throw new InvalidOperationException("The window has no content yet: it must be shown before it can be captured.");
		});

	/// <summary>Captures an arbitrary platform view; null when it is not a widget or has no size yet.</summary>
	internal static Task<IScreenshotResult?> CaptureAsync(object platformView) =>
		GtkDispatch.InvokeAsync(() => platformView is global::Gtk.Widget widget ? CaptureWidget(widget) : null);

	static IScreenshotResult? CaptureWidget(global::Gtk.Widget widget)
	{
		int width = widget.Width, height = widget.Height;
		if (width <= 0 || height <= 0)
			return null;

		var renderer = widget.Native?.Renderer;
		if (renderer is null)
			return null;

		using var paintable = new global::Gtk.WidgetPaintable(widget);
		using var snapshot = new global::Gtk.Snapshot();
		paintable.Snapshot(snapshot, width, height);

		var node = snapshot.ToNode();
		if (node is null)
			return null;

		var texture = renderer.RenderTexture(node, node.Bounds);
		var png = texture.SaveToPngBytes().Data;
		return new GtkScreenshotResult(png, texture.Width, texture.Height);
	}
}

/// <summary>A captured image held as PNG bytes; JPEG output is transcoded through gdk-pixbuf.</summary>
sealed class GtkScreenshotResult : IScreenshotResult
{
	readonly byte[] png;

	internal GtkScreenshotResult(byte[] png, int width, int height)
	{
		this.png = png;
		Width = width;
		Height = height;
	}

	public int Width { get; }

	public int Height { get; }

	public Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100) =>
		Task.FromResult<Stream>(new MemoryStream(Encode(format, quality), writable: false));

	public async Task CopyToAsync(Stream destination, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
	{
		var bytes = Encode(format, quality);
		await destination.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
	}

	internal byte[] Encode(ScreenshotFormat format, int quality)
	{
		if (format == ScreenshotFormat.Png)
			return png;

		using var loader = new Gdk.PixbufLoader(png);
		var pixbuf = loader.Pixbuf ?? throw new InvalidOperationException("The screenshot could not be decoded for transcoding.");
		return pixbuf.SaveToBuffer("jpeg", new[] { "quality" }, new[] { Math.Clamp(quality, 0, 100).ToString(System.Globalization.CultureInfo.InvariantCulture) });
	}
}
