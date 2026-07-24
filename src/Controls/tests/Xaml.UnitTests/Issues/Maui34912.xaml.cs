global using System;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34912 : ContentPage
{
	public Maui34912() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Fact]
		public void StaticTypeInvocationRespectsGlobalUsings()
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
global using System;
""", "GlobalUsings.cs")
				.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui34912ViewModel
{
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class Maui34912 : ContentPage
{
	public Maui34912() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(
					new AdditionalXamlFile(
						"Issues/Maui34912.sgen.xaml",
"""
<?xml version="1.0" encoding="UTF-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests"
             x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.Maui34912"
             x:DataType="local:Maui34912ViewModel">
    <VerticalStackLayout>
        <Label x:Name="nowLabel" Text="{DateTime.Now}" />
        <Label x:Name="utcNowLabel" Text="{System.DateTime.UtcNow}" />
    </VerticalStackLayout>
</ContentPage>
"""));

			Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2009");
			Assert.Empty(result.Diagnostics);

			var page = new Maui34912(XamlInflator.SourceGen);
			Assert.False(string.IsNullOrWhiteSpace(page.nowLabel.Text));
			Assert.False(string.IsNullOrWhiteSpace(page.utcNowLabel.Text));
		}
	}
}

public class Maui34912ViewModel
{
}
