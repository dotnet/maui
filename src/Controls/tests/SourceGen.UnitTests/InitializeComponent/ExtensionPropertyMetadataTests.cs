using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class ExtensionPropertyMetadataTests : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void MetadataPropertyIsAvailableToMarkupExtensionWithRoslyn412()
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Microsoft.Maui.Controls.SourceGen.UnitTests;assembly=Microsoft.Maui.Controls.SourceGen.UnitTests"
	x:Class="Test.ExtensionMetadataPage">
	<Label x:Name="label" local:MetadataExtensions.MetadataTag="{local:MetadataTarget}" />
</ContentPage>
""";
		const string code = """
using Microsoft.Maui.Controls;
namespace Test;
public partial class ExtensionMetadataPage : ContentPage
{
	public ExtensionMetadataPage() => InitializeComponent();
}
""";
		var runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
		var compilation = CreateMauiCompilation()
			.AddReferences(
				MetadataReference.CreateFromFile(typeof(ExtensionPropertyMetadataTests).Assembly.Location),
				MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.ComponentModel.dll")))
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("ExtensionMetadataPage.xaml", xaml));
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
			.Select(d => $"{d.Id}: {d.GetMessage()}"));

		using var output = new MemoryStream();
		var emit = compilation.AddSyntaxTrees(result.GeneratedTrees).Emit(output, cancellationToken: TestContext.Current.CancellationToken);
		Assert.True(emit.Success, string.Join(Environment.NewLine, emit.Diagnostics.Select(d => $"{d.Id}: {d.GetMessage()}")));
		var type = Assembly.Load(output.ToArray()).GetType("Test.ExtensionMetadataPage", throwOnError: true)!;
		var page = (ContentPage)Activator.CreateInstance(type)!;
		var label = (Label)type.GetField("label", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(page)!;
		Assert.Equal("MetadataTag:Object", label.StyleId);
	}
}

public static class MetadataExtensions
{
	extension(Label label)
	{
		public object MetadataTag
		{
			set => label.StyleId = value.ToString();
		}
	}
}

[RequireService([typeof(IProvideValueTarget)])]
public class MetadataTargetExtension : IMarkupExtension<object>
{
	public object ProvideValue(IServiceProvider serviceProvider)
	{
		var target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget))!;
		var property = (PropertyInfo)target.TargetProperty;
		return $"{property.Name}:{property.PropertyType.Name}";
	}

	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}
