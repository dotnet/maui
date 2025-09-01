using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

record ProjectItem(AdditionalText AdditionalText, AnalyzerConfigOptions Options)
{
	public string Configuration
		=> Options.TryGetValue("build_metadata.additionalfiles.Configuration", out var configuration)
				? configuration
				: "Debug";

	public bool EnableLineInfo
	{
		get
		{
			var enableLineInfo = true;
			if (Options.TryGetValue("build_property.MauiXamlLineInfo", out var lineInfo) && string.Compare(lineInfo, "disable", StringComparison.OrdinalIgnoreCase) == 0)
				enableLineInfo = false;
			if (Options.TryGetValue("build_metadata.additionalfiles.LineInfo", out lineInfo) && string.Compare(lineInfo, "enable", StringComparison.OrdinalIgnoreCase) == 0)
				enableLineInfo = true;
			if (Options.TryGetValue("build_metadata.additionalfiles.LineInfo", out lineInfo) && string.Compare(lineInfo, "disable", StringComparison.OrdinalIgnoreCase) == 0)
				enableLineInfo = false;
			return enableLineInfo;
		}
	}
	public bool EnableDiagnostics
	{
		get
		{
			bool enableDiagnostics = false;
			if (Options.TryGetValue("build_property.EnableMauiXamlDiagnostics", out var enDiag) && string.Compare(enDiag, "true", StringComparison.OrdinalIgnoreCase) == 0)
				enableDiagnostics = true;
			if (Options.TryGetValue("build_metadata.additionalfiles.EnableDiagnostics", out enDiag) && string.Compare(enDiag, "true", StringComparison.OrdinalIgnoreCase) == 0)
				enableDiagnostics = true;
			if (Options.TryGetValue("build_metadata.additionalfiles.EnableDiagnostics", out enDiag) && string.Compare(enDiag, "false", StringComparison.OrdinalIgnoreCase) == 0)
				enableDiagnostics = false;
			return enableDiagnostics;
		}
	}

	public string Kind
		=> Options.TryGetValue("build_metadata.additionalfiles.GenKind", out var kind)
				? kind
				: "None";

	public XamlInflator Inflator
	{
		get
		{
			var xamlinflator = 0;
			if (Options.TryGetValue("build_metadata.additionalfiles.Inflator", out var inflator) && !string.IsNullOrEmpty(inflator))
			{
				var parts = inflator!.Split(',');
				for (int i = 0; i < parts.Length; i++)
				{
					var trimmed = parts[i].Trim();
					if (Enum.TryParse<XamlInflator>(trimmed, true, out var xinfl))
						xamlinflator |= (int)xinfl;
				}
			}
			return (XamlInflator)xamlinflator;
		}
	}

	public string? ManifestResourceName
		=> Options.TryGetValue("build_metadata.additionalfiles.ManifestResourceName", out var manifestResourceName)
				? manifestResourceName
				: null;

	public string NoWarn
	{
		get
		{
			string noWarn = "";
			if (Options.TryGetValue("build_property.MauiXamlNoWarn", out var noWarnValue))
				noWarn = noWarnValue;
			if (Options.TryGetValue("build_metadata.additionalfiles.NoWarn", out noWarnValue))
				noWarn = noWarnValue;
			return noWarn;
		}
	}

	public string? RelativePath
		=> Options.TryGetValue("build_metadata.additionalfiles.RelativePath", out var relativePath)
				? relativePath
				: null;

	public string? TargetFramework
		=> Options.TryGetValue("build_metadata.additionalfiles.targetFramework", out var targetFramework)
				? targetFramework
				: null;

	public string? TargetPath
		=> Options.TryGetValue("build_metadata.additionalfiles.TargetPath", out var targetPath) && !string.IsNullOrEmpty(targetPath)
				? targetPath
				: AdditionalText.Path;	
}
