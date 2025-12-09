using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class RelativeSourceBindings : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void FindAncestorBindingContextGeneratesValidCode()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage">
	<StackLayout>
		<StackLayout.BindingContext>
			<test:TestViewModel />
		</StackLayout.BindingContext>
		<StackLayout BindingContext="{x:Null}">
			<Label Text="{Binding Path=Name, Source={RelativeSource AncestorType={x:Type test:TestViewModel}, Mode=FindAncestorBindingContext}}" />
		</StackLayout>
	</StackLayout>
</ContentPage>
""";

		var code =
"""
#nullable enable
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

public class TestViewModel
{
	public string Name => "TestName";
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no diagnostics
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error), 
			$"Generator produced errors: {string.Join(", ", result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Select(d => d.GetMessage()))}");

		// Verify generated code exists
		Assert.NotNull(generated);

		// Verify that XamlTypeResolver is NOT used (would produce trimming and AOT warnings)
		Assert.DoesNotContain("XamlTypeResolver", generated, StringComparison.Ordinal);

		// Verify RelativeBindingSource is generated correctly
		Assert.Contains("RelativeBindingSource", generated, StringComparison.Ordinal);
		Assert.Contains("FindAncestorBindingContext", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void FindAncestorGeneratesValidCode()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<StackLayout x:Name="Stack1" StyleId="TestStack">
		<Label Text="{Binding Path=StyleId, Source={RelativeSource AncestorType={x:Type StackLayout}}}" />
	</StackLayout>
</ContentPage>
""";

		var code =
"""
#nullable enable
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

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no diagnostics
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error),
			$"Generator produced errors: {string.Join(", ", result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Select(d => d.GetMessage()))}");

		// Verify generated code exists
		Assert.NotNull(generated);

		// Verify that XamlTypeResolver is NOT used (would produce trimming and AOT warnings)
		Assert.DoesNotContain("XamlTypeResolver", generated, StringComparison.Ordinal);

		// Verify RelativeBindingSource is generated correctly
		Assert.Contains("RelativeBindingSource", generated, StringComparison.Ordinal);
		Assert.Contains("FindAncestor", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void SelfBindingGeneratesValidCode()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label StyleId="SelfTest" Text="{Binding Path=StyleId, Source={RelativeSource Self}}" />
</ContentPage>
""";

		var code =
"""
#nullable enable
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

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no diagnostics
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error),
			$"Generator produced errors: {string.Join(", ", result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Select(d => d.GetMessage()))}");

		// Verify generated code exists
		Assert.NotNull(generated);

		// Verify that XamlTypeResolver is NOT used (would produce trimming and AOT warnings)
		Assert.DoesNotContain("XamlTypeResolver", generated, StringComparison.Ordinal);

		// Verify RelativeBindingSource.Self is generated correctly
		Assert.Contains("RelativeBindingSource.Self", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void TemplatedParentBindingGeneratesValidCode()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<ControlTemplate x:Key="TestTemplate">
			<Label Text="{Binding Source={RelativeSource TemplatedParent}, Path=StyleId}" />
		</ControlTemplate>
	</ContentPage.Resources>
</ContentPage>
""";

		var code =
"""
#nullable enable
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

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no diagnostics
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error),
			$"Generator produced errors: {string.Join(", ", result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Select(d => d.GetMessage()))}");

		// Verify generated code exists
		Assert.NotNull(generated);

		// Verify that XamlTypeResolver is NOT used (would produce trimming and AOT warnings)
		Assert.DoesNotContain("XamlTypeResolver", generated, StringComparison.Ordinal);

		// Verify RelativeBindingSource.TemplatedParent is generated correctly
		Assert.Contains("RelativeBindingSource.TemplatedParent", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void ImplicitModeInferenceGeneratesValidCode()
	{
		// When AncestorType is a non-Element type and Mode is not specified,
		// FindAncestorBindingContext should be inferred
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage">
	<StackLayout>
		<StackLayout.BindingContext>
			<test:TestViewModel />
		</StackLayout.BindingContext>
		<Label Text="{Binding Source={RelativeSource AncestorType={x:Type test:TestViewModel}}, Path=Name}" />
	</StackLayout>
</ContentPage>
""";

		var code =
"""
#nullable enable
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

public class TestViewModel
{
	public string Name => "TestName";
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no diagnostics
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error),
			$"Generator produced errors: {string.Join(", ", result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Select(d => d.GetMessage()))}");

		// Verify generated code exists
		Assert.NotNull(generated);

		// Verify that XamlTypeResolver is NOT used (would produce trimming and AOT warnings)
		Assert.DoesNotContain("XamlTypeResolver", generated, StringComparison.Ordinal);

		// Verify RelativeBindingSource is generated with FindAncestorBindingContext (inferred from non-Element type)
		Assert.Contains("RelativeBindingSource", generated, StringComparison.Ordinal);
		Assert.Contains("FindAncestorBindingContext", generated, StringComparison.Ordinal);
	}
}
