using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class Maui32758Tests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null, string LineInfo = "enable")
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn, LineInfo: LineInfo);

	[Fact]
	public void StaticExtensionOnPickerItemsSourceShouldUseSetValue()
	{
		// Test that Static markup extension on Picker.ItemsSource generates SetValue instead of Add to collection
		// 
		// Issue: https://github.com/dotnet/maui/issues/32758
		// 
		// When using x:Static to bind a static list to Picker.ItemsSource, the source generator
		// incorrectly generates code that tries to Add() to a collection returned by GetValue():
		//   ((global::System.Collections.IList)picker.GetValue(Picker.ItemsSourceProperty)).Add((object)staticExtension);
		// 
		// This throws NullReferenceException because ItemsSource is null until set.
		// 
		// The correct code should use SetValue() to set the entire collection:
		//   picker.SetValue(Picker.ItemsSourceProperty, staticExtension);
		//
		var viewModelCode =
"""
using System.Collections.Generic;

namespace Test
{
	public class MainPageViewModel
	{
		public static IReadOnlyList<string> PrimitiveValues => new List<string> { "first", "second", "third" };
	}
}
""";

		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Test"
	x:Class="Test.TestPage">
	<Picker ItemsSource="{x:Static local:MainPageViewModel.PrimitiveValues}" />
</ContentPage>
""";

		var codeBehind =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
""";

		var compilation = CreateMauiCompilation();
		
		// Add the view model code and codebehind to the compilation
		compilation = compilation.AddSyntaxTrees(
			CSharpSyntaxTree.ParseText(viewModelCode),
			CSharpSyntaxTree.ParseText(codeBehind)
		);

		var workingDirectory = Environment.CurrentDirectory;
		var xamlFile = new AdditionalXamlFile(
			Path.Combine(workingDirectory, "Test.xaml"), 
			xaml, 
			RelativePath: "Test.xaml",
			ManifestResourceName: $"{compilation.AssemblyName}.Test.xaml"
		);

		var result = RunGenerator<XamlGenerator>(compilation, xamlFile);

		// Check for compilation errors
		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		
		// Output diagnostics for debugging
		System.Console.WriteLine($"=== DIAGNOSTICS ({result.Diagnostics.Length} total) ===");
		foreach (var diag in result.Diagnostics)
		{
			System.Console.WriteLine($"{diag.Severity}: {diag.GetMessage()}");
		}
		System.Console.WriteLine("=== END DIAGNOSTICS ===");
		
		Assert.False(errors.Any(), $"Found errors: {string.Join(", ", errors.Select(e => e.GetMessage()))}");

		// Get the generated code (InitializeComponent implementation in .xsg.cs file)
		System.Console.WriteLine($"=== Generated sources count: {result.Results[0].GeneratedSources.Length} ===");
		foreach (var (source, idx) in result.Results[0].GeneratedSources.Select((s, i) => (s, i)))
		{
			System.Console.WriteLine($"[{idx}] Source hint: {source.HintName}");
		}
		
		// Find the Test.xaml.xsg.cs file (the InitializeComponent implementation)
		var generatedSource = result.Results[0].GeneratedSources.FirstOrDefault(s => s.HintName.EndsWith(".xsg.cs", System.StringComparison.Ordinal));
		if (generatedSource.SourceText == null)
		{
			Assert.Fail("Could not find .xsg.cs file in generated sources");
			return;
		}
		
		var generatedCode = generatedSource.SourceText.ToString();
		
		System.Console.WriteLine("=== FULL GENERATED CODE (.xsg.cs) ===");
		System.Console.WriteLine(generatedCode);
		System.Console.WriteLine("=== END GENERATED CODE ===");

		// CRITICAL: The generated code should use SetValue, NOT GetValue().Add()
		// Wrong pattern: ((global::System.Collections.IList)picker1.GetValue(global::Microsoft.Maui.Controls.Picker.ItemsSourceProperty)).Add((object)staticExtension);
		// Correct pattern: picker1.SetValue(global::Microsoft.Maui.Controls.Picker.ItemsSourceProperty, <static value>);

		var expected = 
"""
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a .NET MAUI source generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

namespace Test;

[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Maui.Controls.SourceGen, Version=10.0.0.0, Culture=neutral, PublicKeyToken=null", "10.0.0.0")]
public partial class TestPage
{
	private partial void InitializeComponent()
	{
		// Fallback to Runtime inflation if the page was updated by HotReload
		static string? getPathForType(global::System.Type type)
		{
			var assembly = type.Assembly;
			foreach (var xria in global::System.Reflection.CustomAttributeExtensions.GetCustomAttributes<global::Microsoft.Maui.Controls.Xaml.XamlResourceIdAttribute>(assembly))
			{
				if (xria.Type == type)
					return xria.Path;
			}
			return null;
		}

		var rlr = global::Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceProvider2?.Invoke(new global::Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceLoadingQuery
		{
			AssemblyName = typeof(global::Test.TestPage).Assembly.GetName(),
			ResourcePath = getPathForType(typeof(global::Test.TestPage)),
			Instance = this,
		});

		if (rlr?.ResourceContent != null)
		{
			this.InitializeComponentRuntime();
			return;
		}

		var staticExtension = global::Test.MainPageViewModel.PrimitiveValues;
		global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(staticExtension!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 7, 10);
		var picker = new global::Microsoft.Maui.Controls.Picker();
		global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(picker!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 7, 3);
		var __root = this;
		global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(__root!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 2, 2);
#if !_MAUIXAML_SG_NAMESCOPE_DISABLE
		global::Microsoft.Maui.Controls.Internals.INameScope iNameScope = global::Microsoft.Maui.Controls.Internals.NameScope.GetNameScope(__root) ?? new global::Microsoft.Maui.Controls.Internals.NameScope();
#endif
#if !_MAUIXAML_SG_NAMESCOPE_DISABLE
		global::Microsoft.Maui.Controls.Internals.NameScope.SetNameScope(__root, iNameScope);
#endif
#if !_MAUIXAML_SG_NAMESCOPE_DISABLE
		picker.transientNamescope = iNameScope;
#endif
#line 7 "/Users/sde/Projects/Microsoft/maui/artifacts/bin/SourceGen.UnitTests/Debug/net10.0/Test.xaml"
		picker.SetValue(global::Microsoft.Maui.Controls.Picker.ItemsSourceProperty, (global::System.Collections.IList)staticExtension);
#line default
#line 7 "/Users/sde/Projects/Microsoft/maui/artifacts/bin/SourceGen.UnitTests/Debug/net10.0/Test.xaml"
		__root.SetValue(global::Microsoft.Maui.Controls.ContentPage.ContentProperty, picker);
#line default
	}
}

""";
		
		Assert.Equal(expected, generatedCode);
	}

	[Fact]
	public void StaticExtensionOnListViewItemsSourceShouldUseSetValue()
	{
		// Test that Static markup extension on ListView.ItemsSource also uses SetValue
		var viewModelCode =
"""
using System.Collections.Generic;

namespace Test
{
	public class MainPageViewModel
	{
		public static IReadOnlyList<string> Items => new List<string> { "Item1", "Item2", "Item3" };
	}
}
""";

		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Test"
	x:Class="Test.TestPage">
	<ListView ItemsSource="{x:Static local:MainPageViewModel.Items}" />
</ContentPage>
""";

		var codeBehind =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
""";

		var compilation = CreateMauiCompilation();
		
		// Add the view model code and codebehind to the compilation
		compilation = compilation.AddSyntaxTrees(
			CSharpSyntaxTree.ParseText(viewModelCode),
			CSharpSyntaxTree.ParseText(codeBehind)
		);

		var workingDirectory = Environment.CurrentDirectory;
		var xamlFile = new AdditionalXamlFile(
			Path.Combine(workingDirectory, "Test.xaml"), 
			xaml, 
			RelativePath: "Test.xaml",
			ManifestResourceName: $"{compilation.AssemblyName}.Test.xaml"
		);

		var result = RunGenerator<XamlGenerator>(compilation, xamlFile);

		// Check for compilation errors
		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		
		Assert.False(errors.Any(), $"Found errors: {string.Join(", ", errors.Select(e => e.GetMessage()))}");

		// Get the generated code
		var generatedSource = result.Results[0].GeneratedSources.FirstOrDefault(s => s.HintName.EndsWith(".xsg.cs", System.StringComparison.Ordinal));
		if (generatedSource.SourceText == null)
		{
			Assert.Fail("Could not find .xsg.cs file in generated sources");
			return;
		}
		
		var generatedCode = generatedSource.SourceText.ToString();
		
		System.Console.WriteLine("=== GENERATED CODE (.xsg.cs) ===");
		System.Console.WriteLine(generatedCode);
		System.Console.WriteLine("=== END ===");

		// Should use SetValue, not GetValue().Add()
		var expected = 
"""
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a .NET MAUI source generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

namespace Test;

[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Maui.Controls.SourceGen, Version=10.0.0.0, Culture=neutral, PublicKeyToken=null", "10.0.0.0")]
public partial class TestPage
{
	private partial void InitializeComponent()
	{
		// Fallback to Runtime inflation if the page was updated by HotReload
		static string? getPathForType(global::System.Type type)
		{
			var assembly = type.Assembly;
			foreach (var xria in global::System.Reflection.CustomAttributeExtensions.GetCustomAttributes<global::Microsoft.Maui.Controls.Xaml.XamlResourceIdAttribute>(assembly))
			{
				if (xria.Type == type)
					return xria.Path;
			}
			return null;
		}

		var rlr = global::Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceProvider2?.Invoke(new global::Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceLoadingQuery
		{
			AssemblyName = typeof(global::Test.TestPage).Assembly.GetName(),
			ResourcePath = getPathForType(typeof(global::Test.TestPage)),
			Instance = this,
		});

		if (rlr?.ResourceContent != null)
		{
			this.InitializeComponentRuntime();
			return;
		}

		var staticExtension = global::Test.MainPageViewModel.Items;
		global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(staticExtension!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 7, 12);
		var listView = new global::Microsoft.Maui.Controls.ListView();
		global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(listView!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 7, 3);
		var __root = this;
		global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(__root!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 2, 2);
#if !_MAUIXAML_SG_NAMESCOPE_DISABLE
		global::Microsoft.Maui.Controls.Internals.INameScope iNameScope = global::Microsoft.Maui.Controls.Internals.NameScope.GetNameScope(__root) ?? new global::Microsoft.Maui.Controls.Internals.NameScope();
#endif
#if !_MAUIXAML_SG_NAMESCOPE_DISABLE
		global::Microsoft.Maui.Controls.Internals.NameScope.SetNameScope(__root, iNameScope);
#endif
#if !_MAUIXAML_SG_NAMESCOPE_DISABLE
		listView.transientNamescope = iNameScope;
#endif
#line 7 "/Users/sde/Projects/Microsoft/maui/artifacts/bin/SourceGen.UnitTests/Debug/net10.0/Test.xaml"
		listView.SetValue(global::Microsoft.Maui.Controls.ItemsView<global::Microsoft.Maui.Controls.Cell>.ItemsSourceProperty, staticExtension);
#line default
#line 7 "/Users/sde/Projects/Microsoft/maui/artifacts/bin/SourceGen.UnitTests/Debug/net10.0/Test.xaml"
		__root.SetValue(global::Microsoft.Maui.Controls.ContentPage.ContentProperty, listView);
#line default
	}
}

""";
		
		Assert.Equal(expected, generatedCode);
	}
}

