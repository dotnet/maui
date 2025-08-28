using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

record ProjectItem
{
	public required AdditionalText AdditionalText { get; init; }
	private string? _targetPath;
	public string? TargetPath { get => _targetPath ?? AdditionalText.Path; init => _targetPath = value; }
	public string? RelativePath { get; init; }
	public string? ManifestResourceName { get; init; }
	public required string Kind { get; init; }
	public string? TargetFramework { get; init; }
	public required XamlInflator Inflator { get; init; } 

	//bypass attribute check. used for testing
	public string Configuration { get; internal set; } = "Debug";
	public bool EnableLineInfo { get; internal set; } = true;
	public bool EnableDiagnostics { get; internal set; } = false;
}
