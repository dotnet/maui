using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>Application id, display name and version derived from an assembly.</summary>
sealed class AppIdentity
{
	AppIdentity(string applicationId, string name, string versionString, Version version, string buildString)
	{
		ApplicationId = applicationId;
		Name = name;
		VersionString = versionString;
		Version = version;
		BuildString = buildString;
	}

	internal string ApplicationId { get; }

	internal string Name { get; }

	internal string VersionString { get; }

	internal Version Version { get; }

	internal string BuildString { get; }

	/// <summary>
	/// Resolves the identity. Id: option, then <c>ApplicationId</c> assembly metadata, then the assembly name.
	/// Name: option, then <c>ApplicationTitle</c> metadata (the project's <c>ApplicationTitle</c>), <c>AssemblyTitle</c>, <c>AssemblyProduct</c>, the assembly name.
	/// Version: <c>ApplicationDisplayVersion</c> metadata, then the informational version (without <c>+build</c>), then the assembly version.
	/// Build: <c>ApplicationVersion</c> metadata, then the assembly version build number, else <c>1</c>.
	/// </summary>
	internal static AppIdentity Resolve(Assembly? assembly, string? idOverride = null, string? nameOverride = null)
	{
		var asmName = assembly?.GetName();
		var simpleName = asmName?.Name ?? "app";

		var id = FirstNonEmpty(idOverride, Metadata(assembly, "ApplicationId"), simpleName)!;
		var name = FirstNonEmpty(
			nameOverride,
			Metadata(assembly, "ApplicationTitle"),
			assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title,
			assembly?.GetCustomAttribute<AssemblyProductAttribute>()?.Product,
			simpleName)!;

		var informational = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
		if (informational is not null)
		{
			var plus = informational.IndexOf('+', StringComparison.Ordinal);
			if (plus >= 0)
				informational = informational.Substring(0, plus);
		}

		var versionString = FirstNonEmpty(Metadata(assembly, "ApplicationDisplayVersion"), informational, asmName?.Version?.ToString(3), "1.0")!;
		var version = ParseVersion(versionString) ?? asmName?.Version ?? new Version(1, 0);

		var build = FirstNonEmpty(
			Metadata(assembly, "ApplicationVersion"),
			asmName?.Version is { Build: >= 0 } v ? v.Build.ToString() : null,
			"1")!;

		return new AppIdentity(id, name, versionString, version, build);
	}

	/// <summary>Parses the leading numeric part of a version string (<c>1.2.3-beta</c> becomes 1.2.3).</summary>
	internal static Version? ParseVersion(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return null;

		var match = Regex.Match(text, @"^\s*(\d+)(\.\d+){0,3}");
		if (!match.Success)
			return null;

		var normalized = match.Value.Trim();
		if (!normalized.Contains('.', StringComparison.Ordinal))
			normalized += ".0";

		return Version.TryParse(normalized, out var parsed) ? parsed : null;
	}

	/// <summary>Replaces characters that are unsafe in a directory name.</summary>
	internal static string ToDirectoryName(string id)
	{
		var invalid = System.IO.Path.GetInvalidFileNameChars();
		var cleaned = new string(id.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
		return cleaned.Length == 0 || cleaned == "." || cleaned == ".." ? "app" : cleaned;
	}

	static string? Metadata(Assembly? assembly, string key) =>
		assembly?.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == key)?.Value;

	static string? FirstNonEmpty(params string?[] values)
	{
		foreach (var v in values)
		{
			if (!string.IsNullOrWhiteSpace(v))
				return v;
		}

		return null;
	}
}
