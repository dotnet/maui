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
        <Label Text="{= helpers:Formatter.Format(local:Name) + '!'}" />
        <Label Text="{= helpers:Formatter.Format(missing:Strings.Gold, IsVip)}" />
        <Label Text="{= this.GetFormatter().Format(local:System.String.Empty)}" />
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
		public static string Format(string missing, bool isVip) => isVip ? missing : "Standard";
	}

	public class InstanceFormatter
	{
		public string Format(string local) => $"Instance: {local}";
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
		public bool IsVip { get; set; } = true;
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
		public Repro.Helpers.InstanceFormatter GetFormatter() => new();
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
		Assert.Contains("global::Repro.Helpers.Formatter.Format(local:__source.Name) +", output, StringComparison.Ordinal);
		Assert.Contains("global::Repro.Helpers.Formatter.Format(global::Repro.MissingGen.Strings.Gold, __source.IsVip)", output, StringComparison.Ordinal);
		Assert.Contains("GetFormatter().Format(local:", output, StringComparison.Ordinal);
		Assert.Contains("String.Empty)", output, StringComparison.Ordinal);
		Assert.DoesNotContain("Repro.LocalGen.System", output, StringComparison.Ordinal);

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

	[Fact]
	public void InterpolationFormatClause_DottedFormatWithoutDataType_UsesRootClassType()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:Time="clr-namespace:Repro"
             x:Class="Repro.Views.ClockPage">
    <VerticalStackLayout>
        <Label Text="{= $'{Time:HH.mm}'}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro
{
	public class DummyType { }
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class ClockPage : ContentPage
	{
		public ClockPage() => InitializeComponent();
		public DateTime Time { get; set; }
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.HH", output, StringComparison.Ordinal);
		Assert.Contains("Time:HH.mm}\"", output, StringComparison.Ordinal);
	}

	[Fact]
	public void GlobalUsings_InCompilationScope_PreservesNamedArgumentMatchingPrefix()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Repro"
             xmlns:helpers="clr-namespace:Repro.Helpers"
             x:Class="Repro.Views.GlobalUsingPage">
    <VerticalStackLayout>
        <Label Text="{= helpers:Formatter.Format(local:DateTime.Now)}" />
        <Label Text="{= helpers:Formatter.Format(local:Dt.Now)}" />
        <Label Text="{= helpers:Formatter.FormatStatic(local:PI)}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
global using System;
global using Dt = System.DateTime;
global using static System.Math;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro
{
	public class DummyType { }
}

namespace Repro.Helpers
{
	public static class Formatter
	{
		public static string Format(DateTime local) => local.Year.ToString();
		public static string FormatStatic(double local) => local.ToString();
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class GlobalUsingPage : ContentPage
	{
		public GlobalUsingPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.DateTime", output, StringComparison.Ordinal);
		Assert.DoesNotContain("global::Repro.Dt", output, StringComparison.Ordinal);
		Assert.DoesNotContain("global::Repro.PI", output, StringComparison.Ordinal);
		Assert.Contains("global::Repro.Helpers.Formatter.Format(", output, StringComparison.Ordinal);
		Assert.Contains("DateTime.Now", output, StringComparison.Ordinal);
		Assert.Contains("Dt.Now", output, StringComparison.Ordinal);
		Assert.Contains("PI", output, StringComparison.Ordinal);
	}

	[Fact]
	public void ConditionalTernaryWithoutWhitespace_PreservesTernaryColonAndExpandsActualXmlnsBranch()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Repro"
             xmlns:Gold="clr-namespace:Repro.Resources"
             xmlns:missing="clr-namespace:Repro.Resources"
             x:Class="Repro.Views.TernaryPage"
             x:DataType="local:TernaryViewModel">
    <VerticalStackLayout>
        <Label Text="{= IsVip ? Gold:Standard}" />
        <Label Text="{= IsVip ? missing:Labels.Gold : Standard}" />
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
		public static string Gold => "GoldResource";
	}
}

namespace Repro
{
	public class TernaryViewModel : INotifyPropertyChanged
	{
		public bool IsVip { get; set; }
		public string Gold { get; set; } = "Gold";
		public string Standard { get; set; } = "Standard";
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class TernaryPage : ContentPage
	{
		public TernaryPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.Resources.Standard", output, StringComparison.Ordinal);
		Assert.Contains("global::Repro.Resources.Labels.Gold", output, StringComparison.Ordinal);
		Assert.Matches(@"__source\.IsVip\s*\?\s*__source\.Gold\s*:\s*__source\.Standard", output);
	}

	[Fact]
	public void TupleElementLabel_PreservesLabelWhenRootInScope_AndExpandsUnscopedXmlns()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Repro"
             xmlns:missing="clr-namespace:Repro.Resources"
             x:Class="Repro.Views.TuplePage"
             x:DataType="local:TupleViewModel">
    <VerticalStackLayout>
        <Label Text="{= (local:Name, Count).ToString()}" />
        <Label Text="{= (missing:Labels.Gold, Count).ToString()}" />
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
		public static string Gold => "Gold";
	}
}

namespace Repro
{
	public class TupleViewModel : INotifyPropertyChanged
	{
		public string Name { get; set; } = "Ada";
		public int Count { get; set; } = 3;
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class TuplePage : ContentPage
	{
		public TuplePage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.Name", output, StringComparison.Ordinal);
		Assert.Contains("global::Repro.Resources.Labels.Gold", output, StringComparison.Ordinal);
		Assert.Contains("__source.Name", output, StringComparison.Ordinal);
	}

	[Fact]
	public void InterpolationFormatClause_BlockBodiedLambdaLocal_PreservesFormatSpecifier()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:Time="clr-namespace:Repro"
             x:Class="Repro.Views.LambdaLocalFormatPage">
    <VerticalStackLayout>
        <Button Clicked="{= (s, e) => { var Time = System.DateTime.Now; this.Title = $'{Time:HH.mm}'; }}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro
{
	public class DummyType { }
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class LambdaLocalFormatPage : ContentPage
	{
		public LambdaLocalFormatPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.HH", output, StringComparison.Ordinal);
		Assert.Contains("Time:HH.mm}\"", output, StringComparison.Ordinal);
	}

	[Fact]
	public void InterpolationExpression_XmlnsMethodCall_PreservesNamedArgumentAndExpandsArgType()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Repro"
             xmlns:helpers="clr-namespace:Repro.Helpers"
             xmlns:missing="clr-namespace:Repro.Resources"
             x:Class="Repro.Views.InterpolatedCallPage"
             x:DataType="local:UserViewModel">
    <VerticalStackLayout>
        <Label Text="{= $'{helpers:Formatter.Format(local:User.Name)}'}" />
        <Label Text="{= $'{helpers:Formatter.FormatText(missing:Labels.Gold)}'}" />
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
		public static string Gold => "Gold";
	}
}

namespace Repro.Helpers
{
	public static class Formatter
	{
		public static string Format(string local) => local;
		public static string FormatText(string value) => value;
	}
}

namespace Repro
{
	public class UserInfo
	{
		public string Name { get; set; } = "Ada";
	}

	public class UserViewModel : INotifyPropertyChanged
	{
		public UserInfo User { get; set; } = new();
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class InterpolatedCallPage : ContentPage
	{
		public InterpolatedCallPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.User.", output, StringComparison.Ordinal);
		Assert.Contains("global::Repro.Helpers.Formatter.Format(", output, StringComparison.Ordinal);
		Assert.Contains("__source.User.Name", output, StringComparison.Ordinal);
		Assert.Contains("global::Repro.Resources.Labels.Gold", output, StringComparison.Ordinal);
	}

	[Fact]
	public void InterpolationFormatClause_ColonInsideFormatString_DoesNotExpandAsXmlns()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Repro"
             xmlns:res="clr-namespace:Repro.Resources"
             x:Class="Repro.Views.FormatStringColonPage"
             x:DataType="local:FormatViewModel">
    <VerticalStackLayout>
        <Label Text="{= $'{Value:res:Labels.Gold}'}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Resources
{
	public static class Labels
	{
		public static string Gold => "G";
	}
}

namespace Repro
{
	public class FormatViewModel : INotifyPropertyChanged
	{
		public DateTime Value { get; set; }
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class FormatStringColonPage : ContentPage
	{
		public FormatStringColonPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.Resources.Labels", output, StringComparison.Ordinal);
		Assert.Contains(":res:Labels.Gold}\"", output, StringComparison.Ordinal);
	}

	[Fact]
	public void XDataType_WithXTypeMarkupExtension_DottedFormatUsesDataType()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:Time="clr-namespace:Repro"
             xmlns:app="clr-namespace:Repro.ViewModels"
             x:Class="Repro.Views.XTypeClockPage"
             x:DataType="{x:Type app:ClockViewModel}">
    <VerticalStackLayout>
        <Label Text="{= $'{Time:HH.mm}'}" />
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
	public class DummyType { }
}

namespace Repro.ViewModels
{
	public class ClockViewModel : INotifyPropertyChanged
	{
		public DateTime Time { get; set; }
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class XTypeClockPage : ContentPage
	{
		public XTypeClockPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.HH", output, StringComparison.Ordinal);
		Assert.Contains("$\"{__source.Time:HH.mm}\"", output, StringComparison.Ordinal);
	}

	[Fact]
	public void VerbatimInterpolatedString_BothPrefixOrders_ParenthesizesXmlnsAliasAndHandlesEscapes()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:res="clr-namespace:Repro.Resources"
             x:Class="Repro.Views.VerbatimInterpolatedPage">
    <VerticalStackLayout>
        <Label Text='{= $@"{res:Labels.Gold}"}' />
        <Label Text='{= @$"{res:Labels.Gold}"}' />
        <Label Text='{= $@"prefix ""quoted"" C:\{res:Labels.Gold}\end"}' />
        <Label Text='{= @$"C:\""quoted""\{res:Labels.Gold}"}' />
        <Label Text='{= $@"val: {res:Labels.Count:D2}"}' />
        <Label Text="{= $@'{res:Labels.Gold}'}" />
        <Label Text="{= @$'{res:Labels.Gold}'}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Resources
{
	public static class Labels
	{
		public static string Gold => "Gold";
		public static int Count => 7;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class VerbatimInterpolatedPage : ContentPage
	{
		public VerbatimInterpolatedPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.Contains("$@\"{(global::Repro.Resources.Labels.Gold)}\"", output, StringComparison.Ordinal);
		Assert.Contains("@$\"{(global::Repro.Resources.Labels.Gold)}\"", output, StringComparison.Ordinal);
		Assert.Contains("(global::Repro.Resources.Labels.Count):D2", output, StringComparison.Ordinal);
	}

	[Fact]
	public void InterpolationExpression_WhitespaceAroundDotAndGenerics_ExpandsXmlnsType()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:res="clr-namespace:Repro.Resources"
             x:Class="Repro.Views.WhitespaceMemberAccessPage">
    <VerticalStackLayout>
        <Label Text="{= $'{res:Labels .Gold}'}" />
        <Label Text="{= $'{res:Labels. Gold}'}" />
        <Label Text="{= $'{res:GenericBox &lt;int&gt; .Value}'}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Resources
{
	public static class Labels
	{
		public static string Gold => "Gold";
	}

	public static class GenericBox<T>
	{
		public static string Value => typeof(T).Name;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class WhitespaceMemberAccessPage : ContentPage
	{
		public WhitespaceMemberAccessPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.Contains("(global::Repro.Resources.Labels .Gold)", output, StringComparison.Ordinal);
		Assert.Contains("(global::Repro.Resources.Labels. Gold)", output, StringComparison.Ordinal);
		Assert.Contains("(global::Repro.Resources.GenericBox <int> .Value)", output, StringComparison.Ordinal);
	}

	[Fact]
	public void QualifiedConstructorCall_PreservesNamedArgumentMatchingPrefix()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Repro"
             xmlns:missing="clr-namespace:Repro.Resources"
             x:Class="Repro.Views.QualifiedCtorPage"
             x:DataType="local:BoxViewModel">
    <VerticalStackLayout>
        <Label Text="{= new Repro.Box(local:Name).ToString()}" />
        <Label Text="{= new global::Repro.Box(local:Name).ToString()}" />
        <Label Text="{= new Repro.Box(missing:Labels.Gold).ToString()}" />
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
		public static string Gold => "Gold";
	}
}

namespace Repro
{
	public class Name { }

	public class Box
	{
		readonly string _value;
		public Box(string local) => _value = local;
		public override string ToString() => _value;
	}

	public class BoxViewModel : INotifyPropertyChanged
	{
		public string Name { get; set; } = "Ada";
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class QualifiedCtorPage : ContentPage
	{
		public QualifiedCtorPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.Name", output, StringComparison.Ordinal);
		Assert.Contains("__source.Name", output, StringComparison.Ordinal);
		Assert.Contains("global::Repro.Resources.Labels.Gold", output, StringComparison.Ordinal);
	}

	[Fact]
	public void AttachedPropertyInsideInterpolation_ParenthesizesAfterRewrite()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:m="clr-namespace:Repro.Attached"
             x:Class="Repro.Views.InterpolatedAttachedPage">
    <VerticalStackLayout>
        <Label Text="{= $'{this.(m:CustomGrid.Row)}'}" />
        <Label>
            <Label.Text>{= $@"{this.(m:CustomGrid.Row)}"}</Label.Text>
        </Label>
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Attached
{
	public static class CustomGrid
	{
		public static readonly BindableProperty RowProperty =
			BindableProperty.CreateAttached("Row", typeof(int), typeof(CustomGrid), 0);

		public static int GetRow(BindableObject view) => (int)view.GetValue(RowProperty);
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class InterpolatedAttachedPage : ContentPage
	{
		public InterpolatedAttachedPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.Contains("$\"{(global::Repro.Attached.CustomGrid.GetRow(this))}\"", output, StringComparison.Ordinal);
		Assert.Contains("$@\"{(global::Repro.Attached.CustomGrid.GetRow(this))}\"", output, StringComparison.Ordinal);
	}

	[Fact]
	public void RawStringLiterals_PreserveContentsAndTransformInterpolatedRawHoles()
	{
		var xaml =
""""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:res="clr-namespace:Repro.Resources"
             x:Class="Repro.Views.RawStringLiteralsPage">
    <VerticalStackLayout>
        <Label Text='{= """literal " $@"{global::System.String.Empty}" """}' />
        <Label Text='{= """raw ? a : b""" }' />
        <Label Text='{= $"""{res:Labels.Gold}""" }' />
        <Label Text='{= $$"""{res:Labels.Gold} {{res:Labels.Gold}} {{{res:Labels.Count:D2}}}""" }' />
    </VerticalStackLayout>
</ContentPage>
"""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Resources
{
	public static class Labels
	{
		public static string Gold => "Gold";
		public static int Count => 7;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class RawStringLiteralsPage : ContentPage
	{
		public RawStringLiteralsPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.Contains("\"\"\"literal \" $@\"{global::System.String.Empty}\" \"\"\"", output, StringComparison.Ordinal);
		Assert.Contains("\"\"\"raw ? a : b\"\"\"", output, StringComparison.Ordinal);
		Assert.Contains("$\"\"\"{(global::Repro.Resources.Labels.Gold)}\"\"\"", output, StringComparison.Ordinal);
		Assert.Contains("$$\"\"\"{res:Labels.Gold} {{(global::Repro.Resources.Labels.Gold)}} {{{(global::Repro.Resources.Labels.Count):D2}}}\"\"\"", output, StringComparison.Ordinal);
	}

	[Fact]
	public void ExpressionLambdaAndCallArguments_EarlierOutVar_PreservesInterpolationIdentifier()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:Time="clr-namespace:Repro"
             x:Class="Repro.Views.OutVarInterpolationPage">
    <VerticalStackLayout>
        <Button Clicked="{= (s, e) => this.UpdateTime(System.DateTime.TryParse('2026-11-15', out var Time), $'{Time:HH.mm}')}" />
        <Label Text="{= this.UpdateTime(System.DateTime.TryParse('2026-11-15', out var Time), $'{Time:HH.mm}')}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro
{
	public static class HH
	{
		public static string mm => "bad";
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class OutVarInterpolationPage : ContentPage
	{
		public OutVarInterpolationPage() => InitializeComponent();

		public string UpdateTime(bool ok, string formatted) => ok ? formatted : string.Empty;
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.HH", output, StringComparison.Ordinal);
		Assert.Contains("Time:HH.mm}\"", output, StringComparison.Ordinal);
	}

	[Fact]
	public void QueryExpression_RangeVariablesAndContinuationScoping_ResolvesXmlnsPrefixes()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Repro.Models"
             xmlns:other="clr-namespace:Repro.Alt"
             x:Class="App.Views.QueryScopePage">
    <VerticalStackLayout>
        <Label Text="{= string.Join(', ', from Person in People select global::Repro.Formatter.Format(local:Person))}" />
        <Label Text="{= string.Join(', ', from p in People let Alias = p.Name join Entry in Entries on p.Id equals Entry.Id select global::Repro.Formatter.Format(local:Alias, other:Entry.Tag))}" />
        <Label Text="{= string.Join(', ', from p in People join e in Entries on p.Id equals e.Id into Grouped select global::Repro.Formatter.Format(local:Grouped))}" />
        <Label Text="{= string.Join(', ', from Person in People select Person.Name into Summary select global::Repro.Formatter.Format(local:Summary))}" />
        <Label Text="{= string.Join(', ', from Person in People select Person.Name into Summary select global::Repro.Formatter.Format(local:Person.Default))}" />
    </VerticalStackLayout>
</ContentPage>
""";

		var codeBehind =
"""
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Alt
{
	public class DummyType { }
}

namespace Repro.Models
{
	public class Person
	{
		public static string Default => "fallback";
	}

	public class Alias { }

	public class Grouped { }

	public class Summary { }

	public class Entry { }
}

namespace Repro
{
	public record PersonItem(int Id, string Name);

	public record EntryItem(int Id, string Tag);

	public sealed class QuerySequence<T> : IEnumerable<T>
	{
		readonly List<T> _items;
		public QuerySequence(IEnumerable<T> items) => _items = new List<T>(items);

		public QuerySequence<TResult> Select<TResult>(Func<T, TResult> selector)
		{
			var results = new List<TResult>(_items.Count);
			foreach (var item in _items)
				results.Add(selector(item));
			return new QuerySequence<TResult>(results);
		}

		public QuerySequence<TResult> Join<TInner, TKey, TResult>(
			IEnumerable<TInner> inner,
			Func<T, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector,
			Func<T, TInner, TResult> resultSelector)
		{
			var innerList = new List<TInner>(inner);
			var results = new List<TResult>();
			foreach (var outerItem in _items)
			{
				var outerKey = outerKeySelector(outerItem);
				foreach (var innerItem in innerList)
				{
					if (EqualityComparer<TKey>.Default.Equals(outerKey, innerKeySelector(innerItem)))
						results.Add(resultSelector(outerItem, innerItem));
				}
			}
			return new QuerySequence<TResult>(results);
		}

		public QuerySequence<TResult> GroupJoin<TInner, TKey, TResult>(
			IEnumerable<TInner> inner,
			Func<T, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector,
			Func<T, IEnumerable<TInner>, TResult> resultSelector)
		{
			var innerList = new List<TInner>(inner);
			var results = new List<TResult>();
			foreach (var outerItem in _items)
			{
				var outerKey = outerKeySelector(outerItem);
				var matches = new List<TInner>();
				foreach (var innerItem in innerList)
				{
					if (EqualityComparer<TKey>.Default.Equals(outerKey, innerKeySelector(innerItem)))
						matches.Add(innerItem);
				}
				results.Add(resultSelector(outerItem, matches));
			}
			return new QuerySequence<TResult>(results);
		}

		public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public static class Formatter
	{
		public static string Format(object local) => local?.ToString() ?? string.Empty;
		public static string Format(object local, string other) => $"{local}:{other}";
	}
}

namespace App.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class QueryScopePage : ContentPage
	{
		public QueryScopePage() => InitializeComponent();

		public Repro.QuerySequence<Repro.PersonItem> People { get; } = new([new(1, "Ada")]);
		public Repro.QuerySequence<Repro.EntryItem> Entries { get; } = new([new(1, "A")]);
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.Contains("Formatter.Format(local:Person)", output, StringComparison.Ordinal);
		Assert.Contains("Formatter.Format(local:Alias, other:Entry.Tag)", output, StringComparison.Ordinal);
		Assert.Contains("Formatter.Format(local:Grouped)", output, StringComparison.Ordinal);
		Assert.Contains("Formatter.Format(local:Summary)", output, StringComparison.Ordinal);
		Assert.DoesNotContain("global::Repro.Models.Alias", output, StringComparison.Ordinal);
		Assert.DoesNotContain("global::Repro.Models.Grouped", output, StringComparison.Ordinal);
		Assert.DoesNotContain("global::Repro.Models.Summary", output, StringComparison.Ordinal);
		Assert.DoesNotContain("global::Repro.Alt.Entry", output, StringComparison.Ordinal);
		Assert.DoesNotContain("Formatter.Format(local:global::Repro.Models.Person", output, StringComparison.Ordinal);
		Assert.Contains("Formatter.Format(global::Repro.Models.Person.Default)", output, StringComparison.Ordinal);
	}

	[Fact]
	public void GenericArity_PreservesNamedArgumentsAcrossTypeAndMethodOverloads()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:plain="clr-namespace:Repro"
             xmlns:generic="clr-namespace:Repro"
             xmlns:pair="clr-namespace:Repro"
             x:Class="Repro.Views.GenericArityPage"
             x:DataType="plain:BoxViewModel">
    <VerticalStackLayout>
        <Label Text="{= new global::Repro.Box(plain:Name).ToString()}" />
        <Label Text="{= new global::Repro.Box&lt;int&gt;(generic:Name).ToString()}" />
        <Label Text="{= new global::Repro.Box&lt;int, string&gt;(pair:Name).ToString()}" />
        <Label Text="{= new plain:Box(plain:Name).ToString()}" />
        <Label Text="{= new plain:Box&lt;int&gt;(generic:Name).ToString()}" />
        <Label Text="{= new global::Repro.Container.Inner(plain:Name).ToString()}" />
        <Label Text="{= new global::Repro.Container.Inner&lt;int&gt;(generic:Name).ToString()}" />
        <Label Text="{= global::Repro.Factory.Create(plain:Name)}" />
        <Label Text="{= global::Repro.Factory.Create&lt;int&gt;(generic:Name)}" />
        <Label Text="{= global::Repro.Factory.Create&lt;int, string&gt;(pair:Name)}" />
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
	public class Name { }

	public class Box
	{
		readonly string _value;
		public Box(string plain) => _value = plain;
		public override string ToString() => _value;
	}

	public class Box<T>
	{
		readonly string _value;
		public Box(string generic) => _value = generic;
		public override string ToString() => _value;
	}

	public class Box<T1, T2>
	{
		readonly string _value;
		public Box(string pair) => _value = pair;
		public override string ToString() => _value;
	}

	public class Container
	{
		public class Inner
		{
			readonly string _value;
			public Inner(string plain) => _value = plain;
			public override string ToString() => _value;
		}

		public class Inner<T>
		{
			readonly string _value;
			public Inner(string generic) => _value = generic;
			public override string ToString() => _value;
		}
	}

	public static class Factory
	{
		public static string Create(string plain) => plain;
		public static string Create<T>(string generic) => generic;
		public static string Create<T1, T2>(string pair) => pair;
	}

	public class BoxViewModel : INotifyPropertyChanged
	{
		public string Name { get; set; } = "Ada";
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class GenericArityPage : ContentPage
	{
		public GenericArityPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.Name", output, StringComparison.Ordinal);
		Assert.Contains("__source.Name", output, StringComparison.Ordinal);
		Assert.Contains("new global::Repro.Box(plain:", output, StringComparison.Ordinal);
		Assert.Contains("new global::Repro.Box<int>(generic:", output, StringComparison.Ordinal);
		Assert.Contains("new global::Repro.Box<int, string>(pair:", output, StringComparison.Ordinal);
		Assert.Contains("new global::Repro.Container.Inner(plain:", output, StringComparison.Ordinal);
		Assert.Contains("new global::Repro.Container.Inner<int>(generic:", output, StringComparison.Ordinal);
		Assert.Contains("Factory.Create(plain:", output, StringComparison.Ordinal);
		Assert.Contains("Factory.Create<int>(generic:", output, StringComparison.Ordinal);
		Assert.Contains("Factory.Create<int, string>(pair:", output, StringComparison.Ordinal);
	}

	[Fact]
	public void InferredGenericMethod_PreservesNamedArgumentAndRejectsUninferableArity()
	{
		var xaml = """
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:helpers="clr-namespace:Repro.Helpers"
	xmlns:local="clr-namespace:Repro.Models"
	x:Class="Repro.Views.InferredGenericMethodPage"
	x:DataType="local:InferredGenericViewModel">
	<VerticalStackLayout>
		<Label Text="{= helpers:Formatter.Format(local:Name)}" />
		<Label Text="{= helpers:Formatter.Uninferable&lt;int&gt;(local:Name)}" />
		<Label Text="{= helpers:Formatter.Uninferable(local:Tag.Value)}" />
	</VerticalStackLayout>
</ContentPage>
""";

		var codeBehind = """
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Models
{
	public class Name { }

	public static class Tag
	{
		public const string Value = "tag-value";
	}

	public class InferredGenericViewModel : INotifyPropertyChanged
	{
		public string Name { get; set; } = "Ada";
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

namespace Repro.Helpers
{
	public static class Formatter
	{
		public static string Format(int other) => other.ToString();
		public static string Format<T>(T local) => local?.ToString() ?? string.Empty;
		public static string Uninferable(string other) => other;
		public static string Uninferable<T>(string local) => local;
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class InferredGenericMethodPage : ContentPage
	{
		public InferredGenericMethodPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.DoesNotContain("global::Repro.Models.Name", output, StringComparison.Ordinal);
		Assert.Contains("Formatter.Format(local:", output, StringComparison.Ordinal);
		Assert.Contains("__source.Name", output, StringComparison.Ordinal);
		Assert.Contains("Formatter.Uninferable<int>(local:", output, StringComparison.Ordinal);
		Assert.Contains("Formatter.Uninferable(global::Repro.Models.Tag.Value)", output, StringComparison.Ordinal);
	}

	[Fact]
	public void GlobalUsingStaticMember_PreservesInterpolationFormatClause()
	{
		var xaml = """
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:ClockTime="clr-namespace:Repro.Formats"
	x:Class="Repro.Views.GlobalUsingStaticFormatPage">
	<Label Text="{= $'{ClockTime:HH.mm}'}" />
</ContentPage>
""";

		var codeBehind = """
global using static Repro.GlobalClock;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Formats
{
	public static class HH
	{
		public static string mm => "decoy";
	}
}

namespace Repro
{
	public static class GlobalClock
	{
		public static DateTime ClockTime => new DateTime(2026, 11, 15, 14, 30, 0);
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class GlobalUsingStaticFormatPage : ContentPage
	{
		public GlobalUsingStaticFormatPage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.Contains("{ClockTime:HH.mm}", output, StringComparison.Ordinal);
		Assert.DoesNotContain("global::Repro.Formats.HH", output, StringComparison.Ordinal);
	}

	[Fact]
	public void NestedTypesInRootAndBaseTypes_PreserveNamedArguments()
	{
		var xaml = """
<?xml version="1.0" encoding="utf-8" ?>
<views:BaseModePage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Repro.Models"
	xmlns:views="clr-namespace:Repro.Views"
	x:Class="Repro.Views.NestedTypeScopePage">
	<VerticalStackLayout>
		<Label Text="{= this.FormatMode(local:Mode.Active)}" />
		<Label Text="{= this.FormatBaseMode(local:BaseMode.Idle)}" />
	</VerticalStackLayout>
</views:BaseModePage>
""";

		var codeBehind = """
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Models
{
	public class Mode { }
}

namespace Repro.Views
{
	public class BaseModePage : ContentPage
	{
		public enum BaseMode
		{
			Idle
		}
	}

	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class NestedTypeScopePage : BaseModePage
	{
		public enum Mode
		{
			Active
		}

		public NestedTypeScopePage() => InitializeComponent();
		public string FormatMode(Mode local) => local.ToString();
		public string FormatBaseMode(BaseMode local) => local.ToString();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.Contains("this.FormatMode(local:", output, StringComparison.Ordinal);
		Assert.Contains("Mode.Active", output, StringComparison.Ordinal);
		Assert.DoesNotContain("global::Repro.Models.Mode", output, StringComparison.Ordinal);
		Assert.Contains("this.FormatBaseMode(local:", output, StringComparison.Ordinal);
		Assert.Contains("BaseMode.Idle", output, StringComparison.Ordinal);
		Assert.DoesNotContain("global::Repro.Models.BaseMode", output, StringComparison.Ordinal);
	}

	[Fact]
	public void ForStatementConditionAndInitializerDeclarations_PreserveInterpolationFormatClause()
	{
		var xaml = """
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:CondTime="clr-namespace:Repro.Formats"
	xmlns:InitTime="clr-namespace:Repro.Formats"
	x:Class="Repro.Views.ForHeaderScopePage">
	<Button Clicked="{= (s, e) => { for (; System.DateTime.TryParse('2026-11-15', out var CondTime);) { this.Title = $'{CondTime:HH.mm}'; break; } bool ok = false; for (ok = System.DateTime.TryParse('2026-11-15', out var InitTime); ok;) { this.Title = $'{InitTime:HH.mm}'; break; } this.Title = $'{CondTime:HH.mm}'; } }" />
</ContentPage>
""";

		var codeBehind = """
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Formats
{
	public static class HH
	{
		public static string mm => "decoy";
	}
}

namespace Repro.Views
{
	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class ForHeaderScopePage : ContentPage
	{
		public ForHeaderScopePage() => InitializeComponent();
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.Contains("{CondTime:HH.mm}", output, StringComparison.Ordinal);
		Assert.Contains("{InitTime:HH.mm}", output, StringComparison.Ordinal);
		// Outside the for statement, CondTime is out of scope, so xmlns expansion applies.
		Assert.Contains("global::Repro.Formats.HH.mm", output, StringComparison.Ordinal);
	}

	[Fact]
	public void ResolvedXmlnsTypeWithInvalidMemberChainOnLocalProperty_ExpandsToXmlnsType()
	{
		var xaml = """
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Repro.Resources"
	x:Class="Repro.Views.AmbiguousMemberChainPage">
	<VerticalStackLayout>
		<Label Text="{= this.Format(local:Labels.Gold)}" />
		<Label Text="{= this.Format(local:Labels.Empty)}" />
		<Label Text="{= this.Format(local:Holder.Gold)}" />
	</VerticalStackLayout>
</ContentPage>
""";

		var codeBehind = """
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Repro.Resources
{
	public static class Labels
	{
		public const string Gold = "GoldLabel";
		public const string Empty = "EmptyLabel";
	}

	public static class Holder
	{
		public const string Gold = "XmlnsHolder";
	}
}

namespace Repro.Views
{
	public class HolderModel
	{
		public string Gold => "PageHolder";
	}

	[XamlProcessing(XamlInflator.SourceGen)]
	public partial class AmbiguousMemberChainPage : ContentPage
	{
		public AmbiguousMemberChainPage() => InitializeComponent();
		public string Labels => "PageString";
		public HolderModel Holder { get; } = new HolderModel();
		public string Format(string local) => local;
	}
}
""";

		var (result, output) = RunGenerator(xaml, codeBehind);
		Assert.Empty(result.Diagnostics);
		Assert.NotNull(output);
		Assert.Contains("this.Format(global::Repro.Resources.Labels.Gold)", output, StringComparison.Ordinal);
		Assert.Contains("this.Format(global::Repro.Resources.Labels.Empty)", output, StringComparison.Ordinal);
		Assert.Contains("this.Format(local:", output, StringComparison.Ordinal);
		Assert.DoesNotContain("global::Repro.Resources.Holder.Gold", output, StringComparison.Ordinal);
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
