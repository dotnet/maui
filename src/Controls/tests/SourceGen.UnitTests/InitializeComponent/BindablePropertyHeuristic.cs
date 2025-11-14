using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class BindablePropertyHeuristic : SourceGenXamlInitializeComponentTestBase
{
	[Theory]
	[InlineData("BindablePropertyAttribute", "", "BalanceProperty")]
	[InlineData("BindablePropertyAttribute", "CustomBalance", "CustomBalance")]
	[InlineData("AutoPropertyAttribute", "", "BalanceProperty")]
	[InlineData("AutoPropertyAttribute", "CustomBalance", "CustomBalance")]
	public void BindablePropertyHeuristic_WithPropertyAndBinding_ShouldGenerateSetBinding(
		string attributeName, 
		string? explicitPropertyName,
		string expectedPropertyFieldName)
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
		<Slider x:Name="BalanceSlider" />
		<local:BalanceView Balance="{Binding Source={x:Reference BalanceSlider}, x:DataType='Slider', Path=Value}" />
	</VerticalStackLayout>
</ContentPage>
""";

		// Build the attribute usage with optional PropertyName parameter
		var hasExplicitPropertyName = !string.IsNullOrEmpty(explicitPropertyName);
		var attributeUsage = hasExplicitPropertyName ? $"[{attributeName.Replace("Attribute", "", StringComparison.Ordinal)}(PropertyName = \"{explicitPropertyName}\")]" : $"[{attributeName.Replace("Attribute", "", StringComparison.Ordinal)}]";

		var code =
$$"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

// Simulates an attribute from a third-party library
public class {{attributeName}} : Attribute
{
	public string? PropertyName { get; set; }
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public partial class BalanceView : Label
{
	{{attributeUsage}}
	public partial double Balance { get; set; }
}
""";

		var (result, generated) = RunGenerator(xaml, code);
		
		// Should not have MAUIX2002 error
		var mauix2002Diagnostics = result.Diagnostics.Where(d => d.Id == "MAUIX2002").ToList();
		Assert.Empty(mauix2002Diagnostics);
		
		// Should have generated code with the correct SetBinding call
		Assert.NotNull(generated);
		Assert.Contains($".SetBinding(global::Test.BalanceView.{expectedPropertyFieldName},", generated, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("BindablePropertyAttribute", "_balance", "", "BalanceProperty")]
	[InlineData("BindablePropertyAttribute", "_balance", "CustomBalance", "CustomBalance")]
	[InlineData("BindablePropertyAttribute", "balance", "", "BalanceProperty")]
	[InlineData("AutoPropertyAttribute", "_balance", "", "BalanceProperty")]
	[InlineData("AutoPropertyAttribute", "_balance", "CustomBalance", "CustomBalance")]
	[InlineData("AutoPropertyAttribute", "balance", "", "BalanceProperty")]
	public void BindablePropertyHeuristic_WithFieldAndBinding_ShouldGenerateSetBinding(
		string attributeName,
		string fieldName,
		string? explicitPropertyName,
		string expectedPropertyFieldName)
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
		<Slider x:Name="BalanceSlider" />
		<local:BalanceView Balance="{Binding Source={x:Reference BalanceSlider}, x:DataType='Slider', Path=Value}" />
	</VerticalStackLayout>
</ContentPage>
""";

		// Build the attribute usage with optional PropertyName parameter
		var hasExplicitPropertyName = !string.IsNullOrEmpty(explicitPropertyName);
		var attributeUsage = hasExplicitPropertyName ? $"[{attributeName.Replace("Attribute", "", StringComparison.Ordinal)}(PropertyName = \"{explicitPropertyName}\")]" : $"[{attributeName.Replace("Attribute", "", StringComparison.Ordinal)}]";

		var code =
$$"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

// Simulates an attribute from a third-party library
public class {{attributeName}} : Attribute
{
	public string? PropertyName { get; set; }
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public partial class BalanceView : Label
{
	{{attributeUsage}}
	private double {{fieldName}};
}
""";

		var (result, generated) = RunGenerator(xaml, code);
		
		// Should not have MAUIX2002 error
		var mauix2002Diagnostics = result.Diagnostics.Where(d => d.Id == "MAUIX2002").ToList();
		Assert.Empty(mauix2002Diagnostics);
		
		// Should have generated code with the correct SetBinding call
		Assert.NotNull(generated);
		Assert.Contains($".SetBinding(global::Test.BalanceView.{expectedPropertyFieldName},", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void PropertyWithoutAttribute_ShouldProduceError()
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
		<Slider x:Name="BalanceSlider" />
		<local:BalanceView Balance="{Binding Source={x:Reference BalanceSlider}, x:DataType='Slider', Path=Value}" />
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

public partial class BalanceView : Label
{
	// Property without BindablePropertyAttribute - should still fail
	public double Balance { get; set; }
}
""";

		var (result, generated) = RunGenerator(xaml, code);
		
		// Should have MAUIX2002 error because there's no BindableProperty and no attribute
		var mauix2002Diagnostics = result.Diagnostics.Where(d => d.Id == "MAUIX2002").ToList();
		Assert.NotEmpty(mauix2002Diagnostics);
	}

	[Fact]
	public void PropertyWithAttribute_ButDifferentName_ShouldProduceError()
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
		<Slider x:Name="BalanceSlider" />
		<local:BalanceView Balance="{Binding Source={x:Reference BalanceSlider}, x:DataType='Slider', Path=Value}" />
	</VerticalStackLayout>
</ContentPage>
""";

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

// Simulates an attribute from a third-party library
public class BindablePropertyAttribute : Attribute
{
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public partial class BalanceView : Label
{
	// Property with completely different name - should still fail
	[BindableProperty] 
	public partial double Amount { get; set; }
}
""";

		var (result, generated) = RunGenerator(xaml, code);
		
		// Should have MAUIX2002 error because the name doesn't match
		var mauix2002Diagnostics = result.Diagnostics.Where(d => d.Id == "MAUIX2002").ToList();
		Assert.NotEmpty(mauix2002Diagnostics);
	}

	[Fact]
	public void PropertyWithAttribute_LiteralValue_ShouldUsePropertySetter()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Test"
	x:Class="Test.TestPage">
	<local:BalanceView Balance="123.45" />
</ContentPage>
""";

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

// Simulates an attribute from a third-party library
public class BindablePropertyAttribute : Attribute
{
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public partial class BalanceView : Label
{
	// Has attribute and a property setter - literal assignments should use the property setter, not the BP
	[BindableProperty] 
	public partial double Balance { get; set; }
}
""";

		var (result, generated) = RunGenerator(xaml, code);
		
		// Should NOT have MAUIX2002 error - should use property setter for literal values
		var mauix2002Diagnostics = result.Diagnostics.Where(d => d.Id == "MAUIX2002").ToList();
		Assert.Empty(mauix2002Diagnostics);
		
		// Should use property setter, not SetBinding
		Assert.NotNull(generated);
		Assert.Contains(".Balance = ", generated, StringComparison.Ordinal);
	}
}
