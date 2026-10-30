using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="IFilePicker"/> over <c>Gtk.FileDialog</c> (portal-backed, GTK 4.10+). Returns <see langword="null"/> when the user
/// dismisses the dialog.
/// </summary>
/// <remarks>The file type strings for the <c>Linux</c> platform may be MIME types, <c>*.ext</c> globs or bare extensions.</remarks>
sealed class GtkFilePicker : IFilePicker
{
	public async Task<FileResult?> PickAsync(PickOptions? options = null) =>
		(await PickCoreAsync(options?.PickerTitle, FileFilterSpec.Parse(options?.FileTypes?.Value), null, multiple: false).ConfigureAwait(false))?.FirstOrDefault();

	public async Task<IEnumerable<FileResult>?> PickMultipleAsync(PickOptions? options = null) =>
		await PickCoreAsync(options?.PickerTitle, FileFilterSpec.Parse(options?.FileTypes?.Value), null, multiple: true).ConfigureAwait(false);

	/// <summary>Shows the open dialog. <paramref name="filterName"/> names the file filter in the dialog.</summary>
	internal static Task<List<FileResult>?> PickCoreAsync(string? title, FileFilterSpec filter, string? filterName, bool multiple) =>
		GtkAsync.Begin<global::Gtk.FileDialog, List<FileResult>?>(
			callback =>
			{
				var dialog = new global::Gtk.FileDialog { Modal = true };
				if (!string.IsNullOrEmpty(title))
					dialog.Title = title;

				if (!filter.IsEmpty)
					ApplyFilter(dialog, filter, filterName);

				var parent = GtkEssentialsContext.GetParentWindow()!;
				if (multiple)
					dialog.OpenMultiple(parent, null!, callback);
				else
					dialog.Open(parent, null!, callback);

				return dialog;
			},
			(dialog, result) =>
			{
				try
				{
					var files = new List<FileResult>();
					if (multiple)
					{
						var list = dialog.OpenMultipleFinish(result);
						for (uint i = 0; i < list.NItems; i++)
						{
							var path = GLib.FileAdapter.GetObject(list.GetObject(i)).Path;
							if (path is not null)
								files.Add(new GtkFileResult(path));
						}
					}
					else
					{
						var path = dialog.OpenFinish(result).Path;
						if (path is not null)
							files.Add(new GtkFileResult(path));
					}

					return files;
				}
				catch (GLib.GException ex) when (GtkAsync.IsDismissed(ex))
				{
					return null;
				}
			});

	static void ApplyFilter(global::Gtk.FileDialog dialog, FileFilterSpec spec, string? name)
	{
		var filter = new global::Gtk.FileFilter();
		if (!string.IsNullOrEmpty(name))
			filter.Name = name;

		if (spec.MimeTypes.Count > 0)
			filter.AddMimeTypes(spec.MimeTypes.ToArray());
		foreach (var suffix in spec.Suffixes)
			filter.AddSuffix(suffix);
		foreach (var pattern in spec.Patterns)
			filter.AddPattern(pattern);

		var filters = new GLib.ListStore(global::Gtk.FileFilter.GType);
		filters.Append(filter.Handle);
		dialog.Filters = (GLib.IListModel)filters;
		dialog.DefaultFilter = filter;
	}
}

/// <summary>
/// <see cref="IMediaPicker"/> for Linux: gallery picking through <c>Gtk.FileDialog</c> with image/video filters.
/// Camera capture is not supported (there is no portal-independent camera capture UI on Linux desktops).
/// </summary>
sealed class GtkMediaPicker : IMediaPicker
{
	static readonly FileFilterSpec Photos = FileFilterSpec.Parse(new[] { "image/*" });
	static readonly FileFilterSpec Videos = FileFilterSpec.Parse(new[] { "video/*" });

	public bool IsCaptureSupported => false;

	public Task<FileResult?> PickPhotoAsync(MediaPickerOptions? options = null) =>
		PickSingleAsync(options, Photos, "Photos");

	public Task<FileResult?> PickVideoAsync(MediaPickerOptions? options = null) =>
		PickSingleAsync(options, Videos, "Videos");

	public Task<List<FileResult>> PickPhotosAsync(MediaPickerOptions? options = null) =>
		PickManyAsync(options, Photos, "Photos");

	public Task<List<FileResult>> PickVideosAsync(MediaPickerOptions? options = null) =>
		PickManyAsync(options, Videos, "Videos");

	public Task<FileResult?> CapturePhotoAsync(MediaPickerOptions? options = null) =>
		throw new FeatureNotSupportedException("Photo capture is not supported on Linux.");

	public Task<FileResult?> CaptureVideoAsync(MediaPickerOptions? options = null) =>
		throw new FeatureNotSupportedException("Video capture is not supported on Linux.");

	static async Task<FileResult?> PickSingleAsync(MediaPickerOptions? options, FileFilterSpec filter, string filterName) =>
		(await GtkFilePicker.PickCoreAsync(options?.Title, filter, filterName, multiple: false).ConfigureAwait(false))?.FirstOrDefault();

	/// <summary>Multiple selection unless <see cref="MediaPickerOptions.SelectionLimit"/> is 1; the result is truncated to the limit (0 = unlimited).</summary>
	static async Task<List<FileResult>> PickManyAsync(MediaPickerOptions? options, FileFilterSpec filter, string filterName)
	{
		var limit = options?.SelectionLimit ?? 1;
		var files = await GtkFilePicker.PickCoreAsync(options?.Title, filter, filterName, multiple: limit != 1).ConfigureAwait(false)
			?? new List<FileResult>();

		return limit > 0 && files.Count > limit ? files.Take(limit).ToList() : files;
	}
}
