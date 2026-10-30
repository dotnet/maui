using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Captures what native code writes to file descriptor 2 for a scope. GTK 4 reports criticals, warnings and CSS parser errors
/// through GLib's structured logging, which bypasses <c>g_log_set_handler</c>, so the only reliable observer is the stream itself.
/// </summary>
sealed class StderrCapture : IDisposable
{
	const int StdErr = 2;

	readonly string _path;
	readonly int _saved;
	bool _active;

	StderrCapture(string path, int saved)
	{
		_path = path;
		_saved = saved;
		_active = true;
	}

	public static StderrCapture? Start()
	{
		try
		{
			var path = Path.Combine(Path.GetTempPath(), $"maui-gtk-stderr-{Guid.NewGuid():N}.txt");
			int saved = Dup(StdErr);
			if (saved < 0)
				return null;

			int file = Open(path);
			if (file < 0)
			{
				Close(saved);
				return null;
			}

			Dup2(file, StdErr);
			Close(file);
			return new StderrCapture(path, saved);
		}
		catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
		{
			return null;
		}
	}

	long _offset;

	/// <summary>Returns the GTK/GLib diagnostics (warnings, criticals, errors) written since the last call, without stopping.</summary>
	public List<string> Read()
	{
		var lines = new List<string>();

		try
		{
			using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			stream.Seek(_offset, SeekOrigin.Begin);
			using var reader = new StreamReader(stream);
			string? line;
			while ((line = reader.ReadLine()) is not null)
			{
				if (line.Contains("-WARNING **", StringComparison.Ordinal)
					|| line.Contains("-CRITICAL **", StringComparison.Ordinal)
					|| line.Contains("-ERROR **", StringComparison.Ordinal))
					lines.Add(line.Trim());
			}

			_offset = stream.Length;
		}
		catch (IOException)
		{
		}

		return lines;
	}

	/// <summary>Stops capturing and returns the diagnostics written since the last <see cref="Read"/>.</summary>
	public List<string> Stop()
	{
		if (!_active)
			return new List<string>();

		var lines = Read();
		_active = false;
		Dup2(_saved, StdErr);
		Close(_saved);

		try
		{
			File.Delete(_path);
		}
		catch (IOException)
		{
		}

		return lines;
	}

	public void Dispose() => Stop();

	// ---- C runtime plumbing (UCRT on Windows, libc elsewhere) ----

	static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	static int Dup(int fd) => IsWindows ? WinDup(fd) : PosixDup(fd);

	static int Dup2(int source, int target) => IsWindows ? WinDup2(source, target) : PosixDup2(source, target);

	static int Close(int fd) => IsWindows ? WinClose(fd) : PosixClose(fd);

	static int Open(string path) =>
		IsWindows
			? WinOpen(path, 0x0001 | 0x0100 | 0x0200 | 0x8000, 0x0180)
			: PosixOpen(path, 1 | 64 | 512, 0x180);

	[DllImport("ucrtbase.dll", EntryPoint = "_dup")]
	static extern int WinDup(int fd);

	[DllImport("ucrtbase.dll", EntryPoint = "_dup2")]
	static extern int WinDup2(int source, int target);

	[DllImport("ucrtbase.dll", EntryPoint = "_close")]
	static extern int WinClose(int fd);

	[DllImport("ucrtbase.dll", EntryPoint = "_wopen", CharSet = CharSet.Unicode)]
	static extern int WinOpen(string path, int flags, int mode);

	[DllImport("libc", EntryPoint = "dup")]
	static extern int PosixDup(int fd);

	[DllImport("libc", EntryPoint = "dup2")]
	static extern int PosixDup2(int source, int target);

	[DllImport("libc", EntryPoint = "close")]
	static extern int PosixClose(int fd);

	[DllImport("libc", EntryPoint = "open")]
	static extern int PosixOpen([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags, int mode);
}
