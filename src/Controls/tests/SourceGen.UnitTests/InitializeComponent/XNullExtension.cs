using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class XNullExtension : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void XNullGeneratesNullLiteral()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Test.TestPage">
    <ContentPage.Resources>
        <x:Null x:Key="nullValue" />
    </ContentPage.Resources>
</ContentPage>
""";

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test
{
	[XamlProcessing(XamlInflator.SourceGen)]
	partial class TestPage : ContentPage
	{
		public TestPage()
		{
			InitializeComponent();
		}
	}
}
""";

		var (result, generated) = RunGenerator(xaml, code, assertNoCompilationErrors: true);
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);
		
		// Verify that the generated code contains "(object)null!" literal instead of NullExtension instantiation
		Assert.Contains("(object)null!", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("new global::Microsoft.Maui.Controls.Xaml.NullExtension", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void XNullInDataTriggerValue()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Test.TestPage">
    <Entry>
        <Entry.Triggers>
            <DataTrigger TargetType="Entry" Binding="{Binding Text}" Value="{x:Null}">
                <Setter Property="TextColor" Value="Red" />
            </DataTrigger>
        </Entry.Triggers>
    </Entry>
</ContentPage>
""";

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test
{
	[XamlProcessing(XamlInflator.SourceGen)]
	partial class TestPage : ContentPage
	{
		public TestPage()
		{
			InitializeComponent();
		}
	}
}
""";

		var (result, generated) = RunGenerator(xaml, code, assertNoCompilationErrors: true);
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);
		
		// Verify that the generated code contains "(object)null!" literal
		Assert.Contains("(object)null!", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("new global::Microsoft.Maui.Controls.Xaml.NullExtension", generated, StringComparison.Ordinal);
	}
}
