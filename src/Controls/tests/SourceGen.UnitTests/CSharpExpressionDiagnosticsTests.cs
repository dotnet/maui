using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

	[Fact]
	public void ComplexExpression_OnTwoWayProperty_EmitsInfoDiagnostic()
	{
		// Complex expressions on two-way properties emit MAUIX2010 as Info (not Warning)
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TwoWayWarningPage"
             x:DataType="local:TwoWayViewModel">
    <Entry Text="{.FirstName + ' ' + .LastName}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class TwoWayViewModel : INotifyPropertyChanged
{
	public string FirstName { get; set; } = "John";
	public string LastName { get; set; } = "Doe";
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TwoWayWarningPage : ContentPage
{
	public TwoWayWarningPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);

		// MAUIX2010 should be emitted as Info level
		var diagnostic = result.Diagnostics.FirstOrDefault(d => d.Id == "MAUIX2010");
		Assert.NotNull(diagnostic);
		Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
	}

	[Fact]
	public void SimplePropertyBinding_OnTwoWayProperty_NoWarning()
	{
		// Simple property bindings CAN generate a setter, so no warning
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.TwoWayNoWarningPage"
             x:DataType="local:TwoWayViewModel">
    <Entry Text="{FirstName}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class TwoWayViewModel : INotifyPropertyChanged
{
	public string FirstName { get; set; } = "John";
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TwoWayNoWarningPage : ContentPage
{
	public TwoWayNoWarningPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		// No MAUIX2010 warning because simple property can be two-way
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2010");
	}

	[Fact]
	public void GetOnlyStructIntermediate_OnTwoWayProperty_ReportsMAUIX2018AndOmitsSetter()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.ReadOnlyStructIntermediatePage"
             x:DataType="local:ReadOnlyStructViewModel">
    <Entry Text="{ReadOnlyMargin.Top}" />
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class ReadOnlyStructViewModel
{
	public Microsoft.Maui.Thickness ReadOnlyMargin { get; } = new Microsoft.Maui.Thickness(1, 2, 3, 4);
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class ReadOnlyStructIntermediatePage : ContentPage
{
	public ReadOnlyStructIntermediatePage() => InitializeComponent();
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);

		var diagnostic = Assert.Single(result.Diagnostics.Where(d => d.Id == "MAUIX2018"));
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
		Assert.Contains("ReadOnlyMargin.Top", diagnostic.GetMessage(), StringComparison.Ordinal);

		Assert.NotNull(output);
		Assert.Contains("TypedBinding<global::TestApp.ReadOnlyStructViewModel, double>", output, StringComparison.Ordinal);
		Assert.Matches(@"__source\s*=>\s*\(__source\.ReadOnlyMargin\.Top,\s*true\),\s*null,", output);
		Assert.DoesNotContain("__source.ReadOnlyMargin.Top = __value", output, StringComparison.Ordinal);
	}

	[Fact]
	public void NullConditionalAccess_OnTwoWayProperty_IsSettable()
	{
		// Null-conditional access (?.) IS settable in C# 14+ (guaranteed on .NET 10+)
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.NullConditionalPage"
             x:DataType="local:NullConditionalViewModel">
    <Entry Text="{.User?.Name}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class User
{
	public string Name { get; set; } = "Test";
}

public class NullConditionalViewModel : INotifyPropertyChanged
{
	public User? User { get; set; }
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class NullConditionalPage : ContentPage
{
	public NullConditionalPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);

		// No MAUIX2010 because ?. is settable in C# 14+ (.NET 10+)
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2010");
	}

	[Fact]
	public void AsyncLambdaEventHandler_ReportsMAUIX2013()
	{
		// Async lambdas are not supported for event handlers
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.AsyncLambdaPage">
    <Button Clicked="{async (s, e) => await Task.Delay(100)}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class AsyncLambdaPage : ContentPage
{
	public AsyncLambdaPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		// Should report MAUIX2013 error about async lambda not supported
		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2013");
	}

	[Fact]
	public void SyncLambdaEventHandler_NoError()
	{
		// Sync lambdas are supported
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.SyncLambdaPage">
    <Button Clicked="{(s, e) => Count++}" />
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class SyncLambdaPage : ContentPage
{
	public int Count { get; set; }
	public SyncLambdaPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		// No MAUIX2013 error
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2013");
	}

	[Fact]
	public void CSharpExpression_WithoutPreviewFeatureFlag_GeneratesExpression()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.NoPreviewPage">
    <Label Text="{GetText()}" />
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class NoPreviewPage : ContentPage
{
	public string GetText() => "Hello";
	public NoPreviewPage() => InitializeComponent();
}
""";

		var (result, generated) = RunGenerator(xaml, codeBehind);

		Assert.Empty(result.Diagnostics);
		Assert.Contains("GetText()", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void SingleQuotedLiteral_MethodExpectsString_GeneratesStringLiteral()
	{
		// When a method expects string, 'x' should become "x"
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.StringMethodPage"
             x:DataType="local:StringMethodViewModel">
    <Label Text="{GetText('x')}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class StringMethodViewModel : INotifyPropertyChanged
{
	public string GetText(string input) => $"Got: {input}";
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class StringMethodPage : ContentPage
{
	public StringMethodPage() => InitializeComponent();
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);

		// Should compile without errors
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);

		// Generated code should have "x" (string literal), not 'x' (char literal)
		Assert.Contains("\"x\"", output, StringComparison.Ordinal);
	}

	[Fact]
	public void SingleQuotedLiteral_MethodExpectsChar_GeneratesCharLiteral()
	{
		// When a method expects char, 'x' should stay as 'x'
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.CharMethodPage"
             x:DataType="local:CharMethodViewModel">
    <Label Text="{.GetChar('x').ToString()}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class CharMethodViewModel : INotifyPropertyChanged
{
	public char GetChar(char input) => input;
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class CharMethodPage : ContentPage
{
	public CharMethodPage() => InitializeComponent();
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);

		// Should have no source generator diagnostics (errors)
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);

		// Generated code should have 'x' (char literal)
		Assert.Contains("'x'", output, StringComparison.Ordinal);
	}

	[Fact]
	public void OperatorAliases_ANDandOR_TransformedToLogicalOperators()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.OperatorAliasPage"
             x:DataType="local:OperatorAliasViewModel">
    <Label IsVisible="{.IsLoaded AND .HasData}" />
    <Label IsVisible="{.IsEmpty OR .HasError}" />
    <Label IsVisible="{.Count GT 0 AND .Count LT 100}" />
    <Label IsVisible="{.Count GTE 0 AND .Count LTE 100}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class OperatorAliasViewModel : INotifyPropertyChanged
{
	public bool IsLoaded { get; set; }
	public bool HasData { get; set; }
	public bool IsEmpty { get; set; }
	public bool HasError { get; set; }
	public int Count { get; set; }
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class OperatorAliasPage : ContentPage
{
	public OperatorAliasPage() => InitializeComponent();
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);

		// Should have no source generator diagnostics (errors)
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);

		// Generated code should have C# operators instead of aliases
		Assert.Contains("&&", output, StringComparison.Ordinal);
		Assert.Contains("||", output, StringComparison.Ordinal);
		Assert.Contains(" < ", output, StringComparison.Ordinal);
		Assert.Contains(" > ", output, StringComparison.Ordinal);
		Assert.Contains(" <= ", output, StringComparison.Ordinal);
		Assert.Contains(" >= ", output, StringComparison.Ordinal);
	}

	[Fact]
	public void XmlnsPrefix_TypeReference_UsesFullyQualifiedDisplayString()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:helpers="clr-namespace:TestApp.Helpers"
             x:Class="TestApp.XmlnsPrefixPage">
    <Label Text="{helpers:ExpressionHelper.Value}" />
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp.Helpers
{
	public static class ExpressionHelper
	{
		public static string Value => "Resolved";
	}
}

namespace TestApp
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class XmlnsPrefixPage : ContentPage
	{
		public XmlnsPrefixPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);

		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		Assert.Contains("global::TestApp.Helpers.ExpressionHelper.Value", output, StringComparison.Ordinal);
		Assert.DoesNotContain("global::.", output, StringComparison.Ordinal);
		Assert.DoesNotContain("<global namespace>", output, StringComparison.Ordinal);
	}

	[Fact]
	public void AttachedBindableProperty_WithNamedTarget_RewritesToGetter()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.NamedAttachedPropertyPage">
    <VerticalStackLayout>
        <Button x:Name="MyButton" />
        <Label Text="{MyButton.(Grid.Row)}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
global using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class NamedAttachedPropertyPage : ContentPage
{
	public NamedAttachedPropertyPage() => InitializeComponent();
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);

		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		Assert.Contains("Grid.GetRow(MyButton)", output, StringComparison.Ordinal);
		Assert.DoesNotContain("__source", output, StringComparison.Ordinal);
	}

	[Fact]
	public void AttachedBindableProperty_WithXmlnsPrefix_RewritesToFullyQualifiedGetter()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:ios="clr-namespace:Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;assembly=Microsoft.Maui.Controls"
             x:Class="TestApp.PrefixedAttachedPropertyPage">
    <Label IsVisible="{this.(ios:Page.UseSafeArea)}" />
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class PrefixedAttachedPropertyPage : ContentPage
{
	public PrefixedAttachedPropertyPage() => InitializeComponent();
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);

		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		Assert.Contains("global::Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this)", output, StringComparison.Ordinal);
	}

	[Fact]
	public void AttachedBindableProperty_RewritesSkipStringAndInterpolatedLiteralSegments()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:helpers="clr-namespace:TestApp.Helpers"
             x:Class="TestApp.LiteralAttachedPropertyPage">
    <VerticalStackLayout>
        <Label Text="{= 'helpers:ExpressionHelper (Grid.Row)'}" />
        <Label Text="{$'helpers:ExpressionHelper (Grid.Row) {this.(Grid.Row)}'}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
global using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp.Helpers
{
	public static class ExpressionHelper
	{
		public static string Value => "Resolved";
	}
}

namespace TestApp
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class LiteralAttachedPropertyPage : ContentPage
	{
		public LiteralAttachedPropertyPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);

		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		Assert.Contains("\"helpers:ExpressionHelper (Grid.Row)\"", output, StringComparison.Ordinal);
		Assert.Contains("$\"helpers:ExpressionHelper (Grid.Row) {Grid.GetRow(this)}\"", output, StringComparison.Ordinal);
		Assert.DoesNotContain("ExpressionHelper.Get", output, StringComparison.Ordinal);
	}

	[Fact]
	public void AttachedBindableProperty_StandaloneTarget_IsRejectedWithoutSourceRewrite()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.StandaloneAttachedPropertyPage">
    <Label Text="{(Grid.Row)}" />
</ContentPage>
""";

		var codeBehind =
"""
global using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class StandaloneAttachedPropertyPage : ContentPage
{
	public StandaloneAttachedPropertyPage() => InitializeComponent();
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);

		Assert.Contains(result.Diagnostics, d =>
			d.Id == "MAUIX2002"
			&& d.GetMessage().Contains("Standalone attached property expression", StringComparison.Ordinal));
		if (output is not null)
			Assert.DoesNotContain("__source", output, StringComparison.Ordinal);
	}

	[Fact]
	public void ParenthesizedStaticMemberExpression_IsNotRejectedAsAttachedProperty()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.ParenthesizedStaticMemberPage">
    <Label Text="{(Math.PI).ToString()}" />
</ContentPage>
""";

		var codeBehind =
"""
global using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class ParenthesizedStaticMemberPage : ContentPage
{
	public ParenthesizedStaticMemberPage() => InitializeComponent();
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);

		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		Assert.Contains("(Math.PI).ToString()", output, StringComparison.Ordinal);
	}

	[Fact]
	public void InterpolatedString_NonExistentMember_ShouldReportDiagnostic()
	{
		// When an interpolated string references a member that doesn't exist,
		// the source generator should report MAUIX2009 instead of letting it
		// fall through to a C# compilation error.
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.InterpolatedNotFoundPage"
             x:DataType="local:InterpolatedNotFoundViewModel">
    <Label Text="{$'Hello, {Name1}!'}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class InterpolatedNotFoundViewModel : INotifyPropertyChanged
{
	public string Name { get; set; } = "World";
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class InterpolatedNotFoundPage : ContentPage
{
	public InterpolatedNotFoundPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);

		// Currently: Name1 doesn't exist on either the page or the ViewModel.
		// The source gen emits $"Hello, {__source.Name1}!" which fails at C# compilation.
		// Ideally, MAUIX2009 should be reported at source generation time.
		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2009");
	}

	[Fact]
	public void InterpolatedString_StaticTypeReferences_ShouldNotReportMemberNotFound()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.StaticTypeInterpolationPage"
             x:DataType="local:StaticTypeInterpolationViewModel">
    <VerticalStackLayout>
        <Label Text="{$'{DateTime.Now}'}" />
        <Label Text="{$'{Math.Max(X, Y)}'}" />
        <Label Text="{$'{System.DateTime.Now}'}" />
        <Label Text="{$'{LocalStatic.Format(X)}'}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
global using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class StaticTypeInterpolationViewModel : INotifyPropertyChanged
{
	public int X { get; set; } = 3;
	public int Y { get; set; } = 5;
	public event PropertyChangedEventHandler? PropertyChanged;
}

public static class LocalStatic
{
	public static string Format(int value) => value.ToString();
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class StaticTypeInterpolationPage : ContentPage
{
	public StaticTypeInterpolationPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2009");
	}

	[Fact]
	public void InterpolatedString_BinaryAndConditionalHoles_ShouldNotReportMemberNotFound()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.InterpolatedComplexHolesPage"
             x:DataType="local:InterpolatedComplexHolesViewModel">
    <Label Text="{$'{X + Y} {(UseX ? X : Y)}'}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class InterpolatedComplexHolesViewModel : INotifyPropertyChanged
{
	public int X { get; set; } = 3;
	public int Y { get; set; } = 5;
	public bool UseX { get; set; } = true;
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class InterpolatedComplexHolesPage : ContentPage
{
	public InterpolatedComplexHolesPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);

		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2009");
	}

	[Fact]
	public void InterpolatedString_BinaryHoleWithMissingMember_ShouldReportDiagnostic()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.InterpolatedBinaryMissingPage"
             x:DataType="local:InterpolatedBinaryMissingViewModel">
    <Label Text="{$'{X + MissingValue}'}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class InterpolatedBinaryMissingViewModel : INotifyPropertyChanged
{
	public int X { get; set; } = 3;
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class InterpolatedBinaryMissingPage : ContentPage
{
	public InterpolatedBinaryMissingPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);

		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2009");
	}

	[Fact]
	public void LambdaEventHandler_MethodGroupWithoutInvocation_ShouldReportDiagnostic()
	{
		// When a lambda event handler references a method without calling it (missing parentheses),
		// the source generator should detect this and report a diagnostic instead of
		// letting it fall through to a C# compilation error.
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.MethodGroupLambdaPage">
    <Button Clicked="{(s, e) => this.OnCounterClicked}" />
</ContentPage>
""";

		var codeBehind =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class MethodGroupLambdaPage : ContentPage
{
	public int ClickCount { get; set; }
	public void OnCounterClicked(object? sender, EventArgs e) => ClickCount++;
	public MethodGroupLambdaPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);

		// The source gen should detect the method group reference and emit MAUIX2019.
		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2019");
		var diagnostic = result.Diagnostics.First(d => d.Id == "MAUIX2019");
		Assert.Contains("Did you mean 'this.OnCounterClicked(s, e)'", diagnostic.GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public void LambdaEventHandler_MethodGroupWithDiscardParameters_ShouldOmitSuggestion()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.DiscardMethodGroupLambdaPage">
    <Button Clicked="{(_, _) => this.OnCounterClicked}" />
</ContentPage>
""";

		var codeBehind =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class DiscardMethodGroupLambdaPage : ContentPage
{
	public int ClickCount { get; set; }
	public void OnCounterClicked(object? sender, EventArgs e) => ClickCount++;
	public DiscardMethodGroupLambdaPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);

		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2019");
		var diagnostic = result.Diagnostics.First(d => d.Id == "MAUIX2019");
		Assert.DoesNotContain("Did you mean", diagnostic.GetMessage(), StringComparison.Ordinal);
		Assert.DoesNotContain("(_, _)", diagnostic.GetMessage(), StringComparison.Ordinal);
	}

	// https://github.com/dotnet/maui/issues/35900
	[Theory]
	[InlineData("<Label TextColor=\"{IsVip ? Colors.Goldenrod : Colors.Gray}\" />")]
	[InlineData("<Label Text=\"{$'Now: {DateTime.Now:t}'}\" />")]
	[InlineData("<Label Text=\"{$'Max = {Math.Max(Price, Quantity):F2}'}\" />")]
	[InlineData("<Label Text=\"{$'{Name} - {DateTime.Now:d}'}\" />")]
	public void StaticTypeReferenceInExpression_DoesNotGenerateInvalidHandlers(string labelXaml)
	{
		var xaml =
$"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.StaticRefPage"
             x:DataType="local:StaticRefViewModel">
    {labelXaml}
</ContentPage>
""";

		var codeBehind =
"""
global using System;
global using Microsoft.Maui.Graphics;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class StaticRefViewModel : INotifyPropertyChanged
{
	public bool IsVip { get; set; }
	public string Name { get; set; } = string.Empty;
	public double Price { get; set; }
	public double Quantity { get; set; }
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class StaticRefPage : ContentPage
{
	public StaticRefPage() => InitializeComponent();
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);

		// The generated code must not contain handler tuples rooted at static type
		// names (Colors, DateTime, Math), which previously produced
		// `static __source => __source.Colors` / `__source.DateTime` / `__source.Math`
		// and broke compilation with CS1061.
		Assert.NotNull(output);
		Assert.DoesNotContain("__source.Colors", output!, StringComparison.Ordinal);
		Assert.DoesNotContain("__source.DateTime", output!, StringComparison.Ordinal);
		Assert.DoesNotContain("__source.Math", output!, StringComparison.Ordinal);

		// And the compilation must succeed (no compiler errors leaked through).
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
	}

	[Fact]
	public void TernaryInsideInterpolationHole_CompilesCleanly()
	{
		// Issue #36155: a ternary directly inside an interpolation hole must be
		// auto-parenthesized so the ':' is not consumed as a format specifier.
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:TestApp"
             x:Class="TestApp.InterpolationTernaryPage"
             x:DataType="local:InterpolationTernaryViewModel">
    <Label Text="{$'You clicked {Count} {Count == 1 ? 'time' : 'times'}.'}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

public class InterpolationTernaryViewModel : INotifyPropertyChanged
{
	public int Count { get; set; }
	public event PropertyChangedEventHandler? PropertyChanged;
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class InterpolationTernaryPage : ContentPage
{
	public InterpolationTernaryPage() => InitializeComponent();
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);

		Assert.NotNull(output);
		// The ternary must be parenthesized in the generated interpolated string
		// (the 'Count' identifier is rewritten to a binding source path, e.g. __source.Count).
		Assert.Contains("== 1 ? \"time\" : \"times\")", output!, StringComparison.Ordinal);
		// And the compilation must succeed (no CS8076/CS8361/CS1003/CS0103 leaked through).
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
	}

	[Fact]
	public void XmlnsPrefixedTypeInTernaryAndCallee_ExpandsAndBinds()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:app="clr-namespace:Repro"
             xmlns:res="clr-namespace:Repro.Resources"
             xmlns:ext="clr-namespace:Repro.Resources;assembly=SourceGeneratorDriver.Generated"
             x:Class="Repro.Views.MainPage"
             x:DataType="app:MainViewModel">
    <VerticalStackLayout>
        <Label Text="{= IsVip ? res:Labels.Gold : res:Labels.Standard}" />
        <Label Text="{= IsVip ? (res:Labels.Gold) : (res:Labels.Standard)}" />
        <Label Text="{= IsVip ? ext:Labels.Gold : ext:Labels.Standard}" />
        <Label Text="{= res:Labels.FormatMessage(IsVip)}" />
        <Label Text="{= $'{(IsVip ? res:Labels.Gold : res:Labels.Standard)}'}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Resources
{
	public static class Labels
	{
		public static string Gold => "Gold member";
		public static string Standard => "Standard member";
		public static string FormatMessage(bool isVip) => isVip ? Gold : Standard;
	}
}

namespace Repro
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public bool IsVip { get; set; }
		public string Repro { get; set; } = string.Empty;
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class MainPage : ContentPage
	{
		public MainPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		Assert.NotNull(output);
		Assert.DoesNotContain("res:", output, StringComparison.Ordinal);
		Assert.DoesNotContain("ext:", output, StringComparison.Ordinal);
		Assert.DoesNotContain("__source.Repro", output, StringComparison.Ordinal);
		AssertEachSetBindingContains(
			output,
			expectedCount: 5,
			"new(static __source => __source, \"IsVip\")",
			"__source.IsVip ? global::Repro.Resources.Labels.Gold : global::Repro.Resources.Labels.Standard",
			"__source.IsVip ? (global::Repro.Resources.Labels.Gold) : (global::Repro.Resources.Labels.Standard)",
			"global::Repro.Resources.Labels.FormatMessage(__source.IsVip)",
			"$\"{(__source.IsVip ? global::Repro.Resources.Labels.Gold : global::Repro.Resources.Labels.Standard)}\"");
	}

	[Fact]
	public void XmlnsPrefixedUnresolvedType_FallsBackToClrNamespaceAndBinds()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:app="clr-namespace:Repro"
             xmlns:missing="clr-namespace:Repro.MissingGen;assembly=SourceGeneratorDriver.Generated"
             x:Class="Repro.Views.MainPage"
             x:DataType="app:MainViewModel">
    <VerticalStackLayout>
        <Label Text="{= IsVip ? missing:Strings.Gold : missing:Strings.Standard}" />
        <Label Text="{= IsVip ? (missing:Strings.Gold) : (missing:Strings.Standard)}" />
        <Label Text="{= missing:Strings.FormatMessage(IsVip)}" />
        <Label Text="{= $'{missing:Strings.FormatMessage(IsVip)}'}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public bool IsVip { get; set; }
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class MainPage : ContentPage
	{
		public MainPage() => InitializeComponent();
	}
}
""";

		// Strings is absent from initial compilation (simulating a sibling ResX source generator).
		var (result, output) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("missing:", output, StringComparison.Ordinal);
		AssertEachSetBindingContains(
			output,
			expectedCount: 4,
			"new(static __source => __source, \"IsVip\")",
			"__source.IsVip ? global::Repro.MissingGen.Strings.Gold : global::Repro.MissingGen.Strings.Standard",
			"__source.IsVip ? (global::Repro.MissingGen.Strings.Gold) : (global::Repro.MissingGen.Strings.Standard)",
			"global::Repro.MissingGen.Strings.FormatMessage(__source.IsVip)",
			"$\"{(global::Repro.MissingGen.Strings.FormatMessage(__source.IsVip))}\"");

		// Once the sibling generator emits Repro.MissingGen.Strings, generated code compiles cleanly.
		var siblingGenerated =
"""
namespace Repro.MissingGen
{
	public static class Strings
	{
		public static string Gold => "Gold member";
		public static string Standard => "Standard member";
		public static string FormatMessage(bool isVip) => isVip ? Gold : Standard;
	}
}
""";
		AssertGeneratedCodeCompilesWithSiblingSource(codeBehind, siblingGenerated, result);
	}

	[Fact]
	public void ExpressionOnHeuristicBindableProperty_AndObservableProperty_GeneratesTypedBinding()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:app="clr-namespace:Repro"
             xmlns:res="clr-namespace:Repro.Resources"
             x:Class="Repro.Views.MainPage"
             x:DataType="app:MainViewModel">
    <app:CustomPopup PopupMessage="{= res:Labels.FormatMessage(Nickname)}" />
</ContentPage>
""";

		var codeBehind =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace CommunityToolkit.Maui
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class BindablePropertyAttribute : Attribute
	{
		public string? PropertyName { get; set; }
	}
}

namespace CommunityToolkit.Mvvm.ComponentModel
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class ObservablePropertyAttribute : Attribute
	{
	}
}

namespace Repro.Resources
{
	public static class Labels
	{
		public static string FormatMessage(string? name) => $"Delete {name}?";
	}
}

namespace Repro
{
	public partial class MainViewModel
	{
		[CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
		private string? _nickname;
	}

	public partial class CustomPopup : ContentView
	{
		[CommunityToolkit.Maui.BindableProperty]
		public partial string? PopupMessage { get; set; }
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class MainPage : ContentPage
	{
		public MainPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("res:", output, StringComparison.Ordinal);
		Assert.Contains("customPopup.SetBinding(global::Repro.CustomPopup.PopupMessageProperty,", output, StringComparison.Ordinal);
		Assert.Contains("__source => (global::Repro.Resources.Labels.FormatMessage(__source.Nickname), true)", output, StringComparison.Ordinal);
		Assert.Contains("new(static __source => __source, \"Nickname\")", output, StringComparison.Ordinal);

		var siblingGenerated =
"""
namespace Repro
{
	public partial class MainViewModel
	{
		public string? Nickname
		{
			get => _nickname;
			set => _nickname = value;
		}
	}

	public partial class CustomPopup
	{
		public static readonly Microsoft.Maui.Controls.BindableProperty PopupMessageProperty =
			Microsoft.Maui.Controls.BindableProperty.Create(nameof(PopupMessage), typeof(string), typeof(CustomPopup), default(string));

		public partial string? PopupMessage
		{
			get => (string?)GetValue(PopupMessageProperty);
			set => SetValue(PopupMessageProperty, value);
		}
	}
}
""";
		AssertGeneratedCodeCompilesWithSiblingSource(codeBehind, siblingGenerated, result);
	}

	[Fact]
	public void ObservableProperty_TwoWayBinding_PreservesPropertyTypeAndSetter()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:app="clr-namespace:Repro"
             x:Class="Repro.Views.MainPage"
             x:DataType="app:MainViewModel">
    <Entry Text="{Nickname}" />
</ContentPage>
""";

		var codeBehind =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace CommunityToolkit.Mvvm.ComponentModel
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class ObservablePropertyAttribute : Attribute
	{
	}
}

namespace Repro
{
	public partial class MainViewModel
	{
		[CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
		private string? _nickname;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class MainPage : ContentPage
	{
		public MainPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.Contains("new global::Microsoft.Maui.Controls.Internals.TypedBinding<global::Repro.MainViewModel, string>(", output, StringComparison.Ordinal);
		Assert.Contains("__source => (__source.Nickname, true),", output, StringComparison.Ordinal);
		Assert.Contains("(__source, __value) => __source.Nickname = __value,", output, StringComparison.Ordinal);

		var siblingGenerated =
"""
namespace Repro
{
	public partial class MainViewModel
	{
		public string? Nickname
		{
			get => _nickname;
			set => _nickname = value;
		}
	}
}
""";
		AssertGeneratedCodeCompilesWithSiblingSource(codeBehind, siblingGenerated, result);
	}

	[Fact]
	public void XmlnsPrefixedGenericMethodInInterpolation_PreservesTypeArguments()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:app="clr-namespace:Repro"
             xmlns:res="clr-namespace:Repro.Resources"
             x:Class="Repro.Views.MainPage"
             x:DataType="app:MainViewModel">
    <VerticalStackLayout>
        <Label Text="{= $'{res:Helper.Format&lt;int, string&gt;(Count)}' }" />
        <Label Text="{= $'{res:Helper.Format&lt;int, string&gt;(Count),10}' }" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Resources
{
	public static class Helper
	{
		public static string Format<TFirst, TSecond>(TFirst value) => $"{typeof(TSecond).Name}:{value}";
	}
}

namespace Repro
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public int Count { get; set; } = 42;
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class MainPage : ContentPage
	{
		public MainPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		Assert.NotNull(output);
		Assert.DoesNotContain("res:", output, StringComparison.Ordinal);
		Assert.Contains("$\"{(global::Repro.Resources.Helper.Format<int, string>(__source.Count))}\"", output, StringComparison.Ordinal);
		Assert.Contains("$\"{(global::Repro.Resources.Helper.Format<int, string>(__source.Count)),10}\"", output, StringComparison.Ordinal);
	}

	[Fact]
	public void InterpolationFormatClause_MatchingXmlnsPrefixAndType_IsPreserved()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:app="clr-namespace:Repro"
             xmlns:Price="clr-namespace:Repro.Formats"
             x:Class="Repro.Views.MainPage"
             x:DataType="app:MainViewModel">
    <Label Text="{= $'{Price:C2}'}" />
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Formats
{
	public class C2
	{
	}
}

namespace Repro
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public decimal Price { get; set; } = 19.99m;
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class MainPage : ContentPage
	{
		public MainPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.Formats.C2", output, StringComparison.Ordinal);
		Assert.Contains("$\"{__source.Price:C2}\"", output, StringComparison.Ordinal);
	}

	[Fact]
	public void XmlnsPrefixedFallback_PreservesNamedArgumentMatchingPrefix()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Repro"
             xmlns:helpers="clr-namespace:Repro.Helpers"
             xmlns:missing="clr-namespace:Repro.MissingGen;assembly=SourceGeneratorDriver.Generated"
             x:Class="Repro.Views.MainPage"
             x:DataType="local:MainViewModel">
    <VerticalStackLayout>
        <Label Text="{= helpers:Formatter.Format(local:Name)}" />
        <Label Text="{= helpers:Formatter.Format(local:User.Name)}" />
        <Label Text="{= helpers:Formatter.Format(local:User.GetName())}" />
        <Label Text="{= helpers:Formatter.Format(missing:Strings.Gold)}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Helpers
{
	public static class Formatter
	{
		public static string Format(string local) => $"Hello, {local}";
	}
}

namespace Repro
{
	public class UserInfo
	{
		public string Name { get; set; } = "Maui";
		public string GetName() => Name;
	}

	public class MainViewModel : INotifyPropertyChanged
	{
		public string Name { get; set; } = "Maui";
		public UserInfo User { get; set; } = new();
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class MainPage : ContentPage
	{
		public MainPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.Name", output, StringComparison.Ordinal);
		Assert.DoesNotContain("global::Repro.User", output, StringComparison.Ordinal);
		Assert.Contains("global::Repro.Helpers.Formatter.Format(local:__source.Name)", output, StringComparison.Ordinal);
		Assert.Contains("global::Repro.Helpers.Formatter.Format(local:__source.User.Name)", output, StringComparison.Ordinal);
		Assert.Contains("global::Repro.Helpers.Formatter.Format(local:__source.User.GetName())", output, StringComparison.Ordinal);
		Assert.Contains("global::Repro.Helpers.Formatter.Format(global::Repro.MissingGen.Strings.Gold)", output, StringComparison.Ordinal);

		var siblingGenerated =
"""
namespace Repro.MissingGen
{
	public static class Strings
	{
		public static string Gold => "Gold member";
	}
}
""";
		AssertGeneratedCodeCompilesWithSiblingSource(codeBehind, siblingGenerated, result);
	}

	[Fact]
	public void InterpolationFormatClause_DottedFormatWithMatchingXmlnsPrefix_IsPreserved()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:app="clr-namespace:Repro"
             xmlns:Time="clr-namespace:Repro"
             x:Class="Repro.Views.MainPage"
             x:DataType="app:MainViewModel">
    <VerticalStackLayout>
        <Label Text="{= $'{Time:HH.mm}'}" />
        <Label Text="{= $'{User.Time:HH.mm}'}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro
{
	public class UserInfo
	{
		public DateTime Time { get; set; }
	}

	public class MainViewModel : INotifyPropertyChanged
	{
		public DateTime Time { get; set; }
		public UserInfo User { get; set; } = new();
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class MainPage : ContentPage
	{
		public MainPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.HH", output, StringComparison.Ordinal);
		Assert.Contains("$\"{__source.Time:HH.mm}\"", output, StringComparison.Ordinal);
		Assert.Contains("$\"{__source.User.Time:HH.mm}\"", output, StringComparison.Ordinal);
	}

	static void AssertEachSetBindingContains(string output, int expectedCount, string requiredSnippet, params string[] expectedBindingExpressions)
	{
		var matches = System.Text.RegularExpressions.Regex.Matches(output, @"\bSetBinding\s*\(");
		Assert.Equal(expectedCount, matches.Count);
		var blocks = new System.Collections.Generic.List<string>(matches.Count);
		for (int i = 0; i < matches.Count; i++)
		{
			var start = matches[i].Index;
			var end = i + 1 < matches.Count ? matches[i + 1].Index : output.Length;
			var block = output.Substring(start, end - start);
			Assert.Contains(requiredSnippet, block, StringComparison.Ordinal);
			blocks.Add(block);
		}

		foreach (var expectedExpression in expectedBindingExpressions)
		{
			Assert.Contains(blocks, b => b.Contains(expectedExpression, StringComparison.Ordinal));
		}
	}

	static void AssertGeneratedCodeCompilesWithSiblingSource(string codeBehind, string siblingGenerated, GeneratorDriverRunResult result)
	{
		var codeBehindTree = CSharpSyntaxTree.ParseText(codeBehind, path: "CodeBehind.cs");
		var siblingTree = CSharpSyntaxTree.ParseText(siblingGenerated, path: "SiblingGenerated.cs");
		var generatedTrees = result.Results.Single().GeneratedSources
			.Select(gs => CSharpSyntaxTree.ParseText(gs.SourceText.ToString(), path: gs.HintName))
			.ToArray();

		var compilationWithSibling = CreateMauiCompilation()
			.AddSyntaxTrees(codeBehindTree, siblingTree)
			.AddSyntaxTrees(generatedTrees);

		var compileErrors = compilationWithSibling.GetDiagnostics()
			.Where(d => d.Severity == DiagnosticSeverity.Error)
			.Where(d => d.Location.SourceTree == siblingTree
				|| d.Location.SourceTree?.FilePath?.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase) == true)
			.ToArray();
		Assert.Empty(compileErrors);
	}
}
