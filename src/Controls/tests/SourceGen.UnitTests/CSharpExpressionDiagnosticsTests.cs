using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class CSharpExpressionDiagnosticsTests : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void AmbiguousMember_ReportsMAUIX2008()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.AmbiguousPage"
             x:DataType="local:AmbiguousViewModel">
    <Label Text="{SharedName}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class AmbiguousViewModel : INotifyPropertyChanged
{
	public string SharedName { get; set; } = "VM SharedName";
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class AmbiguousPage : ContentPage
{
	public string SharedName => "Page SharedName";
	public AmbiguousPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2008");
	}

	[Fact]
	public void MemberNotFound_ReportsMAUIX2009()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.NotFoundPage"
             x:DataType="local:NotFoundViewModel">
    <Label Text="{NonExistentProperty}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class NotFoundViewModel : INotifyPropertyChanged
{
	public string Name { get; set; } = "Name";
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class NotFoundPage : ContentPage
{
	public NotFoundPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2009");
	}

	[Fact]
	public void ExplicitThisPrefix_NoAmbiguousError()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.ExplicitThisPage"
             x:DataType="local:ExplicitThisViewModel">
    <Label Text="{this.Title}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class ExplicitThisViewModel : INotifyPropertyChanged
{
	public string Title { get; set; } = "VM Title";
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class ExplicitThisPage : ContentPage
{
	public string Title => "Page Title";
	public ExplicitThisPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		// No MAUIX2008 errors because we used explicit this. prefix
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2008");
	}

	[Fact]
	public void ExplicitDotPrefix_NoAmbiguousError()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.ExplicitDotPage"
             x:DataType="local:ExplicitDotViewModel">
    <Label Text="{.Title}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class ExplicitDotViewModel : INotifyPropertyChanged
{
	public string Title { get; set; } = "VM Title";
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class ExplicitDotPage : ContentPage
{
	public string Title => "Page Title";
	public ExplicitDotPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		// No MAUIX2008 errors because we used explicit . prefix
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2008");
	}

	[Fact]
	public void MemberConflictsWithStaticType_ReportsMAUIX2011()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.StaticConflictPage"
             x:DataType="local:StaticConflictViewModel">
    <Label Text="{Math.Max(A, B).ToString()}" />
</ContentPage>
""";

		var codeBehind =
"""
global using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class MathHelper
{
	public int Max(int a, int b) => a > b ? a : b;
}

public class StaticConflictViewModel : INotifyPropertyChanged
{
	public MathHelper Math { get; set; } = new();
	public int A => 5;
	public int B => 10;
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class StaticConflictPage : ContentPage
{
	public StaticConflictPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2011");
	}

	[Fact]
	public void ExplicitDotPrefix_NoStaticTypeConflictWarning()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.NoConflictPage"
             x:DataType="local:NoConflictViewModel">
    <Label Text="{.Math.Max(A, B).ToString()}" />
</ContentPage>
""";

		var codeBehind =
"""
global using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class MathHelper
{
	public int Max(int a, int b) => a > b ? a : b;
}

public class NoConflictViewModel : INotifyPropertyChanged
{
	public MathHelper Math { get; set; } = new();
	public int A => 5;
	public int B => 10;
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class NoConflictPage : ContentPage
{
	public NoConflictPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		// No MAUIX2011 warning because we used explicit . prefix
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2011");
	}

	[Fact]
	public void BareIdentifier_AmbiguousBetweenMarkupAndProperty_ReportsMAUIX2007()
	{
		// This test verifies that when a bare identifier like {Binding} could be EITHER
		// a markup extension (BindingExtension) OR a C# expression property (Binding),
		// we emit MAUIX2007 warning and default to markup extension.
		// 
		// We use "Binding" here because it's a known MAUI markup extension that exists
		// in the default namespace. Having a property named "Binding" on the ViewModel
		// creates a genuine ambiguity.
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.AmbiguousMarkupPage"
             x:DataType="local:AmbiguousMarkupViewModel">
    <Label Text="{Binding}" />
</ContentPage>
""";

		var codeBehind =
"""
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class AmbiguousMarkupViewModel : INotifyPropertyChanged
{
	// Property with same name as the Binding markup extension
	public string Binding { get; set; } = "From Property";
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class AmbiguousMarkupPage : ContentPage
{
	public AmbiguousMarkupPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		// Should report MAUIX2007 warning about ambiguity
		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2007");
	}

	[Fact]
	public void BareIdentifier_ExplicitExpressionSyntax_NoAmbiguityWarning()
	{
		// Using {= Binding} explicitly indicates C# expression, no ambiguity
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.ExplicitExpressionPage"
             x:DataType="local:ExplicitExpressionViewModel">
    <Label Text="{= Binding}" />
</ContentPage>
""";

		var codeBehind =
"""
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class ExplicitExpressionViewModel : INotifyPropertyChanged
{
	public string Binding { get; set; } = "From Property";
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class ExplicitExpressionPage : ContentPage
{
	public ExplicitExpressionPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		// No MAUIX2007 warning because we used explicit {= } syntax
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2007");
	}

	[Fact]
	public void BareIdentifier_ExplicitMarkupPrefix_NoAmbiguityWarning()
	{
		// Using {x:Null} explicitly indicates markup extension, no ambiguity check needed
		// (This tests that prefixed markup extensions don't trigger ambiguity warnings)
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.ExplicitMarkupPage"
             x:DataType="local:ExplicitMarkupViewModel">
    <Label Text="{x:Null}" />
</ContentPage>
""";

		var codeBehind =
"""
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class ExplicitMarkupViewModel : INotifyPropertyChanged
{
	// Even if there's a property named "Null", the x: prefix makes it unambiguous
	public string Null { get; set; } = "From Property";
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class ExplicitMarkupPage : ContentPage
{
	public ExplicitMarkupPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		// No MAUIX2007 warning because we used explicit x: prefix
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2007");
	}
}
