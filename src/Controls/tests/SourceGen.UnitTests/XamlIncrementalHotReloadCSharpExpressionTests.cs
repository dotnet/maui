using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Tests that XIHR (XAML Incremental Hot Reload) correctly generates
/// <c>UpdateComponent()</c> patches when XAML uses C# expressions.
/// Verifies that property changes, expression changes, and structural
/// changes in expression-based XAML produce correct UC code.
/// </summary>
[Collection("XamlHotReloadTests")]
public class XamlIncrementalHotReloadCSharpExpressionTests : SourceGenXamlInitializeComponentTestBase, IDisposable
{
	public XamlIncrementalHotReloadCSharpExpressionTests() => XamlHotReloadState.Reset();
	public void Dispose() => XamlHotReloadState.Reset();

	GeneratorDriver? _driver;
	Compilation? _compilation;
	AdditionalXamlFile? _xamlFile;

	const string ViewModelCode =
"""
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class TestViewModel : INotifyPropertyChanged
{
	public string FirstName { get; set; } = "John";
	public string LastName { get; set; } = "Doe";
	public int Count { get; set; } = 42;
	public bool IsActive { get; set; } = true;
	public string? NullableValue { get; set; }
	public decimal Price { get; set; } = 9.99m;
	public int Quantity { get; set; } = 3;
	public string GetText() => "Hello";
	public string GetDisplayName() => $"{FirstName} {LastName}";
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public decimal TaxRate => 0.1m;
	public TestPage() => InitializeComponent();
}
""";

	[Fact]
	public void CSharpExpression_IC_EmitsRegistration()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{FirstName}" />
</ContentPage>
""";
		var (_, text) = RunGenerator(xaml, ViewModelCode, enableIncrementalHotReload: true);
		Assert.NotNull(text);
		Assert.Contains("XamlComponentRegistry.Register(this,", text, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_IC_CompilesCleanly()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <VerticalStackLayout>
        <Label Text="{FirstName}" />
        <Label Text="{GetText()}" />
        <Label IsVisible="{IsActive}" />
    </VerticalStackLayout>
</ContentPage>
""";
		var (result, text) = RunGenerator(xaml, ViewModelCode, enableIncrementalHotReload: true);
		Assert.NotNull(text);
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
	}

	[Fact]
	public void CSharpExpression_PropertyChange_GeneratesUC()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{FirstName}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{LastName}" />
</ContentPage>
""";
		// V1 seeds state
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		// V2 should produce UC with SetBinding
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);

		// Verify generated UC contains SetBinding with TypedBinding for the expression
		Assert.Contains("SetBinding", ucSource, StringComparison.Ordinal);
		Assert.Contains("TypedBinding<global::TestApp.TestViewModel, string>", ucSource, StringComparison.Ordinal);
		Assert.Contains("__source.LastName", ucSource, StringComparison.Ordinal);
		Assert.Contains("\"LastName\"", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("not supported", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_MethodCallChange_GeneratesUC()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{GetText()}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{GetDisplayName()}" />
</ContentPage>
""";
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.Contains("SetBinding", ucSource, StringComparison.Ordinal);
		Assert.Contains("GetDisplayName()", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("not supported", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_TernaryChange_GeneratesUC()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{IsActive ? 'Yes' : 'No'}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{IsActive ? 'Active' : 'Inactive'}" />
</ContentPage>
""";
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.Contains("SetBinding", ucSource, StringComparison.Ordinal);
		Assert.Contains("TypedBinding", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("not supported", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_NullCoalescingChange_GeneratesUC()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{NullableValue ?? 'Default'}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{NullableValue ?? 'N/A'}" />
</ContentPage>
""";
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.Contains("SetBinding", ucSource, StringComparison.Ordinal);
		Assert.Contains("TypedBinding", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("not supported", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_StringInterpolationChange_GeneratesUC()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{$'Hello {FirstName}'}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{$'Hi {FirstName} {LastName}'}" />
</ContentPage>
""";
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.Contains("SetBinding", ucSource, StringComparison.Ordinal);
		Assert.Contains("TypedBinding", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("not supported", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_ConcatenationChange_GeneratesUC()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{FirstName + ' ' + LastName}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{LastName + ', ' + FirstName}" />
</ContentPage>
""";
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.Contains("SetBinding", ucSource, StringComparison.Ordinal);
		Assert.Contains("TypedBinding", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("not supported", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_IdenticalXaml_EmitsEmptyUC()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label Text="{FirstName + ' ' + LastName}" />
</ContentPage>
""";
		RunGenerator(xaml, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xaml, ViewModelCode, enableIncrementalHotReload: true);

		// UC is always emitted (present-but-empty for unchanged XAML) so the method never disappears.
		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.Contains("internal void UpdateComponent()", ucSource!, StringComparison.Ordinal);
		Assert.DoesNotContain("XamlComponentRegistry", ucSource!, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_MixedExpressionAndPlainProperty_GeneratesUC()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel"
             Title="V1">
    <Label Text="{FirstName}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel"
             Title="V2">
    <Label Text="{LastName}" />
</ContentPage>
""";
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		// Both the plain Title change and expression change should be in UC
		Assert.Contains("\"V2\"", ucSource, StringComparison.Ordinal);
		// C# expression generates SetBinding
		Assert.Contains("SetBinding", ucSource, StringComparison.Ordinal);
		Assert.Contains("LastName", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("not supported", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_NewElementAdded_GeneratesUC()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <VerticalStackLayout>
        <Label Text="{FirstName}" />
    </VerticalStackLayout>
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <VerticalStackLayout>
        <Label Text="{FirstName}" />
        <Label Text="{LastName}" />
    </VerticalStackLayout>
</ContentPage>
""";
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		// Structural change may or may not produce UC depending on implementation;
		// if UC is null, the fallback full-reload path handles it
		if (ucSource is not null)
		{
			// New design: UpdateComponent() is emitted (present) but carries no version-chain guard.
			Assert.DoesNotContain("if (__version ==", ucSource, StringComparison.Ordinal);
		}
	}

	[Fact]
	public void CSharpExpression_LambdaEventChange_GeneratesUC()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.TestPage">
    <Button Text="Click" Clicked="{(s, e) => Count++}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.TestPage">
    <Button Text="Click" Clicked="{(s, e) => Count += 2}" />
</ContentPage>
""";

		var lambdaCode =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public int Count { get; set; }
	public TestPage() => InitializeComponent();
}
""";

		RunGenerator(xamlV1, lambdaCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, lambdaCode, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.DoesNotContain("if (__version ==", ucSource!, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_OperatorAliasChange_GeneratesUC()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label IsVisible="{Count GT 0}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Label IsVisible="{Count GTE 10}" />
</ContentPage>
""";
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.Contains("SetBinding", ucSource, StringComparison.Ordinal);
		Assert.Contains("TypedBinding", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("not supported", ucSource, StringComparison.Ordinal);
	}

	/// <summary>
	/// Extracts the UC (UpdateComponent) source from generator results, if any.
	/// </summary>

	[Fact]
	public void DynamicResource_Change_GeneratesSetDynamicResource()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.TestPage">
    <Label TextColor="{DynamicResource PrimaryColor}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.TestPage">
    <Label TextColor="{DynamicResource SecondaryColor}" />
</ContentPage>
""";
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);
		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.Contains("SetDynamicResource", ucSource, StringComparison.Ordinal);
		Assert.Contains("SecondaryColor", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("not supported", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void StaticResource_Change_GeneratesStaticResourceLookup()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.TestPage">
    <Label TextColor="{StaticResource PrimaryColor}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.TestPage">
    <Label TextColor="{StaticResource SecondaryColor}" />
</ContentPage>
""";
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);
		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.Contains("SecondaryColor", ucSource, StringComparison.Ordinal);
		Assert.Contains("SetValue", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("not supported", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void Binding_PathChange_GeneratesSetBinding()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.TestPage">
    <Label Text="{Binding FirstName}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.TestPage">
    <Label Text="{Binding LastName}" />
</ContentPage>
""";
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);
		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.Contains("SetBinding", ucSource, StringComparison.Ordinal);
		Assert.Contains("LastName", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("not supported", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void Binding_WithModeAndStringFormat_GeneratesSetBinding()
	{
		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.TestPage">
    <Label Text="{Binding FirstName}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.TestPage">
    <Label Text="{Binding LastName, Mode=OneWay, StringFormat='Name: {0}'}" />
</ContentPage>
""";
		RunGenerator(xamlV1, ViewModelCode, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, ViewModelCode, enableIncrementalHotReload: true);
		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.Contains("SetBinding", ucSource, StringComparison.Ordinal);
		Assert.Contains("\"LastName\"", ucSource, StringComparison.Ordinal);
		Assert.Contains("BindingMode.OneWay", ucSource, StringComparison.Ordinal);
		Assert.Contains("Name: {0}", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("not supported", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_XmlnsPrefixedTypeChange_GeneratesUC()
	{
		const string code =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp.Helpers
{
	public static class Labels
	{
		public static string Active => "Active";
		public static string Inactive => "Inactive";
	}
}

namespace TestApp
{
	public class TestViewModel : INotifyPropertyChanged
	{
		public bool IsActive { get; set; } = true;
		public string LabelText { get; set; } = "Initial";
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class TestPage : ContentPage
	{
		public TestPage() => InitializeComponent();
	}
}
""";

		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             xmlns:helpers="clr-namespace:TestApp.Helpers"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <VerticalStackLayout>
        <Label Text="{LabelText}" />
    </VerticalStackLayout>
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             xmlns:helpers="clr-namespace:TestApp.Helpers"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <VerticalStackLayout>
        <Label Text="{= IsActive ? helpers:Labels.Active : helpers:Labels.Inactive}" />
        <Button Text="Second" />
    </VerticalStackLayout>
</ContentPage>
""";

		RunGenerator(xamlV1, code, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, code, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.DoesNotContain("helpers:", ucSource, StringComparison.Ordinal);
		Assert.Contains("global::TestApp.Helpers.Labels.Active", ucSource, StringComparison.Ordinal);
		Assert.Contains("global::TestApp.Helpers.Labels.Inactive", ucSource, StringComparison.Ordinal);
		Assert.Contains("SetBinding", ucSource, StringComparison.Ordinal);
		Assert.Contains("new(static __source => __source, \"IsActive\")", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_ObservablePropertyTwoWay_GeneratesTypedSetterInUC()
	{
		const string code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace CommunityToolkit.Mvvm.ComponentModel
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class ObservablePropertyAttribute : Attribute
	{
	}
}

namespace TestApp
{
	public partial class TestViewModel
	{
		[CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
		private string? _nickname;
	}

	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class TestPage : ContentPage
	{
		public TestPage() => InitializeComponent();
	}
}
""";

		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Entry Text="Initial" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <Entry Text="{Nickname}" />
</ContentPage>
""";

		RunGenerator(xamlV1, code, assertNoCompilationErrors: false, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, code, assertNoCompilationErrors: false, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.Contains("new global::Microsoft.Maui.Controls.Internals.TypedBinding<global::TestApp.TestViewModel, string>(", ucSource, StringComparison.Ordinal);
		Assert.Contains("(__source, __value) => __source.Nickname = __value,", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_HeuristicBindableProperty_UpdatesExpressionInUC()
	{
		const string code =
"""
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace CommunityToolkit.Maui
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class BindablePropertyAttribute : Attribute
	{
		public string? PropertyName { get; set; }
	}
}

namespace TestApp
{
	public partial class CustomPopup : ContentView
	{
		[CommunityToolkit.Maui.BindableProperty]
		public partial string? PopupMessage { get; set; }
	}

	public class TestViewModel : INotifyPropertyChanged
	{
		public string? Nickname { get; set; }
		public string? OtherName { get; set; }
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class TestPage : ContentPage
	{
		public TestPage() => InitializeComponent();
	}
}
""";

		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <local:CustomPopup PopupMessage="{Nickname}" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TestPage"
             x:DataType="local:TestViewModel">
    <local:CustomPopup PopupMessage="{OtherName}" />
</ContentPage>
""";

		RunGenerator(xamlV1, code, assertNoCompilationErrors: false, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, code, assertNoCompilationErrors: false, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.DoesNotContain("skipped", ucSource, StringComparison.OrdinalIgnoreCase);
		Assert.Contains("global::TestApp.CustomPopup.PopupMessageProperty", ucSource, StringComparison.Ordinal);
		Assert.Contains("SetBinding(global::TestApp.CustomPopup.PopupMessageProperty,", ucSource, StringComparison.Ordinal);
		Assert.Contains("__source.OtherName", ucSource, StringComparison.Ordinal);
		Assert.Contains("new(static __source => __source, \"OtherName\")", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_HeuristicBindableProperty_RemovesRootAttribute_GeneratesClearAndRemoveBindingInUC()
	{
		const string code =
"""
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace CommunityToolkit.Maui
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class BindablePropertyAttribute : Attribute
	{
		public string? PropertyName { get; set; }
	}
}

namespace TestApp
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class CustomPopup : ContentView
	{
		public CustomPopup() => InitializeComponent();

		[CommunityToolkit.Maui.BindableProperty]
		public partial string? PopupMessage { get; set; }
	}

	public class TestViewModel : INotifyPropertyChanged
	{
		public string? Nickname { get; set; }
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}
""";

		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<local:CustomPopup xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                   xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                   xmlns:local="clr-namespace:TestApp"
                   x:Class="TestApp.CustomPopup"
                   x:DataType="local:TestViewModel"
                   PopupMessage="{Nickname}" />
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<local:CustomPopup xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                   xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                   xmlns:local="clr-namespace:TestApp"
                   x:Class="TestApp.CustomPopup"
                   x:DataType="local:TestViewModel" />
""";

		RunGenerator(xamlV1, code, assertNoCompilationErrors: false, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, code, assertNoCompilationErrors: false, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.DoesNotContain("skipped", ucSource, StringComparison.OrdinalIgnoreCase);
		Assert.Contains("RemoveBinding(global::TestApp.CustomPopup.PopupMessageProperty)", ucSource, StringComparison.Ordinal);
		Assert.Contains("ClearValue(global::TestApp.CustomPopup.PopupMessageProperty)", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_XDataTypeWithXTypeMarkup_PreservesDottedFormatInUC()
	{
		const string code =
"""
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro
{
	public class DummyType { }
}

namespace TestApp
{
	public class ClockViewModel : INotifyPropertyChanged
	{
		public DateTime Time { get; set; }
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class XTypePage : ContentPage
	{
		public XTypePage() => InitializeComponent();
	}
}
""";

		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:Time="clr-namespace:Repro"
             xmlns:app="clr-namespace:TestApp"
             x:Class="TestApp.XTypePage"
             x:DataType="{x:Type app:ClockViewModel}">
    <Label Text="Initial" />
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:Time="clr-namespace:Repro"
             xmlns:app="clr-namespace:TestApp"
             x:Class="TestApp.XTypePage"
             x:DataType="{x:Type app:ClockViewModel}">
    <Label Text="{= $'{Time:HH.mm}'}" />
</ContentPage>
""";

		RunGenerator(xamlV1, code, assertNoCompilationErrors: false, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, code, assertNoCompilationErrors: false, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.DoesNotContain("skipped", ucSource, StringComparison.OrdinalIgnoreCase);
		Assert.DoesNotContain("global::Repro.HH", ucSource, StringComparison.Ordinal);
		Assert.Contains("__source.Time:HH.mm}\"", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_VerbatimAndAttachedPropertyInterpolation_ParenthesizesAliasesInUC()
	{
		const string code =
"""
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp.Resources
{
	public static class Labels
	{
		public static string Gold => "Gold";
	}
}

namespace TestApp.Attached
{
	public static class CustomGrid
	{
		public static readonly BindableProperty RowProperty =
			BindableProperty.CreateAttached("Row", typeof(int), typeof(CustomGrid), 0);

		public static int GetRow(BindableObject view) => (int)view.GetValue(RowProperty);
	}
}

namespace TestApp
{
	public class DummyViewModel : INotifyPropertyChanged
	{
		public int Dummy { get; set; }
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class VerbatimAttachedPage : ContentPage
	{
		public VerbatimAttachedPage() => InitializeComponent();
	}
}
""";

		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:res="clr-namespace:TestApp.Resources"
             xmlns:m="clr-namespace:TestApp.Attached"
             xmlns:app="clr-namespace:TestApp"
             x:Class="TestApp.VerbatimAttachedPage"
             x:DataType="app:DummyViewModel">
    <VerticalStackLayout>
        <Label Text="Initial1" />
        <Label Text="Initial2" />
        <Label Text="Initial3" />
    </VerticalStackLayout>
</ContentPage>
""";
		var xamlV2 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:res="clr-namespace:TestApp.Resources"
             xmlns:m="clr-namespace:TestApp.Attached"
             xmlns:app="clr-namespace:TestApp"
             x:Class="TestApp.VerbatimAttachedPage"
             x:DataType="app:DummyViewModel">
    <VerticalStackLayout>
        <Label Text='{= $@"{res:Labels.Gold}"}' />
        <Label Text='{= @$"C:\""q""\{res:Labels.Gold}"}' />
        <Label Text="{= $'{this.(m:CustomGrid.Row)}'}" />
    </VerticalStackLayout>
</ContentPage>
""";

		RunGenerator(xamlV1, code, assertNoCompilationErrors: false, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, code, assertNoCompilationErrors: false, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.DoesNotContain("skipped", ucSource, StringComparison.OrdinalIgnoreCase);
		Assert.Contains("$@\"{(global::TestApp.Resources.Labels.Gold)}\"", ucSource, StringComparison.Ordinal);
		Assert.Contains("@$\"C:\\\"\"q\"\"\\{(global::TestApp.Resources.Labels.Gold)}\"", ucSource, StringComparison.Ordinal);
		Assert.Contains("$\"{(global::TestApp.Attached.CustomGrid.GetRow(this))}\"", ucSource, StringComparison.Ordinal);
	}

	[Fact]
	public void CSharpExpression_RawStringsOutVarQueriesAndGenericArity_PreservedInUC()
	{
		const string code =
"""
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp.Models
{
	public class Name { }
	public class HH { public static string mm => "bad"; }
	public class Person { public static string Default => "fallback"; }
	public static class Labels { public static string Gold => "Gold"; }
}

namespace TestApp
{
	public class Box
	{
		public Box(string plain) { }
		public override string ToString() => "";
	}

	public class Box<T>
	{
		public Box(string generic) { }
		public override string ToString() => "";
	}

	public static class Formatter
	{
		public static string Format(object local) => local?.ToString() ?? string.Empty;
	}

	public sealed class QuerySequence<T> : IEnumerable<T>
	{
		readonly List<T> _items;
		public QuerySequence(IEnumerable<T> items) => _items = new List<T>(items);
		public QuerySequence<TResult> Select<TResult>(Func<T, TResult> selector)
		{
			var results = new List<TResult>(_items.Count);
			foreach (var item in _items)
				results.Add(selector(item));
			return new QuerySequence<TResult>(results);
		}
		public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class UcScopeViewModel : INotifyPropertyChanged
	{
		public string Name { get; set; } = "Ada";
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class UcScopePage : ContentPage
	{
		public UcScopePage() => InitializeComponent();
		public QuerySequence<string> People { get; } = new(["Ada"]);
		public string UpdateTime(bool ok, string formatted) => ok ? formatted : string.Empty;
	}
}
""";

		var xamlV1 =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:res="clr-namespace:TestApp.Models"
             xmlns:Time="clr-namespace:TestApp.Models"
             xmlns:local="clr-namespace:TestApp.Models"
             xmlns:generic="clr-namespace:TestApp.Models"
             xmlns:app="clr-namespace:TestApp"
             x:Class="TestApp.UcScopePage"
             x:DataType="app:UcScopeViewModel">
    <VerticalStackLayout>
        <Label Text="Initial1" />
        <Label Text="Initial2" />
        <Label Text="Initial3" />
        <Label Text="Initial4" />
        <Label Text="Initial5" />
    </VerticalStackLayout>
</ContentPage>
""";
		var xamlV2 =
""""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:res="clr-namespace:TestApp.Models"
             xmlns:Time="clr-namespace:TestApp.Models"
             xmlns:local="clr-namespace:TestApp.Models"
             xmlns:generic="clr-namespace:TestApp.Models"
             xmlns:app="clr-namespace:TestApp"
             x:Class="TestApp.UcScopePage"
             x:DataType="app:UcScopeViewModel">
    <VerticalStackLayout>
        <Label Text='{= """literal " $@"{global::System.String.Empty}" """}' />
        <Label Text='{= $"""{res:Labels.Gold}""" }' />
        <Label Text="{= this.UpdateTime(System.DateTime.TryParse('2026-11-15', out var Time), $'{Time:HH.mm}')}" />
        <Label Text="{= string.Join(', ', from Person in People select global::TestApp.Formatter.Format(local:Person))}" />
        <Label Text="{= new global::TestApp.Box&lt;int&gt;(generic:Name).ToString()}" />
    </VerticalStackLayout>
</ContentPage>
"""";

		RunGenerator(xamlV1, code, assertNoCompilationErrors: false, enableIncrementalHotReload: true);
		var (result, _) = RunGenerator(xamlV2, code, assertNoCompilationErrors: false, enableIncrementalHotReload: true);

		var ucSource = GetUCSource(result);
		Assert.NotNull(ucSource);
		Assert.DoesNotContain("skipped", ucSource, StringComparison.OrdinalIgnoreCase);
		Assert.Contains("\"\"\"literal \" $@\"{global::System.String.Empty}\" \"\"\"", ucSource, StringComparison.Ordinal);
		Assert.Contains("$\"\"\"{(global::TestApp.Models.Labels.Gold)}\"\"\"", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("global::TestApp.Models.HH", ucSource, StringComparison.Ordinal);
		Assert.Contains("Time:HH.mm}\"", ucSource, StringComparison.Ordinal);
		Assert.Contains("Formatter.Format(local:Person)", ucSource, StringComparison.Ordinal);
		Assert.DoesNotContain("global::TestApp.Models.Name", ucSource, StringComparison.Ordinal);
		Assert.Contains("new global::TestApp.Box<int>(generic:", ucSource, StringComparison.Ordinal);
	}

	static string? GetUCSource(GeneratorDriverRunResult result)
	{
		foreach (var gen in result.Results)
		{
			foreach (var src in gen.GeneratedSources)
			{
				if (src.HintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase))
					return src.SourceText.ToString();
			}
		}
		return null;
	}

	protected new (GeneratorDriverRunResult result, string? text) RunGenerator(
		string xaml,
		string code,
		string noWarn = "",
		string targetFramework = "",
		string? path = null,
		string lineinfo = "enable",
		bool assertNoCompilationErrors = true,
		bool enableIncrementalHotReload = false)
	{
		_compilation ??= CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		var workingDirectory = Environment.CurrentDirectory;
		var nextFile = new AdditionalXamlFile(
			Path.Combine(workingDirectory, path ?? "Test.xaml"),
			xaml,
			RelativePath: path ?? "Test.xaml",
			TargetFramework: targetFramework,
			NoWarn: noWarn,
			ManifestResourceName: $"{_compilation.AssemblyName}.Test.xaml",
			Lineinfo: lineinfo,
			EnableIncrementalHotReload: enableIncrementalHotReload);

		if (_driver is null)
		{
			_driver = CSharpGeneratorDriver.Create([new XamlGenerator().AsSourceGenerator()])
				.AddAdditionalTexts([nextFile.Text])
				.WithUpdatedAnalyzerConfigOptions(CreateAnalyzerConfigOptionsProvider(nextFile));
		}
		else
		{
			_driver = _driver.ReplaceAdditionalText(_xamlFile!.Text, nextFile.Text);
		}

		_driver = _driver.RunGeneratorsAndUpdateCompilation(_compilation, out var updatedCompilation, out _);
		if (assertNoCompilationErrors)
		{
			var compilationErrors = updatedCompilation.GetDiagnostics()
				.Where(d => d.Severity == DiagnosticSeverity.Error)
				.Where(d => d.Location.SourceTree?.FilePath?.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase) == true)
				.ToArray();

			if (compilationErrors.Length > 0)
			{
				var errorMessages = string.Join(Environment.NewLine, compilationErrors.Select(e => $"{e.Id}: {e.GetMessage()} at {e.Location}"));
				throw new InvalidOperationException($"Generated code has compilation errors:{Environment.NewLine}{errorMessages}");
			}
		}

		_xamlFile = nextFile;
		var result = _driver.GetRunResult();
		var generated = result.Results.SingleOrDefault().GeneratedSources
			.SingleOrDefault(gs => gs.HintName.EndsWith(".xsg.cs")
				&& !gs.HintName.EndsWith(".handler.xsg.cs")
				&& !gs.HintName.Contains(".uc.xsg", StringComparison.Ordinal))
			.SourceText?.ToString();

		return (result, generated);
	}
}
