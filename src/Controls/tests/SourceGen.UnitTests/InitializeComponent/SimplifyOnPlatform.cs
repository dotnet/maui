using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class SimplifyOnPlatform : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void SimplifyOnPlatformMarkupExtension()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
    <ContentPage.Resources>
        <Style TargetType="Label" x:Key="style">
            <Setter Property="TextColor" Value="Pink " />
            <Setter Property="IsVisible" Value="{OnPlatform Android=True, iOS=False}" />
        </Style>
    </ContentPage.Resources>
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
""";


		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-android");
		Assert.False(result.Diagnostics.Any());
		Assert.Contains("new global::Microsoft.Maui.Controls.Style(\"Microsoft.Maui.Controls.Label, Microsoft.Maui.Controls\")", generated, StringComparison.Ordinal);
		Assert.Contains("Label.TextColorProperty", generated, StringComparison.Ordinal);
		Assert.Contains("VisualElement.IsVisibleProperty", generated, StringComparison.Ordinal);
		Assert.Contains("style.Initializer = styleInitializer", generated, StringComparison.Ordinal);
		
		// Verify lazy behavior: Initializer is set but NOT called immediately
		Assert.DoesNotContain("styleInitializer(style, new global::Microsoft.Maui.Controls.Label())", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("style.Initializer = null!", generated, StringComparison.Ordinal);
		
		Assert.DoesNotContain("OnPlatformExtension", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void SimplifyOnPlatformElement()
	{
		// Issue #32521: OnPlatform elements should be simplified at compile time based on target framework
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<OnPlatform x:TypeArguments="View">
		<On Platform="Android">
			<Label x:Name="AndroidLabel" Text="Android Content" />
		</On>
		<On Platform="iOS">
			<Label x:Name="iOSLabel" Text="iOS Content" />
		</On>
	</OnPlatform>
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
""";

		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-android");
		Assert.False(result.Diagnostics.Any());

		// Should contain only Android label, not iOS label
		Assert.Contains("AndroidLabel", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("iOSLabel", generated, StringComparison.Ordinal);
		Assert.Contains("Android Content", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("iOS Content", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void SimplifyOnPlatformWithXKeyAndValueNodeWithoutTypeArguments()
	{
		// OnPlatform with x:Key but WITHOUT x:TypeArguments should NOT be simplified
		// when the target value is a ValueNode, because we don't know how to create the typed element
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<OnPlatform x:Key="TestValue">
			<On Platform="Android" Value="10" />
			<On Platform="iOS" Value="20" />
		</OnPlatform>
	</ContentPage.Resources>
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
""";

		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-android");
		Assert.False(result.Diagnostics.Any());

		// OnPlatform should NOT be simplified because there's no x:TypeArguments and the target is a ValueNode
		Assert.Contains("OnPlatform", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void SimplifyOnPlatformWithXKeyAndXTypeArguments()
	{
		// OnPlatform with x:Key and x:TypeArguments should be simplified by creating a typed element
		// For example, OnPlatform with x:TypeArguments="Color" should become <Color x:Key="TestColor">Red</Color>
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<OnPlatform x:Key="TestColor" x:TypeArguments="Color">
			<On Platform="Android" Value="Red" />
			<On Platform="iOS" Value="Blue" />
		</OnPlatform>
	</ContentPage.Resources>
	<Label Text="Test" TextColor="{StaticResource TestColor}" />
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
""";

		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-android");
		Assert.False(result.Diagnostics.Any());

		// OnPlatform SHOULD be simplified because x:TypeArguments tells us how to create a proper typed element
		Assert.DoesNotContain("OnPlatform", generated, StringComparison.Ordinal);
		Assert.Contains("TestColor", generated, StringComparison.Ordinal);
		// Should contain Red (Android), not Blue (iOS)
		Assert.Contains("Red", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("Blue", generated, StringComparison.Ordinal);

		// Verify the generated code creates a Color object directly
		Assert.Contains("Colors.Red", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void SimplifyOnPlatformWithXKeyAndElementNode()
	{
		// OnPlatform with x:Key should be simplified if the target value is an ElementNode
		// and the x:Key should be transferred to the replacement element
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<OnPlatform x:Key="TestStyle" x:TypeArguments="Style">
			<On Platform="Android">
				<Style TargetType="Label">
					<Setter Property="TextColor" Value="Red" />
				</Style>
			</On>
			<On Platform="iOS">
				<Style TargetType="Label">
					<Setter Property="TextColor" Value="Blue" />
				</Style>
			</On>
		</OnPlatform>
	</ContentPage.Resources>
	<Label Text="Test" Style="{StaticResource TestStyle}" />
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
""";

		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-android");
		Assert.False(result.Diagnostics.Any());

		// OnPlatform should be simplified and the x:Key transferred to the Style element
		Assert.DoesNotContain("OnPlatform", generated, StringComparison.Ordinal);
		Assert.Contains("TestStyle", generated, StringComparison.Ordinal);
		// Should contain Android style (Red), not iOS style (Blue)
		Assert.Contains("Red", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("Blue", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void OnPlatformWithMissingTargetPlatformShouldUseDefault()
	{
		// Reproduces Bugzilla39636: When MacCatalyst is not defined in OnPlatform,
		// SourceGen should use default(T) instead of throwing an exception.
		// The SimplifyOnPlatformVisitor marks the node as IsOnPlatformDefaultValue,
		// and CreateValuesVisitor generates default(T) for it.
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<OnPlatform x:Key="SizeMedium" x:TypeArguments="x:Double">
			<On Platform="iOS" Value="40"/>
			<On Platform="Android" Value="30"/>
			<On Platform="UWP" Value="60"/>
		</OnPlatform>
	</ContentPage.Resources>
	<Label Text="Test" WidthRequest="{StaticResource SizeMedium}" />
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
""";

		// Test with MacCatalyst where platform is not defined
		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-maccatalyst");
		
		// Should not have any errors (no TargetInvocationException)
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		
		// Should generate default(double) for the value since no matching platform
		// The generated code should include: double double0 = default;
		Assert.Contains("double double0 = default;", generated, StringComparison.Ordinal);
	}
}
