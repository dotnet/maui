using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.ImageSources;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>Bridges decoded textures into CSS, which paints per-widget images (slider thumbs, image-paint backgrounds).</summary>
	internal static class GtkTextureCss
	{
		/// <summary>A CSS <c>url()</c> holding <paramref name="texture"/> as an inline PNG; <see langword="null"/> when there is no texture.</summary>
		/// <param name="texture">The decoded image.</param>
		public static string? ToUrl(Gdk.Texture? texture)
		{
			var png = texture?.SaveToPngBytes()?.Data;
			return png is null || png.Length == 0 ? null : "url(\"data:image/png;base64," + Convert.ToBase64String(png) + "\")";
		}
	}

	/// <summary>
	/// Paints a MAUI <see cref="Paint"/> background on a widget, including the image paint (<c>Page.BackgroundImageSource</c>, <c>ImageBrush</c>)
	/// that <see cref="GtkStyle.SetBackground"/> cannot: the image source is decoded through the handler's services and drawn as a CSS
	/// <c>background-image</c> scaled to cover the widget.
	/// </summary>
	internal static class GtkBackgroundImage
	{
		static readonly System.Runtime.CompilerServices.ConditionalWeakTable<global::Gtk.Widget, System.Collections.Generic.Dictionary<string, GtkImageLoader>> s_loaders = new();

		/// <summary>Applies <paramref name="paint"/> to <paramref name="widget"/> (clearing what a previous paint set).</summary>
		/// <param name="handler">The handler (for its image services).</param>
		/// <param name="widget">The widget.</param>
		/// <param name="paint">The background paint; <see langword="null"/> clears it.</param>
		/// <param name="selector">The CSS sub-selector.</param>
		public static void Update(IElementHandler handler, global::Gtk.Widget widget, Paint? paint, string selector = "")
		{
			var style = GtkStyle.For(widget);
			style.SetBackground(paint, selector);

			var loaders = s_loaders.GetOrCreateValue(widget);
			if (paint is not ImageSourcePaint { ImageSource: { } source })
			{
				if (loaders.TryGetValue(selector, out var stale))
					stale.Reset();
				return;
			}

			if (!loaders.TryGetValue(selector, out var loader))
				loaders[selector] = loader = new GtkImageLoader();

			_ = loader.UpdateAsync(handler, new GtkImageSourcePart(source), texture =>
			{
				style.ClearPrefix("background", selector);
				var url = GtkTextureCss.ToUrl(texture);
				if (url is null)
					return;

				style.Set("background-image", url, selector);
				style.Set("background-size", "cover", selector);
				style.Set("background-position", "center", selector);
				style.Set("background-repeat", "no-repeat", selector);
			});
		}

	}

	/// <summary>An <see cref="IImageSourcePart"/> over a bare <see cref="IImageSource"/> (a slider thumb or an image paint, which are not image views).</summary>
	internal sealed class GtkImageSourcePart : IImageSourcePart
	{
		public GtkImageSourcePart(IImageSource? source) => Source = source;

		public IImageSource? Source { get; }

		public bool IsAnimationPlaying => false;

		public void UpdateIsLoading(bool isLoading)
		{
		}
	}
}
