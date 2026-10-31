using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Gtk.ImageSources;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// Builds the <c>icon</c> attribute of a <c>GMenuItem</c>: a serialised <c>GIcon</c> (<c>('themed', &lt;names&gt;)</c>, <c>('file', &lt;path&gt;)</c>
	/// or <c>('bytes', &lt;ay&gt;)</c>) that <c>Gtk.PopoverMenu</c> draws in front of the label.
	/// </summary>
	/// <remarks>File and stream sources are shown; font and URI sources have no menu representation and leave the item without icon.</remarks>
	internal static class GtkMenuIcons
	{
		/// <summary>
		/// The property name Controls raises when <c>MenuItem.IconImageSource</c> changes. Core only maps <c>Source</c>, which nothing triggers for a
		/// later change, so the GTK handlers map this key as well.
		/// </summary>
		public const string ControlsIconProperty = "IconImageSource";

		static readonly char[] s_separators = { '/', (char)92 };

		/// <summary>An icon looked up by name in the icon theme.</summary>
		public static GLib.Variant Themed(string name) =>
			Serialize("themed", new GLib.Variant(new[] { name }));

		/// <summary>An icon loaded from an image file.</summary>
		public static GLib.Variant File(string path) =>
			Serialize("file", new GLib.Variant(path));

		/// <summary>An icon decoded from encoded image data.</summary>
		public static GLib.Variant Bytes(byte[] data)
		{
			var children = new GLib.Variant[data.Length];
			for (int i = 0; i < data.Length; i++)
				children[i] = new GLib.Variant(data[i]);

			return Serialize("bytes", GLib.Variant.NewArray(children));
		}

		static GLib.Variant Serialize(string kind, GLib.Variant value) =>
			GLib.Variant.NewTuple(new[] { new GLib.Variant(kind), GLib.Variant.NewVariant(value) });

		/// <summary>
		/// The icon of a file image source: the resolved image file, otherwise (for a bare name without directory) the theme icon of that name.
		/// </summary>
		public static GLib.Variant? FromFile(string? file)
		{
			if (string.IsNullOrEmpty(file))
				return null;

			if (GtkFileImageSourceService.Resolve(file) is { } path)
				return File(path);

			var name = Path.GetFileNameWithoutExtension(file);
			return string.IsNullOrEmpty(name) || file!.IndexOfAny(s_separators) >= 0 ? null : Themed(name);
		}

		/// <summary>
		/// Converts <paramref name="source"/> to an icon and passes it to <paramref name="apply"/> on the GTK thread. File sources are applied
		/// synchronously, streams after they were read; a result is dropped when <paramref name="isCurrent"/> is false by then.
		/// </summary>
		public static void Load(IImageSource? source, Func<bool> isCurrent, Action<GLib.Variant?> apply)
		{
			switch (source)
			{
				case null:
					apply(null);
					break;
				case IFileImageSource file:
					apply(FromFile(file.File));
					break;
				case IStreamImageSource stream:
					apply(null);
					_ = LoadStreamAsync(stream, isCurrent, apply);
					break;
				default:
					apply(null);
					break;
			}
		}

		static async Task LoadStreamAsync(IStreamImageSource source, Func<bool> isCurrent, Action<GLib.Variant?> apply)
		{
			byte[]? data = null;
			try
			{
				using var stream = await source.GetStreamAsync(CancellationToken.None).ConfigureAwait(false);
				if (stream is not null)
				{
					using var memory = new MemoryStream();
					await stream.CopyToAsync(memory).ConfigureAwait(false);
					data = memory.ToArray();
				}
			}
			catch (Exception)
			{
				// An unreadable stream leaves the item without icon.
			}

			if (data is not { Length: > 0 })
				return;

			GtkRuntime.Post(() =>
			{
				if (isCurrent())
					apply(Bytes(data));
			});
		}
	}
}
