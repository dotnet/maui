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

using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;
using static Microsoft.Maui.Controls.SourceGen.TypeHelpers;

namespace Microsoft.Maui.Controls.SourceGen;

[Generator(LanguageNames.CSharp)]
public class CodeBehindGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext initContext)
	{
#if DEBUG
		if (!System.Diagnostics.Debugger.IsAttached)
		{
			System.Diagnostics.Debugger.Launch();
		}
#endif
		var projectItemProvider = initContext.AdditionalTextsProvider
			.Combine(initContext.AnalyzerConfigOptionsProvider)
			.Select(ComputeProjectItem)
			.WithTrackingName(TrackingNames.ProjectItemProvider);

		var xamlProjectItemProviderForCB = projectItemProvider
			.Where(static p => p?.Kind == "Xaml")
			.Select(ComputeXamlProjectItemForCB)
			.WithTrackingName(TrackingNames.XamlProjectItemProviderForCB);
		
		var xamlProjectItemProviderForIC = projectItemProvider
			.Where(static p => p?.Kind == "Xaml")
			.Select(ComputeXamlProjectItemForIC)
			.WithTrackingName(TrackingNames.XamlProjectItemProviderForIC);

		var cssProjectItemProvider = projectItemProvider
			.Where(static p => p?.Kind == "Css")
			.WithTrackingName(TrackingNames.CssProjectItemProvider);

		// Only provide a new Compilation when the references change
		var referenceCompilationProvider = initContext.CompilationProvider
			.WithComparer(new CompilationReferencesComparer())
			.WithTrackingName(TrackingNames.ReferenceCompilationProvider);

		var xmlnsDefinitionsProvider = referenceCompilationProvider
			.Select(GetAssemblyAttributes)
			.WithTrackingName(TrackingNames.XmlnsDefinitionsProvider);

		var referenceTypeCacheProvider = referenceCompilationProvider
			.Select(GetTypeCache)
			.WithTrackingName(TrackingNames.ReferenceTypeCacheProvider);

		var xamlSourceProviderForCB = xamlProjectItemProviderForCB
			.Combine(xmlnsDefinitionsProvider)
			.Combine(referenceTypeCacheProvider)
			.Combine(referenceCompilationProvider)
			.Select(static (t, _) => (t.Left.Left, t.Left.Right, t.Right))
			.WithTrackingName(TrackingNames.XamlSourceProviderForCB);

		var xamlSourceProviderForIC = xamlProjectItemProviderForIC
			.Combine(xmlnsDefinitionsProvider)
			.Combine(referenceTypeCacheProvider)
			.Combine(referenceCompilationProvider)
			.Select(static (t, _) => (t.Left.Left, t.Left.Right, t.Right))
			.WithTrackingName(TrackingNames.XamlSourceProviderForIC);

		// Register the XAML pipeline for CodeBehind
		initContext.RegisterSourceOutput(xamlSourceProviderForCB, static (sourceProductionContext, provider) =>
		{
			var ((xamlItem, xmlnsCache), typeCache, compilation) = provider;

			CodeBehindCodeWriter.GenerateXamlCodeBehind(xamlItem, compilation, sourceProductionContext, xmlnsCache, typeCache);
		});

		// Register the XAML pipeline for InitializeComponent
		initContext.RegisterImplementationSourceOutput(xamlSourceProviderForIC, static (spc, provider) =>
		{
			var ((xamlItem, xmlnsCache), typeCache, compilation) = provider;
			if (!CanSourceGenXaml(xamlItem, compilation, spc, xmlnsCache, typeCache))
				return;
			
			var fileName = $"{(string.IsNullOrEmpty(Path.GetDirectoryName(xamlItem!.ProjectItem!.TargetPath)) ? "" : Path.GetDirectoryName(xamlItem.ProjectItem.TargetPath) + Path.DirectorySeparatorChar)}{Path.GetFileNameWithoutExtension(xamlItem.ProjectItem.TargetPath)}.{xamlItem.ProjectItem.Kind.ToLowerInvariant()}.xsg.cs".Replace(Path.DirectorySeparatorChar, '_');
			var code = InitializeComponentCodeWriter.GenerateInitializeComponent(xamlItem, compilation, xmlnsCache, typeCache);
			spc.AddSource(fileName, code);
			
		});

		// Register the CSS pipeline
		initContext.RegisterImplementationSourceOutput(cssProjectItemProvider, static (sourceProductionContext, cssItem) =>
		{
			if (cssItem == null)			
				return;

			GenerateCssCodeBehind(cssItem, sourceProductionContext);
		});
	}

	static bool CanSourceGenXaml(XamlProjectItemForIC? xamlItem, Compilation compilation, SourceProductionContext context, AssemblyCaches xmlnsCache, IDictionary<XmlType, string> typeCache)
	{
		ProjectItem? projItem;
		if (xamlItem == null || (projItem = xamlItem.ProjectItem) == null)
			return false;
		var itemName = projItem.ManifestResourceName ?? projItem.RelativePath;
		if (itemName == null)
			return false;
		if (xamlItem.Root == null)
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
}