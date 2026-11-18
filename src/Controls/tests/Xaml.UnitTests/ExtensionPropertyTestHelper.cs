// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Controls.Xaml.UnitTests.ExternalAssembly;
using Microsoft.CodeAnalysis;
using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

static class ExtensionPropertyTestHelper
{
	public static Type Compile(Type fixture)
	{
		var typeName = $"Generated{fixture.Name}";
		var fullName = $"{fixture.Namespace}.{typeName}";
		var resourceId = XamlResourceIdAttribute.GetResourceIdForType(fixture);
		using var stream = fixture.Assembly.GetManifestResourceStream(resourceId)!;
		using var reader = new StreamReader(stream);
		var xaml = reader.ReadToEnd().Replace(fixture.FullName!, fullName, StringComparison.Ordinal);
		var compilation = CreateExtensionCompilation()
			.AddReferences(
				MetadataReference.CreateFromFile(fixture.Assembly.Location),
				MetadataReference.CreateFromFile(typeof(ExternalLabelExtensions).Assembly.Location))
			.WithAdditionalSource(
$$"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
[assembly: XmlnsDefinition(XamlParser.MauiGlobalUri, "Microsoft.Maui.Controls.Xaml.UnitTests.GlobalScopedExtensions", AssemblyName = "Microsoft.Maui.Controls.Xaml.UnitTests")]
[assembly: XmlnsDefinition(XamlParser.MauiGlobalUri, "httpExtensions", AssemblyName = "Microsoft.Maui.Controls.Xaml.UnitTests")]
namespace {{fixture.Namespace}};
public partial class {{typeName}} : ContentPage
{
	public {{typeName}}() => InitializeComponent();
}
""");
		var result = compilation.RunMauiSourceGenerator(new AdditionalXamlFile($"{typeName}.rt.xaml", xaml));
		return Emit(compilation, result, fullName);
	}

	public static Type CompileSourceDeclaredProperty()
	{
		const string xaml =
"""
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests.SourceExtensions"
	x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.SourceExtensionPage">
	<Label x:Name="label" local:SourceExtensions.SourceTag="{local:SourceValue}" />
</ContentPage>
""";
		var compilation = CreateExtensionCompilation().WithAdditionalSource(
"""
using System;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceExtensions
{
	[RequireService([typeof(IProvideValueTarget)])]
	public class SourceValueExtension : IMarkupExtension<string>
	{
		public string ProvideValue(IServiceProvider provider)
		{
			var target = (IProvideValueTarget)provider.GetService(typeof(IProvideValueTarget))!;
			var property = (PropertyInfo)target.TargetProperty;
			return $"{property.Name}:{property.PropertyType.Name}";
		}
		object IMarkupExtension.ProvideValue(IServiceProvider provider) => ProvideValue(provider);
	}

	public static class SourceExtensions
	{
		extension(Label label)
		{
			public string SourceTag { set => label.StyleId = value; }
		}
	}
}
namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class SourceExtensionPage : ContentPage
	{
		public SourceExtensionPage() => InitializeComponent();
	}
}
""");
		var result = compilation.RunMauiSourceGenerator(new AdditionalXamlFile("SourceExtensionPage.rt.xaml", xaml));
		return Emit(compilation, result, "Microsoft.Maui.Controls.Xaml.UnitTests.SourceExtensionPage");
	}

	static Compilation CreateExtensionCompilation()
	{
		var runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
		return CreateMauiCompilation().AddReferences(
			MetadataReference.CreateFromFile(typeof(System.ComponentModel.TypeConverter).Assembly.Location),
			MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.ComponentModel.dll")),
			MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.Xml.ReaderWriter.dll")),
			MetadataReference.CreateFromFile(typeof(System.Xml.IXmlLineInfo).Assembly.Location));
	}

	static Type Emit(Compilation compilation, GeneratorDriverRunResult result, string fullName)
	{
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
			.Select(d => $"{d.Id}: {d.GetMessage()}"));

		using var output = new MemoryStream();
		var emit = compilation.AddSyntaxTrees(result.GeneratedTrees).Emit(output);
		Assert.True(emit.Success, string.Join(Environment.NewLine, emit.Diagnostics.Select(d => $"{d.Id}: {d.GetMessage()}")));
		return Assembly.Load(output.ToArray()).GetType(fullName, throwOnError: true)!;
	}

	public static T Named<T>(this ContentPage page, string name) where T : class
		=> Assert.IsType<T>(page.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(page));
}
