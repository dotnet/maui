using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

record ProjectItem(AdditionalText AdditionalText, AnalyzerConfigOptions Options)
{
	private readonly AdditionalText _additionalText = AdditionalText;

	public string Configuration
		=> Options.GetValueOrDefault("build_property.Configuration", "Debug");

	public bool EnableLineInfo
	{
		get
		{
			if (Options.IsEnabled("build_metadata.additionalfiles.LineInfo"))
				return true;
			if (Options.IsDisabled("build_metadata.additionalfiles.LineInfo"))
				return false;
			return !Options.IsDisabled("build_property.MauiXamlLineInfo");
		}
	}

	public bool EnableDiagnostics
	{
		get
		{
			if (Options.IsTrue("build_metadata.additionalfiles.EnableDiagnostics"))
				return true;
			if (Options.IsFalse("build_metadata.additionalfiles.EnableDiagnostics"))
				return false;
			if (Options.IsTrue("build_property.EnableMauiXamlDiagnostics"))
				return true;
			if (Options.IsFalse("build_property.EnableMauiXamlDiagnostics"))
				return false;
			return !Configuration.Equals("Release", StringComparison.OrdinalIgnoreCase);
		}
	}

	public string Kind
		=> Options.GetValueOrDefault("build_metadata.additionalfiles.GenKind", "None");

	public XamlInflator Inflator
	{
		get
		{
			var xamlinflator = 0;
			var parts = Options.GetValueOrDefault("build_metadata.additionalfiles.Inflator", "").Split(',');
			for (int i = 0; i < parts.Length; i++)
			{
				var trimmed = parts[i].Trim();
				if (Enum.TryParse<XamlInflator>(trimmed, true, out var xinfl))
					xamlinflator |= (int)xinfl;
			}
			return (XamlInflator)xamlinflator;
		}
	}

	public string? ManifestResourceName
		=> Options.GetValueOrNull("build_metadata.additionalfiles.ManifestResourceName");

	public string NoWarn
		=> Options.GetValueOrNull("build_metadata.additionalfiles.NoWarn") ?? Options.GetValueOrNull("build_property.MauiXamlNoWarn") ?? "";

	public string? RelativePath
		=> Options.GetValueOrNull("build_metadata.additionalfiles.RelativePath");

	public string? TargetFramework
		=> Options.GetValueOrNull("build_property.targetFramework");

	public string? TargetPath
		=> Options.GetValueOrDefault("build_metadata.additionalfiles.TargetPath", _additionalText.Path);
}
