using System;
using System.IO;

// gdk-pixbuf is the only GTK 4 animation decoder in the bindings (GTK deprecates it, but it is not going away).
#pragma warning disable CS0612, CS0618

namespace Microsoft.Maui.Gtk.ImageSources
{
	/// <summary>
	/// A decoded multi-frame image (GIF, APNG, animated WebP) over <c>Gdk.PixbufAnimation</c>. The animation owns an iterator; <see cref="Advance"/>
	/// moves it to the frame due now and <see cref="CurrentTexture"/> is the frame to show.
	/// </summary>
	internal sealed class GtkImageAnimation : IDisposable
	{
		readonly Gdk.PixbufAnimation _animation;
		readonly Gdk.PixbufAnimationIter _iterator;
		Gdk.Texture? _texture;

		GtkImageAnimation(Gdk.PixbufAnimation animation)
		{
			_animation = animation;
			_iterator = animation.GetIter(IntPtr.Zero);
		}

		/// <summary>Whether <see cref="Dispose"/> ran (a ticking player must stop using it).</summary>
		public bool IsDisposed { get; private set; }

		/// <summary>The frame to show now.</summary>
		public Gdk.Texture CurrentTexture => _texture ??= new Gdk.Texture(_iterator.Pixbuf);

		/// <summary>Milliseconds the current frame stays on screen; <c>-1</c> when it stays forever (last frame of a non-looping animation).</summary>
		public int DelayMilliseconds => _iterator.DelayTime;

		/// <summary>Moves to the frame due at the current time.</summary>
		/// <returns><see langword="true"/> when the frame changed (<see cref="CurrentTexture"/> is then a new texture).</returns>
		public bool Advance()
		{
			if (IsDisposed || !_iterator.Advance(IntPtr.Zero))
				return false;

			_texture = null;
			return true;
		}

		/// <summary>
		/// Whether <paramref name="data"/> can hold an animation: a GIF, a PNG with an <c>acTL</c> chunk (APNG) or a WebP with an <c>ANIM</c> chunk.
		/// A cheap header sniff so static images keep the plain texture path.
		/// </summary>
		public static bool MayBeAnimated(ReadOnlySpan<byte> data)
		{
			if (data.Length < 12)
				return false;

			// GIF87a / GIF89a
			if (data[0] == 'G' && data[1] == 'I' && data[2] == 'F' && data[3] == '8')
				return true;

			// PNG: acTL must precede the first IDAT.
			if (data[0] == 0x89 && data[1] == 'P' && data[2] == 'N' && data[3] == 'G')
			{
				var idat = data.IndexOf("IDAT"u8);
				var actl = data.IndexOf("acTL"u8);
				return actl >= 0 && (idat < 0 || actl < idat);
			}

			// RIFF....WEBP with an ANIM chunk in the VP8X header area.
			if (data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F' && data[8] == 'W' && data[9] == 'E' && data[10] == 'B' && data[11] == 'P')
				return data.IndexOf("ANIM"u8) >= 0;

			return false;
		}

		/// <summary>Decodes <paramref name="data"/> when it is an animation; <see langword="null"/> for a still image or on failure.</summary>
		/// <param name="data">The encoded image.</param>
		public static GtkImageAnimation? TryCreate(byte[] data)
		{
			if (!MayBeAnimated(data))
				return null;

			try
			{
				using var loader = new Gdk.PixbufLoader();
				loader.Write(data);
				loader.Close();
				var animation = loader.Animation;
				if (animation is null || animation.IsStaticImage)
					return null;

				return new GtkImageAnimation(animation);
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>Reads <paramref name="path"/> and decodes it when it is an animation.</summary>
		/// <param name="path">The image file.</param>
		public static GtkImageAnimation? TryCreateFromFile(string path)
		{
			try
			{
				return TryCreate(File.ReadAllBytes(path));
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (IsDisposed)
				return;

			IsDisposed = true;
			_texture = null;
			_iterator.Dispose();
			_animation.Dispose();
		}
	}

	/// <summary>What an animation-aware load produced: a still <see cref="Texture"/>, or an <see cref="Animation"/> (whose first frame is <see cref="Texture"/>).</summary>
	internal sealed class GtkLoadedImage
	{
		public GtkLoadedImage(Gdk.Texture texture, GtkImageAnimation? animation)
		{
			Texture = texture;
			Animation = animation;
		}

		public Gdk.Texture Texture { get; }

		public GtkImageAnimation? Animation { get; }
	}

	/// <summary>An image source service that can also decode multi-frame images.</summary>
	internal interface IGtkAnimatedImageSourceService : IGtkImageSourceService
	{
		/// <summary>Loads <paramref name="imageSource"/>, decoding it as an animation when it is one; <see langword="null"/> when it cannot be loaded.</summary>
		/// <param name="imageSource">The MAUI image source.</param>
		/// <param name="cancellationToken">Cancels the load.</param>
		System.Threading.Tasks.Task<GtkLoadedImage?> GetImageAsync(IImageSource imageSource, System.Threading.CancellationToken cancellationToken = default);
	}
}
