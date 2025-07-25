using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

class ProjectItem
{
	public ProjectItem(AdditionalText additionalText, string? targetPath, string? relativePath, string? manifestResourceName, string kind, string? targetFramework)
	{
		AdditionalText = additionalText;
		TargetPath = targetPath ?? additionalText.Path;
		RelativePath = relativePath;
		ManifestResourceName = manifestResourceName;
		Kind = kind;
		TargetFramework = targetFramework;
	}

	public AdditionalText AdditionalText { get; }
	public string? TargetPath { get; }
	public string? RelativePath { get; }
	public string? ManifestResourceName { get; }
	public string Kind { get; }
	public string? TargetFramework { get; }

	//bypass attribute check. used for testing
	public bool ForceSourceGen { get; internal set; }
	public string Configuration { get; internal set; } = "Debug";
}
