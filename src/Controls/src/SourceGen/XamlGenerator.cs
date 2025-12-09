using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

using static GeneratorHelpers;
using static LocationHelpers;

[Generator(LanguageNames.CSharp)]
public class XamlGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext initContext)
	{
#if DEBUG
		// if (!System.Diagnostics.Debugger.IsAttached)
		// {
		// 	System.Diagnostics.Debugger.Launch();
		// }
#endif
		// Only provide a new Compilation when the references or syntax trees change
		var referenceCompilationProvider = initContext.CompilationProvider
			.WithComparer(new CompilationSignaturesComparer())
			.WithTrackingName(TrackingNames.CompilationProvider);

		var referenceTypeCacheProvider = referenceCompilationProvider
			.Select(GetTypeCache)
			.WithTrackingName(TrackingNames.ReferenceTypeCacheProvider);

		var xmlnsDefinitionsProvider = referenceCompilationProvider
			.Select(GetAssemblyAttributes)
			.WithTrackingName(TrackingNames.XmlnsDefinitionsProvider);

		var projectItemProvider = initContext.AdditionalTextsProvider
			.Combine(initContext.AnalyzerConfigOptionsProvider)
			.Select(ComputeProjectItem)
			.WithTrackingName(TrackingNames.ProjectItemProvider);

		var xamlProjectItemProviderForCB = projectItemProvider
			.Where(static p => p?.Kind == "Xaml")
			.Combine(xmlnsDefinitionsProvider)
			.Select(ComputeXamlProjectItemForCB)
			.WithTrackingName(TrackingNames.XamlProjectItemProviderForCB);

		var xamlProjectItemProviderForIC = projectItemProvider
			.Where(static p => p?.Kind == "Xaml")
			.Combine(xmlnsDefinitionsProvider)
			.Select(ComputeXamlProjectItemForIC)
			.WithTrackingName(TrackingNames.XamlProjectItemProviderForIC);

		var cssProjectItemProvider = projectItemProvider
			.Where(static p => p?.Kind == "Css")
			.WithTrackingName(TrackingNames.CssProjectItemProvider);

		var xamlSourceProviderForCB = xamlProjectItemProviderForCB
			.Combine(xmlnsDefinitionsProvider, referenceTypeCacheProvider, initContext.CompilationProvider)
			.Select(GetSource)
			.WithTrackingName(TrackingNames.XamlSourceProviderForCB);

		var compilationWithCodeBehindProvider = xamlSourceProviderForCB
			.Collect()
			.Combine(initContext.CompilationProvider)
			.Select(static (t, ct) =>
			{
				var compilation = t.Right;
				var options = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions;
				foreach (var (source, xamlItem, diagnostics) in t.Left)
				{
					ct.ThrowIfCancellationRequested();

					if (source is not null)
					{
						var tree = CSharpSyntaxTree.ParseText(source, options: options, cancellationToken: ct);
						compilation = compilation.AddSyntaxTrees(tree);
					}
				}
				return compilation;
			})
			.WithTrackingName(TrackingNames.CompilationWithCodeBehindProvider);

		//this xmlnsDefinitionProvider is computed AFTER feeding the codebehind into the compilation, and allows correct assemblySymbol comparisons
		var xmlnsDefinitionsProviderForIC = compilationWithCodeBehindProvider
			.Select(GetAssemblyAttributes)
			.WithTrackingName(TrackingNames.XmlnsDefinitionsProviderForIC);

		var xamlSourceProviderForIC = xamlProjectItemProviderForIC
			.Combine(xmlnsDefinitionsProviderForIC, compilationWithCodeBehindProvider.Select(GetTypeCache), compilationWithCodeBehindProvider)
			.WithTrackingName(TrackingNames.XamlSourceProviderForIC);

		// Register the XAML pipeline for CodeBehind
		initContext.RegisterSourceOutput(xamlSourceProviderForCB, static (sourceProductionContext, provider) =>
		{
			var (source, xamlItem, diagnostics) = provider;

			try
			{
				if (diagnostics != null)
					foreach (var diag in diagnostics)
						sourceProductionContext.ReportDiagnostic(diag);
				if (source != null)
					sourceProductionContext.AddSource(GetHintName(xamlItem?.ProjectItem, "sg"), source);
			}
			catch (Exception e)
			{
				IXmlLineInfo lineInfo;
				string errorMessage;

				if (e is XamlParseException xpe)
				{
					lineInfo = xpe.XmlInfo;
					errorMessage = xpe.UnformattedMessage;
				}
				else if (e is XmlException xmlEx)
				{
					lineInfo = new XmlLineInfo(xmlEx.LineNumber, xmlEx.LinePosition);
					errorMessage = StripLineInfoFromXmlExceptionMessage(xmlEx.Message);
				}
				else
				{
					lineInfo = new XmlLineInfo();
					errorMessage = e.Message;
				}

				var location = xamlItem?.ProjectItem?.RelativePath is not null ? LocationCreate(xamlItem.ProjectItem.RelativePath, lineInfo, string.Empty) : null;
				sourceProductionContext.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, errorMessage));
			}
		});

		// Register the XAML pipeline for InitializeComponent
		initContext.RegisterImplementationSourceOutput(xamlSourceProviderForIC, static (sourceProductionContext, provider) =>
		{
			var (xamlItem, xmlnsCache, typeCache, compilation) = provider;

			if (xamlItem?.ProjectItem?.RelativePath is not string relativePath)
			{
				throw new InvalidOperationException("Xaml item or target path is null");
			}

			if (!ShouldGenerateSourceGenInitializeComponent(xamlItem, xmlnsCache, compilation))
				return;

			if (!CanSourceGenXaml(xamlItem, compilation, sourceProductionContext, xmlnsCache, typeCache))
			{
				if (xamlItem != null && xamlItem.Exception != null)
				{
					IXmlLineInfo lineInfo;
					string errorMessage;

					if (xamlItem.Exception is XamlParseException xpe)
					{
						lineInfo = xpe.XmlInfo;
						errorMessage = xpe.UnformattedMessage;
					}
					else if (xamlItem.Exception is XmlException xmlEx)
					{
						lineInfo = new XmlLineInfo(xmlEx.LineNumber, xmlEx.LinePosition);
						errorMessage = StripLineInfoFromXmlExceptionMessage(xmlEx.Message);
					}
					else if (xamlItem.Exception.InnerException is XmlException innerXmlEx)
					{
						lineInfo = new XmlLineInfo(innerXmlEx.LineNumber, innerXmlEx.LinePosition);
						errorMessage = StripLineInfoFromXmlExceptionMessage(innerXmlEx.Message);
					}
					else
					{
						// Try to extract line info from message if present
						lineInfo = ExtractLineInfoFromMessage(xamlItem.Exception.Message);
						errorMessage = StripLineInfoFromXmlExceptionMessage(xamlItem.Exception.Message);
					}

					var location = LocationCreate(relativePath, lineInfo, string.Empty);
					sourceProductionContext.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, errorMessage));
				}
				return;
			}

			try
			{
				if (!ShouldGenerateSourceGenInitializeComponent(xamlItem, xmlnsCache, compilation))
					return;

				var code = InitializeComponentCodeWriter.GenerateInitializeComponent(xamlItem, compilation, sourceProductionContext, xmlnsCache, typeCache);
				sourceProductionContext.AddSource(GetHintName(xamlItem.ProjectItem, "xsg"), code);
			}
			catch (Exception e)
			{
				IXmlLineInfo lineInfo;
				string errorMessage;

				if (e is XamlParseException xpe)
				{
					lineInfo = xpe.XmlInfo;
					errorMessage = xpe.UnformattedMessage;
				}
				else if (e is XmlException xmlEx)
				{
					lineInfo = new XmlLineInfo(xmlEx.LineNumber, xmlEx.LinePosition);
					errorMessage = StripLineInfoFromXmlExceptionMessage(xmlEx.Message);
				}
				else
				{
					lineInfo = new XmlLineInfo();
					errorMessage = e.Message;
				}

				var location = xamlItem?.ProjectItem?.RelativePath is not null ? LocationCreate(xamlItem.ProjectItem.RelativePath, lineInfo, string.Empty) : null;
				sourceProductionContext.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, errorMessage));
			}
		});

		// Register the CSS pipeline
		initContext.RegisterImplementationSourceOutput(cssProjectItemProvider, static (sourceProductionContext, cssItem) =>
		{
			if (cssItem == null)
				return;

			GenerateCssCodeBehind(cssItem, sourceProductionContext);
		});


		// This could have been in the template, but having it here ensure it's never removed
		initContext.RegisterPostInitializationOutput(static context =>
		{
			context.AddSource("GlobalXmlns.g.cs", SourceText.From(
$"""
{AutoGeneratedHeaderText}
#nullable enable

[assembly: global::Microsoft.Maui.Controls.XmlnsDefinition("{XamlParser.MauiGlobalUri}", "{XamlParser.MauiUri}")]
[assembly: global::Microsoft.Maui.Controls.XmlnsPrefix("{XamlParser.MauiGlobalUri}", "global")]

#if MauiAllowImplicitXmlnsDeclaration
[assembly: global::Microsoft.Maui.Controls.Xaml.Internals.AllowImplicitXmlnsDeclaration]
#endif
"""
			, Encoding.UTF8));
		});

		// Generate AppThemeBinding helper methods for .NET 10 compatibility
		initContext.RegisterPostInitializationOutput(static context =>
		{
			context.AddSource("AppThemeBindingHelpers.g.cs", SourceText.From(
$$"""
{{AutoGeneratedHeaderText}}
#nullable enable

namespace Microsoft.Maui.Controls.XamlSourceGen;

internal static class AppThemeBindingHelpers
{
#if NET11_0_OR_GREATER
	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingLight(object? light)
	{
		return new global::Microsoft.Maui.Controls.AppThemeBinding
		{
			Light = light,
		};
	}

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingDark(object? dark)
	{
		return new global::Microsoft.Maui.Controls.AppThemeBinding
		{
			Dark = dark,
		};
	}

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingDefault(object? defaultValue)
	{
		return new global::Microsoft.Maui.Controls.AppThemeBinding
		{
			Default = defaultValue,
		};
	}

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingLightDark(object? light, object? dark)
	{
		return new global::Microsoft.Maui.Controls.AppThemeBinding
		{
			Light = light,
			Dark = dark,
		};
	}

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingLightDefault(object? light, object? defaultValue)
	{
		return new global::Microsoft.Maui.Controls.AppThemeBinding
		{
			Light = light,
			Default = defaultValue,
		};
	}

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingDarkDefault(object? dark, object? defaultValue)
	{
		return new global::Microsoft.Maui.Controls.AppThemeBinding
		{
			Dark = dark,
			Default = defaultValue,
		};
	}

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingLightDarkDefault(object? light, object? dark, object? defaultValue)
	{
		return new global::Microsoft.Maui.Controls.AppThemeBinding
		{
			Light = light,
			Dark = dark,
			Default = defaultValue,
		};
	}
#else
	// Shared UnsafeAccessor methods for accessing internal AppThemeBinding members
	[global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Constructor)]
	[return: global::System.Runtime.CompilerServices.UnsafeAccessorType("Microsoft.Maui.Controls.AppThemeBinding, Microsoft.Maui.Controls")]
	private static extern object AppThemeBindingCtor();

	[global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_Light")]
	private static extern void AppThemeBinding_SetLight(
		[global::System.Runtime.CompilerServices.UnsafeAccessorType("Microsoft.Maui.Controls.AppThemeBinding, Microsoft.Maui.Controls")]
		object instance,
		object? light);

	[global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_Dark")]
	private static extern void AppThemeBinding_SetDark(
		[global::System.Runtime.CompilerServices.UnsafeAccessorType("Microsoft.Maui.Controls.AppThemeBinding, Microsoft.Maui.Controls")]
		object instance,
		object? dark);

	[global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_Default")]
	private static extern void AppThemeBinding_SetDefault(
		[global::System.Runtime.CompilerServices.UnsafeAccessorType("Microsoft.Maui.Controls.AppThemeBinding, Microsoft.Maui.Controls")]
		object instance,
		object? defaultValue);

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingLight(object? light)
	{
		var binding = (global::Microsoft.Maui.Controls.BindingBase)AppThemeBindingCtor();
		AppThemeBinding_SetLight(binding, light);
		return binding;
	}

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingDark(object? dark)
	{
		var binding = (global::Microsoft.Maui.Controls.BindingBase)AppThemeBindingCtor();
		AppThemeBinding_SetDark(binding, dark);
		return binding;
	}

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingDefault(object? defaultValue)
	{
		var binding = (global::Microsoft.Maui.Controls.BindingBase)AppThemeBindingCtor();
		AppThemeBinding_SetDefault(binding, defaultValue);
		return binding;
	}

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingLightDark(object? light, object? dark)
	{
		var binding = (global::Microsoft.Maui.Controls.BindingBase)AppThemeBindingCtor();
		AppThemeBinding_SetLight(binding, light);
		AppThemeBinding_SetDark(binding, dark);
		return binding;
	}

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingLightDefault(object? light, object? defaultValue)
	{
		var binding = (global::Microsoft.Maui.Controls.BindingBase)AppThemeBindingCtor();
		AppThemeBinding_SetLight(binding, light);
		AppThemeBinding_SetDefault(binding, defaultValue);
		return binding;
	}

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingDarkDefault(object? dark, object? defaultValue)
	{
		var binding = (global::Microsoft.Maui.Controls.BindingBase)AppThemeBindingCtor();
		AppThemeBinding_SetDark(binding, dark);
		AppThemeBinding_SetDefault(binding, defaultValue);
		return binding;
	}

	[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static global::Microsoft.Maui.Controls.BindingBase CreateAppThemeBindingLightDarkDefault(object? light, object? dark, object? defaultValue)
	{
		var binding = (global::Microsoft.Maui.Controls.BindingBase)AppThemeBindingCtor();
		AppThemeBinding_SetLight(binding, light);
		AppThemeBinding_SetDark(binding, dark);
		AppThemeBinding_SetDefault(binding, defaultValue);
		return binding;
	}
#endif
}
"""
			, Encoding.UTF8));
		});

		// Register the global xmlns definitions. create equivalent XmlnsDefintion for the global ones, so most of the 1sr and 3rd party tooling should keep working
		initContext.RegisterSourceOutput(xmlnsDefinitionsProvider, static (sourceProductionContext, xmlnsCache) =>
		{
			var source = GenerateGlobalXmlns(sourceProductionContext, xmlnsCache);
			if (!string.IsNullOrEmpty(source))
				sourceProductionContext.AddSource("Global.Xmlns.cs", SourceText.From(source!, Encoding.UTF8));
		});
	}

	private static string GetHintName(ProjectItem? projectItem, string suffix)
	{
		if (projectItem?.RelativePath is not string relativePath)
		{
			throw new InvalidOperationException("Project item or target path is null");
		}

		var prefix = Path.GetDirectoryName(relativePath).Replace(Path.DirectorySeparatorChar, '_').Replace(':', '_');
		if (!string.IsNullOrEmpty(prefix))
			prefix += "_";
		var fileNameNoExtension = Path.GetFileNameWithoutExtension(relativePath);
		var kind = projectItem.Kind.ToLowerInvariant() ?? "unknown-kind";
		return $"{prefix}{fileNameNoExtension}.{kind}.{suffix}.cs";
	}

	private static string? GenerateGlobalXmlns(SourceProductionContext sourceProductionContext, AssemblyAttributes xmlnsCache)
	{
		if (xmlnsCache.GlobalGeneratedXmlnsDefinitions.Count == 0)
			return null;

		var sb = new StringBuilder();
		sb.AppendLine(AutoGeneratedHeaderText);
		sb.AppendLine("#nullable enable");
		foreach (var xmlns in xmlnsCache.GlobalGeneratedXmlnsDefinitions)
			sb.AppendLine($"[assembly: global::Microsoft.Maui.Controls.XmlnsDefinition(\"{xmlns.XmlNamespace}\", \"{xmlns.Target}\", AssemblyName = \"{EscapeIdentifier(xmlns.AssemblyName)}\")]");

		return sb.ToString();
	}

	static bool ShouldGenerateSourceGenInitializeComponent(XamlProjectItemForIC xamlItem, AssemblyAttributes xmlnsCache, Compilation compilation)
	{
		var text = xamlItem.ProjectItem.AdditionalText.GetText();
		if (text == null)
			return false;

		XmlNode? root;
		XmlNamespaceManager nsmgr;
		try
		{
			(root, nsmgr) = LoadXmlDocument(text, xmlnsCache, CancellationToken.None);
		}
		catch (Exception)
		{
			return false;
		}

		if (root == null)
			return false;

		var rootClass = root.Attributes["Class", XamlParser.X2006Uri]
					 ?? root.Attributes["Class", XamlParser.X2009Uri];
		INamedTypeSymbol? rootType;

		if (rootClass != null)
		{
			XmlnsHelper.ParseXmlns(rootClass.Value, out var rootTypeName, out var rootClrNamespace, out _, out _);
			rootType = compilation.GetTypeByMetadataName($"{rootClrNamespace}.{rootTypeName}");
		}
		else
		{ //no x:Class, but it can be an autogenerated type (starting with __Type, and with a XamlResourceId attribute)
			ITypeSymbol xamlResIdAttr = compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.XamlResourceIdAttribute")!;
			INamedTypeSymbol? GetTypeForResourcePath(string resourcePath, IAssemblySymbol assembly)
			{
				//XRID use paths with forward slashes. we can't change that it's used by HR
				var attr = assembly.GetAttributes(xamlResIdAttr).FirstOrDefault(attr => ((string)attr.ConstructorArguments[1].Value!).Replace('/', Path.DirectorySeparatorChar) == resourcePath);
				return attr?.ConstructorArguments[2].Value as INamedTypeSymbol;
			}

			rootType = GetTypeForResourcePath(xamlItem.ProjectItem.RelativePath!, compilation.Assembly);
		}

		if (rootType == null)
			return false;

		var xamlInflators = xamlItem.ProjectItem.Inflator;

		if ((xamlInflators & XamlInflator.SourceGen) != XamlInflator.SourceGen)
			return false;

		return true;
	}

	static bool CanSourceGenXaml(XamlProjectItemForIC? xamlItem, Compilation compilation, SourceProductionContext context, AssemblyAttributes xmlnsCache, IDictionary<XmlType, INamedTypeSymbol> typeCache)
	{
		ProjectItem? projItem;
		if (xamlItem == null || (projItem = xamlItem.ProjectItem) == null)
			return false;
		var itemName = projItem.ManifestResourceName ?? projItem.RelativePath;
		if (itemName == null)
			return false;
		if (xamlItem.Xaml == null)
			return false;
		return true;
	}

	static void GenerateCssCodeBehind(ProjectItem projItem, SourceProductionContext sourceProductionContext)
	{
		var sb = new StringBuilder();
		var hintName = $"{(string.IsNullOrEmpty(Path.GetDirectoryName(projItem.TargetPath)) ? "" : Path.GetDirectoryName(projItem.TargetPath) + Path.DirectorySeparatorChar)}{Path.GetFileNameWithoutExtension(projItem.TargetPath)}.{projItem.Kind.ToLowerInvariant()}.sg.cs".Replace(Path.DirectorySeparatorChar, '_');

		if (projItem.ManifestResourceName != null && projItem.TargetPath != null)
		{
			sb.AppendLine($"[assembly: global::Microsoft.Maui.Controls.Xaml.XamlResourceId(\"{projItem.ManifestResourceName}\", \"{projItem.TargetPath.Replace('\\', '/')}\", null)]");
		}

		sourceProductionContext.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
	}

	static void ApplyTransforms(XmlNode node, string? targetFramework, XmlNamespaceManager nsmgr)
	{
		SimplifyOnPlatform(node, targetFramework, nsmgr);
	}

	static void SimplifyOnPlatform(XmlNode node, string? targetFramework, XmlNamespaceManager nsmgr)
	{
		//remove OnPlatform nodes if the platform doesn't match, so we don't generate field for x:Name of elements being removed
		if (targetFramework == null)
		{
			return;
		}

		string? target = null;
		targetFramework = targetFramework.Trim();
		if (targetFramework.IndexOf("-android", StringComparison.OrdinalIgnoreCase) != -1)
		{
			target = "Android";
		}

		if (targetFramework.IndexOf("-ios", StringComparison.OrdinalIgnoreCase) != -1)
		{
			target = "iOS";
		}

		if (targetFramework.IndexOf("-macos", StringComparison.OrdinalIgnoreCase) != -1)
		{
			target = "macOS";
		}

		if (targetFramework.IndexOf("-maccatalyst", StringComparison.OrdinalIgnoreCase) != -1)
		{
			target = "MacCatalyst";
		}

		if (target == null)
		{
			return;
		}

		//no need to handle {OnPlatform} markup extension, as you can't x:Name there
		var onPlatformNodes = node.SelectNodes("//__f__:OnPlatform", nsmgr);
		foreach (XmlNode onPlatformNode in onPlatformNodes)
		{
			var onNodes = onPlatformNode.SelectNodes("__f__:On", nsmgr);
			foreach (XmlNode onNode in onNodes)
			{
				var platforms = onNode.SelectSingleNode("@Platform");
				var plats = platforms.Value.Split(',');
				var match = false;

				foreach (var plat in plats)
				{
					if (string.IsNullOrWhiteSpace(plat))
					{
						continue;
					}

					if (plat.Trim() == target)
					{
						match = true;
						break;
					}
				}
				if (!match)
				{
					onNode.ParentNode.RemoveChild(onNode);
				}
			}
		}
	}

	static string StripLineInfoFromXmlExceptionMessage(string message)
	{
		// XmlException messages typically end with " Line X, position Y."
		// We want to strip that since we're reporting location separately
		var lineIndex = message.LastIndexOf(" Line ");
		if (lineIndex > 0)
		{
			// Strip both the trailing period after the line info and any double periods
			return message.Substring(0, lineIndex).TrimEnd('.', ' ');
		}
		return message;
	}

	static IXmlLineInfo ExtractLineInfoFromMessage(string message)
	{
		// Try to extract "Line X, position Y" from the message
		var lineIndex = message.LastIndexOf(" Line ");
		if (lineIndex > 0)
		{
			try
			{
				var lineInfoPart = message.Substring(lineIndex + 6); // Skip " Line "
				var parts = lineInfoPart.Split(new[] { ", position " }, StringSplitOptions.None);
				if (parts.Length == 2)
				{
					var lineStr = parts[0].Trim();
					var posStr = parts[1].TrimEnd('.', ' ');
					if (int.TryParse(lineStr, out int lineNumber) && int.TryParse(posStr, out int linePosition))
					{
						return new XmlLineInfo(lineNumber, linePosition);
					}
				}
			}
			catch
			{
				// Ignore parsing errors
			}
		}
		return new XmlLineInfo();
	}
}
