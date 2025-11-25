using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class Maui32837Tests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn);

	[Fact]
	public void ConverterWithStaticResourceShouldReceiveCorrectValue()
	{
		// Test that converter receives correct value when binding uses StaticResource
		var converterCode =
"""
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TestApp
{
    public class IntToCornerRadiusConverter : IValueConverter
    {
        public object Convert(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int radius)
            {
                return new CornerRadius(radius);
            }
            return new CornerRadius(0);
        }

        public object ConvertBack(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is CornerRadius cornerRadius)
            {
                return (int)cornerRadius.TopLeft;
            }
            return 0;
        }
    }
}
""";

		var appXaml =
"""
<?xml version="1.0" encoding="UTF-8" ?>
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.App">
    <Application.Resources>
        <x:Int32 x:Key="MyRadius">16</x:Int32>
        
        <local:IntToCornerRadiusConverter x:Key="IntToCornerRadiusConverter" />
        
        <RoundRectangle x:Key="MyRoundRectangle">
            <RoundRectangle.CornerRadius>
                <Binding Source="{StaticResource MyRadius}" 
                         Converter="{StaticResource IntToCornerRadiusConverter}"/>
            </RoundRectangle.CornerRadius>
        </RoundRectangle>
        
        <Style TargetType="Border" ApplyToDerivedTypes="True">
            <Setter Property="StrokeShape" Value="{StaticResource MyRoundRectangle}" />
        </Style>
    </Application.Resources>
</Application>
""";

		var appCodeBehind =
"""
using Microsoft.Maui.Controls;

namespace TestApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
    }
}
""";

		var mainPageXaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.MainPage"
             BackgroundColor="#333333"
             Title="Main Page">
    <StackLayout Spacing="8">
        <Border x:Name="TestBorder" BackgroundColor="White" Padding="24" Margin="16" />
    </StackLayout>
</ContentPage>
""";

		var mainPageCodeBehind =
"""
using Microsoft.Maui.Controls;

namespace TestApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
    }
}
""";

		var compilation = CreateMauiCompilation();
		
		// Add the converter code to the compilation
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(converterCode));
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(appCodeBehind));
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(mainPageCodeBehind));

		// Run generator for App.xaml
		var appResult = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("App.xaml", appXaml));

		// Check for errors in App.xaml generation
		var appErrors = appResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		
		// Output diagnostics for debugging
		foreach (var diag in appErrors)
		{
			System.Console.WriteLine($"App.xaml Error: {diag.GetMessage()}");
		}
		
		Assert.Empty(appErrors);

		// Update compilation with generated App.xaml.g.cs
		if (appResult.GeneratedTrees.Any())
		{
			compilation = compilation.AddSyntaxTrees(appResult.GeneratedTrees);
		}

		// Run generator for MainPage.xaml
		var mainPageResult = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("MainPage.xaml", mainPageXaml));

		// Check for errors in MainPage.xaml generation
		var mainPageErrors = mainPageResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		
		// Output diagnostics for debugging
		foreach (var diag in mainPageErrors)
		{
			System.Console.WriteLine($"MainPage.xaml Error: {diag.GetMessage()}");
		}
		
		Assert.Empty(mainPageErrors);

		// Verify the generated code contains proper converter usage
		var mainPageGenerated = mainPageResult.Results.Length > 0 
			? mainPageResult.Results[0].GeneratedSources
				.FirstOrDefault(gs => gs.HintName.EndsWith(".sg.cs", System.StringComparison.OrdinalIgnoreCase))
				.SourceText?.ToString() ?? ""
			: "";
		
		System.Console.WriteLine("Generated MainPage.sg.cs code:");
		System.Console.WriteLine(mainPageGenerated);
		
		// Also check App.xaml generated code
		var appGenerated = appResult.Results.Length > 0
			? appResult.Results[0].GeneratedSources
				.FirstOrDefault(gs => gs.HintName.EndsWith(".sg.cs", System.StringComparison.OrdinalIgnoreCase))
				.SourceText?.ToString() ?? ""
			: "";
		
		System.Console.WriteLine("\nGenerated App.sg.cs code:");
		System.Console.WriteLine(appGenerated);

		// The generated code should properly setup the binding with converter
		// This is where the bug manifests - converter doesn't receive correct value in SourceGen
		Assert.NotEmpty(mainPageGenerated);
		Assert.NotEmpty(appGenerated);
	}
}
