using System;
using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34712 : ContentPage
{
	public Maui34712() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Fact]
		public void XCodeSucceedsWithSourceGen()
		{
			var page = new Maui34712(XamlInflator.SourceGen);
			Assert.NotNull(page);
			Assert.Equal("Hello from x:Code", page.GetMessage());
			// Verify that using System.Globalization was promoted correctly
			Assert.NotNull(page.GetFormattedDate());
		}

		[Fact]
		public void XCodeFailsAtRuntime()
		{
			// x:Code requires SourceGen; runtime inflation is not supported
			Assert.Throws<NotSupportedException>(() => new Maui34712(XamlInflator.Runtime));
		}

		[Fact]
		public void XCodeFailsWithXamlC()
		{
			// x:Code requires SourceGen; XamlC compilation is not supported
			Assert.Throws<NotSupportedException>(() => new Maui34712(XamlInflator.XamlC));
		}

		[Fact]
		public void XCodeSourceGenProducesNoDiagnostics()
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class Maui34712 : ContentPage
{
	public Maui34712() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(
					new AdditionalXamlFile(
						"Issues/Maui34712.sgen.xaml",
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.Maui34712">
    <x:Code><![CDATA[
        using System;
        using System.Globalization;

        public string GetMessage() => "Hello from x:Code";

        public string GetFormattedDate()
            => DateTime.Now.ToString("d", CultureInfo.InvariantCulture);
    ]]></x:Code>
    <Label x:Name="testLabel" Text="Test" />
</ContentPage>
"""));
			Assert.Empty(result.Diagnostics);
		}
	}
}
