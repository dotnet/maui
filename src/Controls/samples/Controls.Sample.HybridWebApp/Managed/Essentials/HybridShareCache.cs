namespace Maui.HybridApp.Essentials;

// Retained OS files outlive document handles. Never remove a young shared file on chooser completion.
internal sealed class HybridShareCache
{
	internal const int MaximumFileBytes = 4 * 1024 * 1024;
	internal const int MaximumFiles = 32;
	internal const long MaximumTotalBytes = 64L * 1024 * 1024;
	internal static readonly TimeSpan Retention = TimeSpan.FromHours(24);
	readonly string _root;
	readonly TimeProvider _time;
	readonly object _gate = new();
	readonly HashSet<string> _active = new(StringComparer.Ordinal);
	readonly Dictionary<string, DateTimeOffset> _retainedUntil = new(StringComparer.Ordinal);

	internal HybridShareCache(string cacheRoot, TimeProvider time)
	{
		_root = Path.Combine(cacheRoot, "hybrid-share-v1");
		_time = time;
	}

	internal string Stage(string name, byte[] bytes)
	{
		if (name.Length is < 1 or > 100 || name is "." or ".." || name[0] == '.' ||
			name.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '.' and not '-' and not '_') ||
			bytes.Length is < 1 or > MaximumFileBytes)
			throw new CommandException("invalid_arguments", "Use a bounded nonempty file and a plain ASCII file name, not a path.");
		lock (_gate)
		{
			Directory.CreateDirectory(_root);
			PruneLocked();
			var files = Files();
			if (files.Length >= MaximumFiles || files.Sum(f => f.Length) + bytes.LongLength > MaximumTotalBytes)
				throw new CommandException("resource_limit", "The retained native share cache is full. Retry after retained files expire.");
			var directory = Path.Combine(_root, Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(directory);
			var path = Path.Combine(directory, name);
			try
			{
				File.WriteAllBytes(path, bytes);
				File.SetLastWriteTimeUtc(path, _time.GetUtcNow().UtcDateTime);
				return path;
			}
			catch { Directory.Delete(directory, true); throw; }
		}
	}

	internal void BeginShare(IEnumerable<string> paths)
	{
		lock (_gate)
			foreach (var path in paths)
			{
				File.SetLastWriteTimeUtc(path, _time.GetUtcNow().UtcDateTime);
				_active.Add(path);
				_retainedUntil[path] = _time.GetUtcNow() + Retention;
			}
	}
	internal void EndShare(IEnumerable<string> paths)
	{
		lock (_gate)
			foreach (var path in paths)
			{
				_active.Remove(path);
				if (!File.Exists(path)) { _retainedUntil.Remove(path); continue; }
				_retainedUntil[path] = _time.GetUtcNow() + Retention;
				try { if (File.Exists(path)) File.SetLastWriteTimeUtc(path, _time.GetUtcNow().UtcDateTime); }
				catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
			}
	}
	internal void RemoveUnshared(string path)
	{
		lock (_gate) Remove(path);
	}
	internal void Prune()
	{
		lock (_gate) { if (Directory.Exists(_root)) PruneLocked(); }
	}
	FileInfo[] Files() => Directory.GetFiles(_root, "*", SearchOption.AllDirectories).Select(path => new FileInfo(path)).ToArray();
	void PruneLocked()
	{
		foreach (var file in Files())
			if (!_active.Contains(file.FullName) && (!_retainedUntil.TryGetValue(file.FullName, out var until) || until <= _time.GetUtcNow()) &&
				_time.GetUtcNow().UtcDateTime - file.LastWriteTimeUtc >= Retention)
				try { Remove(file.FullName); }
				catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
	}
	void Remove(string path)
	{
		File.Delete(path);
		_retainedUntil.Remove(path);
		var parent = Path.GetDirectoryName(path)!;
		if (Directory.Exists(parent) && !Directory.EnumerateFileSystemEntries(parent).Any()) Directory.Delete(parent);
	}
}
