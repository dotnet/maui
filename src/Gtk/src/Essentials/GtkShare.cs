using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>What <see cref="GtkShare"/> needs from the desktop; a seam so tests do not open real applications.</summary>
interface IShareBackend
{
	Task SetClipboardTextAsync(string text);

	Task<bool> OpenUriAsync(string uri);

	Task<bool> OpenFileAsync(string path);

	Task<bool> OpenContainingFolderAsync(string path);
}

/// <summary>Production backend: <see cref="GtkClipboard"/> and the <c>Gtk.UriLauncher</c>/<c>Gtk.FileLauncher</c> portal calls.</summary>
sealed class GtkShareBackend : IShareBackend
{
	readonly GtkClipboard clipboard = new();

	public Task SetClipboardTextAsync(string text) => clipboard.SetTextAsync(text);

	public Task<bool> OpenUriAsync(string uri) => GtkLauncher.LaunchUriAsync(uri);

	public Task<bool> OpenFileAsync(string path) => GtkLauncher.LaunchFileAsync(path);

	public Task<bool> OpenContainingFolderAsync(string path) => GtkLauncher.OpenContainingFolderAsync(path);
}

/// <summary>
/// <see cref="IShare"/> for Linux, which has no share sheet: a desktop approximation. Text goes to the clipboard, a
/// <see cref="ShareTextRequest.Uri"/> is opened with its default handler, files are opened with their default application
/// (OpenURI portal) and, when no application handles a file, its containing folder is shown instead.
/// </summary>
sealed class GtkShare : IShare
{
	readonly IShareBackend backend;

	public GtkShare() : this(new GtkShareBackend())
	{
	}

	internal GtkShare(IShareBackend backend) =>
		this.backend = backend ?? throw new ArgumentNullException(nameof(backend));

	public async Task RequestAsync(ShareTextRequest request)
	{
		if (request is null)
			throw new ArgumentNullException(nameof(request));

		var hasText = !string.IsNullOrEmpty(request.Text);
		var hasUri = !string.IsNullOrEmpty(request.Uri);
		if (!hasText && !hasUri)
			throw new ArgumentException("A share request needs a text or a URI.", nameof(request));

		if (hasText)
			await backend.SetClipboardTextAsync(request.Text!).ConfigureAwait(false);

		if (hasUri && !await backend.OpenUriAsync(request.Uri!).ConfigureAwait(false))
			throw new InvalidOperationException($"No application could open '{request.Uri}'.");
	}

	public Task RequestAsync(ShareFileRequest request)
	{
		if (request is null)
			throw new ArgumentNullException(nameof(request));
		if (request.File is null)
			throw new ArgumentException("A share request needs a file.", nameof(request));

		return OpenAsync(new[] { request.File });
	}

	public Task RequestAsync(ShareMultipleFilesRequest request)
	{
		if (request is null)
			throw new ArgumentNullException(nameof(request));

		var files = request.Files?.Where(f => f is not null).ToList();
		if (files is null || files.Count == 0)
			throw new ArgumentException("A share request needs at least one file.", nameof(request));

		return OpenAsync(files);
	}

	async Task OpenAsync(IReadOnlyList<ShareFile> files)
	{
		// Validate everything first so a missing file does not leave a half-shared set.
		var paths = new List<string>(files.Count);
		foreach (var file in files)
		{
			var path = file.FullPath;
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
				throw new FileNotFoundException("The file to share does not exist.", path);
			paths.Add(path);
		}

		List<Exception>? failures = null;
		foreach (var path in paths)
		{
			try
			{
				await OpenOneAsync(path).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				(failures ??= new()).Add(ex);
			}
		}

		if (failures is { Count: 1 })
			ExceptionDispatchInfo.Capture(failures[0]).Throw();
		else if (failures is not null)
			throw new AggregateException(failures);
	}

	async Task OpenOneAsync(string path)
	{
		bool opened;
		try
		{
			opened = await backend.OpenFileAsync(path).ConfigureAwait(false);
		}
		catch (Exception)
		{
			opened = false;
		}

		if (opened)
			return;

		// No application handles the file: reveal it so the user can still act on it.
		if (!await backend.OpenContainingFolderAsync(path).ConfigureAwait(false))
			throw new InvalidOperationException($"Neither an application nor the file manager could open '{path}'.");
	}
}
