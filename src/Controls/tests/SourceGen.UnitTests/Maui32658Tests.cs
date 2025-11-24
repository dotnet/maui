using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class Maui32658Tests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn);

	[Fact]
	public void MarkupExtensionWithCustomXmlnsNoThirdParty()
	{
		// Test reproducing MauiIcons pattern: generic abstract base class that returns BindingBase
		// This more closely matches the actual MauiIcons implementation
		var markupExtensionCode =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System;

[assembly: XmlnsDefinition("http://test.example.com/markup", "TestMarkup")]

namespace TestMarkup
{
	public enum TestIcons
	{
		None,
		Home,
		Settings,
		AccessAlarm
	}

	// Simulate MauiIcons BaseIcon class
	public abstract class BaseIcon : BindableObject
	{
		public static readonly BindableProperty IconColorProperty = 
			BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(BaseIcon), null);

		public Color IconColor
		{
			get => (Color)GetValue(IconColorProperty);
			set => SetValue(IconColorProperty, value);
		}
	}

	// Simulate MauiIcons generic BaseIconExtension<TEnum>
	[ContentProperty(nameof(Icon))]
	public abstract class BaseIconExtension<TEnum> : BaseIcon, IMarkupExtension<BindingBase> where TEnum : Enum
	{
		public static new readonly BindableProperty IconProperty = 
			BindableProperty.Create(nameof(Icon), typeof(TEnum?), typeof(BaseIconExtension<TEnum>), null);

		public TEnum? Icon
		{
			get => (TEnum?)GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

		public BindingBase ProvideValue(IServiceProvider serviceProvider)
		{
			// Return a binding that would produce a FontImageSource
			return new Binding 
			{ 
				Source = new FontImageSource 
				{ 
					Glyph = Icon?.ToString() ?? "",
					FontFamily = "TestFont",
					Size = 24
				},
				Path = "."
			};
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) 
			=> ProvideValue(serviceProvider);
	}

	// Concrete implementation like MaterialOutlinedExtension
	public class TestIconExtension : BaseIconExtension<TestIcons>
	{
	}
}
""";

		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="http://test.example.com/markup"
	x:Class="Test.TestPage">
	<ImageButton Source="{test:TestIcon Icon=AccessAlarm}"/>
</ContentPage>
""";

		var compilation = CreateMauiCompilation();
		
		// Add the markup extension code to the compilation
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(markupExtensionCode));

		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		// This should NOT produce "Index was outside the bounds of the array" error
		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		
		// Output diagnostics for debugging
		foreach (var diag in errors)
		{
			System.Console.WriteLine($"Error: {diag.GetMessage()}");
		}
		
		Assert.False(errors.Any(e => e.GetMessage().Contains("Index was outside the bounds of the array", System.StringComparison.Ordinal)), 
			$"Found IndexOutOfRangeException: {string.Join(", ", errors.Select(e => e.GetMessage()))}");
	}
}
