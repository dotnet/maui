using System;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class OnIdiomTypeInference : SourceGenXamlInitializeComponentTestBase
{
	const string Code = """
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.ComponentModel;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class NumericView : View
{
	public float FloatValue { get; set; }
	public double DoubleValue { get; set; }
	[TypeConverter(typeof(CustomDoubleConverter))]
	public double ConvertedDoubleValue { get; set; }
	public decimal DecimalValue { get; set; }
	public uint UIntValue { get; set; }
	public long LongValue { get; set; }
	public ulong ULongValue { get; set; }
}

public class CustomDoubleConverter : TypeConverter
{
}

public class CustomValue
{
	public static implicit operator CustomValue(string value) => new();
}

public class CustomValueView : View
{
	public CustomValue Value { get; set; } = new();
}
""";

	static string GetXaml(string content) => $$"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			 xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			 xmlns:local="clr-namespace:Test"
			 x:Class="Test.TestPage">
	{{content}}
</ContentPage>
""";

	[Theory]
	[InlineData("<Label Text=\"{OnIdiom Default=Default, Phone=Phone}\" />", "OnIdiom<string>")]
	[InlineData("<Grid Margin=\"{OnIdiom '1,2,3,4', Desktop='5,6,7,8'}\" />", "OnIdiom<global::Microsoft.Maui.Thickness>")]
	[InlineData("<Grid Grid.Row=\"{OnIdiom 1, Desktop=2}\" />", "OnIdiom<int>")]
	public void InfersTargetPropertyType(string content, string expectedType)
	{
		var (result, generated) = RunGenerator(GetXaml(content), Code);

		Assert.False(result.Diagnostics.Any());
		Assert.Contains($"new global::Microsoft.Maui.Controls.{expectedType}", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("OnIdiomExtension", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void InfersSetterPropertyType()
	{
		var xaml = """
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			 xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			 x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style TargetType="Label">
			<Setter Property="FontSize" Value="{OnIdiom 18, Desktop=24}" />
		</Style>
	</ContentPage.Resources>
</ContentPage>
""";

		var (result, generated) = RunGenerator(xaml, Code);

		Assert.False(result.Diagnostics.Any());
		Assert.Contains("new global::Microsoft.Maui.Controls.OnIdiom<double>", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("OnIdiomExtension", generated, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("FloatValue", "float", "10", "11.3", "10F", "11.3F")]
	[InlineData("FloatValue", "float", "10", "11", "10F", "11F")]
	[InlineData("DoubleValue", "double", "10", "11.3", "10D", "11.3D")]
	[InlineData("DoubleValue", "double", "10", "11", "10D", "11D")]
	[InlineData("DecimalValue", "decimal", "10", "11.3", "10M", "11.3M")]
	[InlineData("DecimalValue", "decimal", "10", "11", "10M", "11M")]
	[InlineData("UIntValue", "uint", "10", "11", "10U", "11U")]
	[InlineData("LongValue", "long", "10", "11", "10L", "11L")]
	[InlineData("ULongValue", "ulong", "10", "11", "10UL", "11UL")]
	public void InfersNumericTypeWithLiteralSuffixFromTargetProperty(
		string propertyName,
		string expectedType,
		string defaultValue,
		string phoneValue,
		string expectedDefaultLiteral,
		string expectedPhoneLiteral)
	{
		var xaml = $$"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			 xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			 xmlns:local="clr-namespace:Test"
			 x:Class="Test.TestPage">
	<local:NumericView {{propertyName}}="{OnIdiom Default={{defaultValue}}, Phone={{phoneValue}}}" />
</ContentPage>
""";

		var (result, generated) = RunGenerator(xaml, Code);

		Assert.False(result.Diagnostics.Any());
		Assert.Contains($"new global::Microsoft.Maui.Controls.OnIdiom<{expectedType}>", generated, StringComparison.Ordinal);
		Assert.Contains($"onIdiom.Default = {expectedDefaultLiteral};", generated, StringComparison.Ordinal);
		Assert.Contains($"onIdiom.Phone = {expectedPhoneLiteral};", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("OnIdiomExtension", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void InfersTypeAfterSimplifyingOnPlatform()
	{
		var xaml = """
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			 xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			 x:Class="Test.TestPage">
	<Label FontSize="{OnPlatform Android={OnIdiom 18, Desktop=24}, iOS=20}" />
</ContentPage>
""";

		var (result, generated) = RunGenerator(xaml, Code, targetFramework: "net11.0-android");

		Assert.False(result.Diagnostics.Any());
		Assert.Contains("new global::Microsoft.Maui.Controls.OnIdiom<double>", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("OnIdiomExtension", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("OnPlatformExtension", generated, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("<Label Text=\"{OnIdiom Phone=Phone}\" />")]
	[InlineData("<Label Text=\"{OnIdiom Default={Binding Name}, Phone=Phone}\" />")]
	public void KeepsRuntimeExtensionWhenInferenceIsNotBehaviorPreserving(string content)
	{
		var (result, generated) = RunGenerator(GetXaml(content), Code);

		Assert.False(result.Diagnostics.Any());
		Assert.Contains("OnIdiomExtension", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("new global::Microsoft.Maui.Controls.OnIdiom<", generated, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("<Label FontSize=\"{OnIdiom Medium, Desktop=Large}\" />")]
	[InlineData("<Label FontSize=\"{OnIdiom Default='', Desktop=20}\" />")]
	[InlineData("<Button CommandParameter=\"{OnIdiom Default=Default, Desktop=Desktop}\" />")]
	[InlineData("<local:NumericView ConvertedDoubleValue=\"{OnIdiom Default=10, Desktop=20}\" />")]
	[InlineData("<local:CustomValueView Value=\"{OnIdiom Default=Default, Desktop=Desktop}\" />")]
	public void KeepsRuntimeExtensionWhenTargetNeedsRuntimeBehavior(string content)
	{
		var (result, generated) = RunGenerator(GetXaml(content), Code);

		Assert.False(result.Diagnostics.Any());
		Assert.Contains("OnIdiomExtension", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("new global::Microsoft.Maui.Controls.OnIdiom<", generated, StringComparison.Ordinal);
	}

}
