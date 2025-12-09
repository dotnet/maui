using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class WebViewBindingTest : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void WebViewSourceBindingCompilesCorrectly()
	{
		var xaml = 
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Test"
             x:Class="Test.TestPage"
             x:DataType="local:TestViewModel">
    <WebView x:Name="WebViewControl"
             Source="{Binding Source}"
             AutomationId="WebViewControl"/>
</ContentPage>
""";

		var code = 
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test
{
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
		public WebViewSource Source { get; set; }
	}
}
""";

		var (result, generated) = RunGenerator(xaml, code);
		
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		
		// Should compile as property binding to Source property, not as self-binding
		Assert.Contains("TypedBinding<global::Test.TestViewModel, global::Microsoft.Maui.Controls.WebViewSource>", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("TypedBinding<global::Test.TestViewModel, global::Test.TestViewModel>", generated, StringComparison.Ordinal);
		
		// Should have getter that accesses .Source property
		Assert.Contains("source.Source", generated, StringComparison.Ordinal);
	}
}
