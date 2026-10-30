using System;
using System.IO;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>Resolves the XDG base directories (<c>$XDG_DATA_HOME</c>, <c>$XDG_CONFIG_HOME</c>, <c>$XDG_CACHE_HOME</c>).</summary>
/// <remarks>Per the XDG spec a relative or empty value is ignored and the default under the home directory is used.</remarks>
sealed class XdgPaths
{
	readonly Func<string, string?> getEnv;
	readonly string home;

	internal XdgPaths(Func<string, string?>? getEnv = null, string? home = null)
	{
		this.getEnv = getEnv ?? Environment.GetEnvironmentVariable;
		this.home = home
			?? this.getEnv("HOME")
			?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	}

	internal static XdgPaths Default { get; } = new XdgPaths();

	internal string DataHome => Resolve("XDG_DATA_HOME", ".local/share");

	internal string ConfigHome => Resolve("XDG_CONFIG_HOME", ".config");

	internal string CacheHome => Resolve("XDG_CACHE_HOME", ".cache");

	string Resolve(string variable, string fallback)
	{
		var value = getEnv(variable);
		if (!string.IsNullOrWhiteSpace(value) && Path.IsPathRooted(value))
			return value!;

		return Path.Combine(home, fallback.Replace('/', Path.DirectorySeparatorChar));
	}
}
