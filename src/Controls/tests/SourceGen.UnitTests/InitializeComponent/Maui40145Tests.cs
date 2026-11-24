using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class Maui40145Tests : SourceGenXamlInitializeComponentTestBase
{
	[Theory]
	[InlineData("""FontSize="{DynamicResource FontSizeMedium}" """)]
	[InlineData("""FontSize="{DynamicResource Key=FontSizeMedium}" """)]
	[InlineData("""FontSize="{DynamicResource {x:Static local:FontSizes.Medium}}" """)]
	[InlineData("""FontSize="{DynamicResource Key={x:Static local:FontSizes.Medium}}" """)]
	[InlineData("""FontSize="{DynamicResource {x:Static local:FontSizes.ReadOnlyMedium}}" """)]
	[InlineData("""FontSize="{DynamicResource Key={x:Static local:FontSizes.ReadOnlyMedium}}" """)]
	[InlineData("""FontSize="{DynamicResource {x:Static local:FontSizes.MediumProperty}}" """)]
	[InlineData("""FontSize="{DynamicResource Key={x:Static local:FontSizes.MediumProperty}}" """)]
	[InlineData("""FontSize="{DynamicResource {x:String FontSizeMedium}}" """)]
	[InlineData("""FontSize="{DynamicResource Key={x:String FontSizeMedium}}" """)]
	public void DynamicResourceKey_CompilesAndTracksResourceChanges(string fontSizeAttribute)
	{
		var xaml = $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:Test"
			             x:Class="Test.TestPage">
			    <ContentPage.Resources>
			        <x:Double x:Key="FontSizeMedium">16</x:Double>
			    </ContentPage.Resources>
			    <Label {{fontSizeAttribute}} />
			</ContentPage>
			""";
		const string code = """
			using Microsoft.Maui.Controls;
			using Microsoft.Maui.Controls.Xaml;

			namespace Test;

			public static class FontSizes
			{
				public const string Medium = "FontSizeMedium";
				public static readonly string ReadOnlyMedium = Medium;
				public static string MediumProperty => Medium;
			}

			public partial class TestPage : ContentPage
			{
				public TestPage() => InitializeComponent();
			}
			""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation($"Maui40145_{Guid.NewGuid():N}")
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		var xamlFile = new AdditionalXamlFile(Path.Combine(Environment.CurrentDirectory, "Test.xaml"), xaml);

		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, xamlFile);

		Assert.Empty(result.Diagnostics);
		using var stream = new MemoryStream();
		var emitted = compilation.AddSyntaxTrees(result.GeneratedTrees).Emit(stream);
		Assert.True(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));
		var assembly = Assembly.Load(stream.ToArray());
		var page = Assert.IsAssignableFrom<ContentPage>(Activator.CreateInstance(assembly.GetType("Test.TestPage")!));
		var label = Assert.IsType<Label>(page.Content);
		Assert.Equal(16d, label.FontSize);

		page.Resources["FontSizeMedium"] = 24d;

		Assert.Equal(24d, label.FontSize);
	}
}
