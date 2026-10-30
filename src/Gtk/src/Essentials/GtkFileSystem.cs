using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="IFileSystem"/> for Linux: app data in <c>$XDG_DATA_HOME/&lt;app&gt;</c>, cache in <c>$XDG_CACHE_HOME/&lt;app&gt;</c>.
/// App package files are read from <see cref="AppContext.BaseDirectory"/>.
/// </summary>
sealed class GtkFileSystem : IFileSystem
{
	readonly string appDataDirectory;
	readonly string cacheDirectory;
	readonly string packageDirectory;

	internal GtkFileSystem(string applicationId, XdgPaths? xdg = null, string? packageDirectory = null)
	{
		xdg ??= XdgPaths.Default;
		var dir = AppIdentity.ToDirectoryName(applicationId);
		appDataDirectory = Path.Combine(xdg.DataHome, dir);
		cacheDirectory = Path.Combine(xdg.CacheHome, dir);
		this.packageDirectory = packageDirectory ?? AppContext.BaseDirectory;
	}

	public string AppDataDirectory => Ensure(appDataDirectory);

	public string CacheDirectory => Ensure(cacheDirectory);

	public Task<Stream> OpenAppPackageFileAsync(string filename)
	{
		var path = ResolvePackagePath(filename);
		return Task.FromResult<Stream>(File.OpenRead(path));
	}

	public Task<bool> AppPackageFileExistsAsync(string filename)
	{
		try
		{
			return Task.FromResult(File.Exists(ResolvePackagePath(filename)));
		}
		catch (ArgumentException)
		{
			return Task.FromResult(false);
		}
	}

	string ResolvePackagePath(string filename)
	{
		if (string.IsNullOrWhiteSpace(filename))
			throw new ArgumentException("The file name cannot be empty.", nameof(filename));

		var relative = filename.Replace('\\', '/').TrimStart('/');
		return Path.Combine(packageDirectory, relative.Replace('/', Path.DirectorySeparatorChar));
	}

	static string Ensure(string path)
	{
		Directory.CreateDirectory(path);
		return path;
	}
}
