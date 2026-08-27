using System;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class Maui34912Tests : SourceGenXamlInitializeComponentTestBase
{
	[Theory]
	[InlineData("{DateTime.Now}", "DateTime.Now")]
	[InlineData("{DateTime.Now.ToString()}", "DateTime.Now.ToString()")]
	[InlineData("{System.DateTime.UtcNow}", "System.DateTime.UtcNow")]
	[InlineData("{string.Empty}", "string.Empty")]
	[InlineData("{int.MaxValue}", "int.MaxValue")]
	public void StaticExpressionCompilesWithoutBinding(string expression, string generatedExpression)
	{
		var (result, generated) = RunExpression(expression, "global using System;");

		Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2009");
		Assert.NotNull(generated);
		Assert.Contains(generatedExpression, generated, StringComparison.Ordinal);
		Assert.DoesNotContain("TypedBinding", generated, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("global using Dt = System.DateTime;", "{Dt.Now}", "Dt.Now")]
	[InlineData("global using Sys = System;", "{Sys.DateTime.UtcNow}", "Sys.DateTime.UtcNow")]
	public void GlobalUsingAliasCompilesWithoutBinding(string globalUsing, string expression, string generatedExpression)
	{
		var (result, generated) = RunExpression(expression, globalUsing);

		Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2009");
		Assert.NotNull(generated);
		Assert.Contains(generatedExpression, generated, StringComparison.Ordinal);
		Assert.DoesNotContain("TypedBinding", generated, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("{Convert.ToString(Count)}", "__source.Convert")]
	[InlineData("{Math.Max(Count, 1)}", "__source.Math")]
	public void StaticCallWithViewModelOperandGeneratesBinding(string expression, string invalidStaticSource)
	{
		var (result, generated) = RunExpression(
			expression,
			"global using System;",
			"public int Count { get; set; }");

		Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2009");
		Assert.Contains("TypedBinding", generated, StringComparison.Ordinal);
		Assert.Contains("__source.Count", generated, StringComparison.Ordinal);
		Assert.DoesNotContain(invalidStaticSource, generated, StringComparison.Ordinal);
	}

	[Fact]
	public void ViewModelMemberShadowingStaticTypeRemainsBinding()
	{
		var (result, generated) = RunExpression(
			"{Math.ToString()}",
			"global using System;",
			"public int Math { get; set; }");

		Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2011");
		Assert.NotNull(generated);
		Assert.Contains("TypedBinding", generated, StringComparison.Ordinal);
		Assert.Contains("__source.Math.ToString()", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void MemberInheritedFromInterfaceGeneratesBinding()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.InterfaceExpressionPage"
             x:DataType="local:IChildViewModel">
    <Label Text="{Name.ToUpperInvariant()}" />
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public interface IBaseViewModel
{
	string Name { get; }
}

public interface IChildViewModel : IBaseViewModel
{
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class InterfaceExpressionPage : ContentPage
{
	public InterfaceExpressionPage() => InitializeComponent();
}
""";

		var (result, generated) = RunGenerator(xaml, codeBehind);

		Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2009");
		Assert.NotNull(generated);
		Assert.Contains("__source.Name.ToUpperInvariant()", generated, StringComparison.Ordinal);
		Assert.Contains("\"Name\"", generated, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("{DateTime}")]
	[InlineData("{DateTimeProperty}")]
	public void BareTypeLikeIdentifierStillReportsMemberNotFound(string expression)
	{
		var (result, _) = RunExpression(expression, "global using System;", assertNoCompilationErrors: false);

		Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2009");
	}

	[Fact]
	public void FileScopedUsingDoesNotApplyToGeneratedExpression()
	{
		var (result, _) = RunExpression("{DateTime.Now}", "using System;", assertNoCompilationErrors: false);

		Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2009");
	}

	private (Microsoft.CodeAnalysis.GeneratorDriverRunResult Result, string? Generated) RunExpression(
		string expression,
		string usings,
		string viewModelMembers = "",
		bool assertNoCompilationErrors = true)
	{
		var xaml =
$$"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.StaticExpressionPage"
             x:DataType="local:StaticExpressionViewModel">
    <Label Text="{{expression}}" />
</ContentPage>
""";

		var codeBehind =
$$"""
{{usings}}
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class StaticExpressionViewModel
{
	{{viewModelMembers}}
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class StaticExpressionPage : ContentPage
{
	public StaticExpressionPage() => InitializeComponent();
}
""";

		return RunGenerator(xaml, codeBehind, assertNoCompilationErrors: assertNoCompilationErrors);
	}
}
