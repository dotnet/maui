namespace Microsoft.Maui.Controls.HybridWebApps.UI;

/// <summary>Validates frontend-owned locations inside the trusted asset bundle, not an application route catalog.</summary>
public static class BundledLocation
{
	/// <summary>Validates an asset-relative entry location and optional query/fragment.</summary>
	/// <param name="location">Bundle-relative location such as index.html#/any-frontend-route.</param>
	/// <returns>Normalized relative URL, never a remote/special-scheme destination.</returns>
	public static string Validate(string location)
	{
		if (location.Length is < 1 or > 4096 || location.Any(char.IsControl) || location.Contains('\\', StringComparison.Ordinal) ||
			location.StartsWith("//", StringComparison.Ordinal) || Uri.TryCreate(location, UriKind.Absolute, out _))
			throw new CommandException("invalid_location", "Use an asset-relative bundled location, never a remote URL, scheme or UNC path.");
		var relative = location.TrimStart('/');
		if (!Uri.TryCreate(new Uri("https://bundle.invalid/"), relative, out var uri) || uri.Host != "bundle.invalid")
			throw new CommandException("invalid_location", "The location must stay inside the trusted asset bundle.");
		var rawPath = relative.Split('?', '#')[0];
		var path = Uri.UnescapeDataString(rawPath);
		if (path.Length == 0 || path.Contains('%', StringComparison.Ordinal) || path.Contains('\\', StringComparison.Ordinal) ||
			path.Split('/').Any(segment => segment is "." or ".." || segment.Length == 0))
			throw new CommandException("invalid_location", "Bundle paths must not contain traversal, empty segments or double encoding.");
		return relative;
	}
}
