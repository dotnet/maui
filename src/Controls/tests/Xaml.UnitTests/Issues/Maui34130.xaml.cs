using System;
using System.ComponentModel;
using System.Globalization;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

#nullable enable

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34130 : ContentPage
{
	public Maui34130()
	{
		InitializeComponent();
		BindingContext = new Maui34130ViewModel();
	}

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ReproducesSourceGenNullabilityDiagnostics(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var xaml =
"""
#nullable enable
using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34130SourceGenRepro : ContentPage
{
	public Maui34130SourceGenRepro()
	{
		InitializeComponent();
		BindingContext = new Maui34130SourceGenReproViewModel();
	}
}
		
[TypeConverter(typeof(Maui34130SourceGenReproSizeConverter))]
public readonly struct Maui34130SourceGenReproSize(double value)
{
	public double Value { get; } = value;
}

public class Maui34130SourceGenReproSizeConverter : TypeConverter, IExtendedTypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		=> value is string s && double.TryParse(s, NumberStyles.Any, culture, out double d)
			? new Maui34130SourceGenReproSize(d)
			: base.ConvertFrom(context, culture, value)!;

	public object ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
		=> double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double d)
			? new Maui34130SourceGenReproSize(d)
			: new Maui34130SourceGenReproSize(0);
}

public class Maui34130SourceGenReproSizeBox : View
{
	public static readonly BindableProperty SizeProperty =
		BindableProperty.Create(nameof(Size), typeof(Maui34130SourceGenReproSize), typeof(Maui34130SourceGenReproSizeBox), default(Maui34130SourceGenReproSize));

	public Maui34130SourceGenReproSize Size
	{
		get => (Maui34130SourceGenReproSize)GetValue(SizeProperty);
		set => SetValue(SizeProperty, value);
	}
}

public class Maui34130SourceGenReproGenericWrapper<T>
{
	public T? Value { get; set; }
}

public class Maui34130SourceGenReproItem
{
	public string Name { get; set; } = string.Empty;
}

public class Maui34130SourceGenReproViewModel
{
	public Maui34130SourceGenReproGenericWrapper<Maui34130SourceGenReproItem?> SelectedItem { get; } = new();
}
""";

				var xamlMarkup =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			 xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			 xmlns:local="clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests"
			 x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.Maui34130SourceGenRepro">
	<VerticalStackLayout>
		<Button x:DataType="local:Maui34130SourceGenReproViewModel"
				CommandParameter="{Binding SelectedItem}" />
		<local:Maui34130SourceGenReproSizeBox Size="42" />
	</VerticalStackLayout>
</ContentPage>
""";

				var compilation = CreateMauiCompilation();
				var csharpCompilation = (Microsoft.CodeAnalysis.CSharp.CSharpCompilation)compilation;
				compilation = csharpCompilation.WithOptions(
					csharpCompilation.Options.WithNullableContextOptions(Microsoft.CodeAnalysis.NullableContextOptions.Enable));

				var result = compilation
					.WithAdditionalSource(xaml, hintName: "Maui34130SourceGenRepro.xaml.cs")
					.RunMauiSourceGenerator(new MockSourceGenerator.AdditionalXamlFile("Issues/Maui34130SourceGenRepro.xaml", xamlMarkup, TargetFramework: "net10.0"));
				var generated = result.GeneratedInitializeComponent();

				// Verify CS8605 fix: generated conversion for value-type target uses null-forgiving before unboxing
				Assert.Contains("ConvertFromInvariantString(\"42\"", generated, StringComparison.Ordinal);
				Assert.Contains(")!);", generated, StringComparison.Ordinal);

				// Verify CS8619 fix: generated TypedBinding preserves nullable generic type argument (MyItem?)
				Assert.Contains("TypedBinding<global::Microsoft.Maui.Controls.Xaml.UnitTests.Maui34130SourceGenReproViewModel, global::Microsoft.Maui.Controls.Xaml.UnitTests.Maui34130SourceGenReproGenericWrapper<global::Microsoft.Maui.Controls.Xaml.UnitTests.Maui34130SourceGenReproItem?>>", generated, StringComparison.Ordinal);
				Assert.DoesNotContain("TypedBinding<global::Microsoft.Maui.Controls.Xaml.UnitTests.Maui34130SourceGenReproViewModel, global::Microsoft.Maui.Controls.Xaml.UnitTests.Maui34130SourceGenReproGenericWrapper<global::Microsoft.Maui.Controls.Xaml.UnitTests.Maui34130SourceGenReproItem>>", generated, StringComparison.Ordinal);
				Assert.Contains("source.SelectedItem, true", generated, StringComparison.Ordinal);
			}
			else
				Assert.NotNull(new Maui34130(inflator));
		}
	}
}

[TypeConverter(typeof(Maui34130SizeConverter))]
public readonly record struct Maui34130Size(double Value);

public class Maui34130SizeConverter : TypeConverter, IExtendedTypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		=> value is string s && double.TryParse(s, NumberStyles.Any, culture, out double d)
			? new Maui34130Size(d)
			: base.ConvertFrom(context, culture, value)!;

	public object ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
		=> double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double d)
			? new Maui34130Size(d)
			: new Maui34130Size(0);
}

public class Maui34130SizeBox : View
{
	public static readonly BindableProperty SizeProperty =
		BindableProperty.Create(nameof(Size), typeof(Maui34130Size), typeof(Maui34130SizeBox), default(Maui34130Size));

	public Maui34130Size Size
	{
		get => (Maui34130Size)GetValue(SizeProperty);
		set => SetValue(SizeProperty, value);
	}
}

public class Maui34130GenericWrapper<T>
{
	public T? Value { get; set; }
}

public class Maui34130Item
{
	public string Name { get; set; } = string.Empty;
}

public class Maui34130ViewModel
{
	public Maui34130GenericWrapper<Maui34130Item?> SelectedItem { get; } = new();
}
