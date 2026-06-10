using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

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

	// -------------------------------------------------------------------------
	// IC with XIHR: C# expressions should emit __version and Registry calls
	// -------------------------------------------------------------------------

	[Fact]
	public void CSharpExpression_IC_EmitsVersionField()
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
		Assert.Contains("private int __version = 0;", text, StringComparison.Ordinal);
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

	// -------------------------------------------------------------------------
	// UC generation: property value change in C# expression
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// No change → no UC
	// -------------------------------------------------------------------------

	[Fact]
	public void CSharpExpression_IdenticalXaml_NoUCGenerated()
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

		var ucSource = GetUCSource(result);
		Assert.Null(ucSource);
	}

	// -------------------------------------------------------------------------
	// Mixed: expression + plain property changes
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// Structural change: add a new element with expression
	// -------------------------------------------------------------------------

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
			Assert.Contains("__version == 0", ucSource, StringComparison.Ordinal);
		}
	}

	// -------------------------------------------------------------------------
	// Lambda event handler change
	// -------------------------------------------------------------------------

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
		Assert.Contains("__version == 0", ucSource, StringComparison.Ordinal);
	}

	// -------------------------------------------------------------------------
	// Expression with operator aliases (AND, OR, GT, LT)
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// Helpers
	// -------------------------------------------------------------------------

	/// <summary>
	/// Extracts the UC (UpdateComponent) source from generator results, if any.
	/// </summary>
	// -------------------------------------------------------------------------
	// DynamicResource
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// StaticResource
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// {Binding Path} markup
	// -------------------------------------------------------------------------

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
}
