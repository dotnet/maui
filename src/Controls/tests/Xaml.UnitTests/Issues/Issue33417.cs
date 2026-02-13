using System;
using System.Linq;
using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Issue33417
{
	const string InvalidBindingXaml = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
					 xmlns:local="clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests"
		             x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.Maui33417_InvalidBinding"
		             x:DataType="local:Foo">
		    <VerticalStackLayout>
		        <Label Text="{Binding NonExistentProperty}" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	[Fact]
	public void Issue33417_Test()
	{
		var result = CreateMauiCompilation()
			.RunMauiSourceGenerator(new AdditionalXamlFile("Maui33417_InvalidBinding.xaml", InvalidBindingXaml));
		Assert.NotEmpty(result.Diagnostics);
		var hasTypeError = result.Diagnostics.Any(d => 
			d.Id == "MAUIX2000" && d.GetMessage().Contains("Foo", StringComparison.Ordinal));
		Assert.True(hasTypeError,
			$"Should report type resolution error (MAUIX2000) for 'local:Foo'. Found diagnostics: {string.Join(", ", result.Diagnostics.Select(d => $"{d.Id}: {d.GetMessage()}"))}");
	}
}
