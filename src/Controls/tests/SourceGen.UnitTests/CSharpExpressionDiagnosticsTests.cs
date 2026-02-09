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
}
