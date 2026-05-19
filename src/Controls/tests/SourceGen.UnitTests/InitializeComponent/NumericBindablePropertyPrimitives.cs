using System;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class NumericBindablePropertyPrimitives : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void BindableProperty_PrimitiveSmallIntegers_EmitTypedCastsAndBoundaries()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Test"
	x:Class="Test.TestPage">
	<VerticalStackLayout>
		<local:NumericView SByteValue="-128" ByteValue="255" ShortValue="0" UShortValue="65535" />
	</VerticalStackLayout>
</ContentPage>
""";

		var code =
"""
using System;
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

public class NumericView : BindableObject
{
	public static readonly BindableProperty SByteValueProperty =
		BindableProperty.Create(nameof(SByteValue), typeof(sbyte), typeof(NumericView), default(sbyte));

	public static readonly BindableProperty ByteValueProperty =
		BindableProperty.Create(nameof(ByteValue), typeof(byte), typeof(NumericView), default(byte));

	public static readonly BindableProperty ShortValueProperty =
		BindableProperty.Create(nameof(ShortValue), typeof(short), typeof(NumericView), default(short));

	public static readonly BindableProperty UShortValueProperty =
		BindableProperty.Create(nameof(UShortValue), typeof(ushort), typeof(NumericView), default(ushort));

	public sbyte SByteValue
	{
		get => (sbyte)GetValue(SByteValueProperty);
		set => SetValue(SByteValueProperty, value);
	}

	public byte ByteValue
	{
		get => (byte)GetValue(ByteValueProperty);
		set => SetValue(ByteValueProperty, value);
	}

	public short ShortValue
	{
		get => (short)GetValue(ShortValueProperty);
		set => SetValue(ShortValueProperty, value);
	}

	public ushort UShortValue
	{
		get => (ushort)GetValue(UShortValueProperty);
		set => SetValue(UShortValueProperty, value);
	}
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		Assert.Empty(result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);

		Assert.Contains("(sbyte)-128", generated, StringComparison.Ordinal);
		Assert.Contains("(byte)255", generated, StringComparison.Ordinal);
		Assert.Contains("(short)0", generated, StringComparison.Ordinal);
		Assert.Contains("(ushort)65535", generated, StringComparison.Ordinal);
	}
}
