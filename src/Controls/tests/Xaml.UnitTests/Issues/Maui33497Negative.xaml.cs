using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly.Maui33497Negative;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// View with a BindableProperty that accepts ExternalGlobalType enum.
/// </summary>
public class Maui33497NegativeView : ContentView
{
	public static readonly BindableProperty ValueProperty =
		BindableProperty.Create(nameof(Value), typeof(ExternalGlobalType), typeof(Maui33497NegativeView), ExternalGlobalType.Value1);

	public ExternalGlobalType Value
	{
		get => (ExternalGlobalType)GetValue(ValueProperty);
		set => SetValue(ValueProperty, value);
	}
}

/// <summary>
/// Test for https://github.com/dotnet/maui/issues/33497 - Negative case
/// 
/// Scenario: The ExternalAssembly declares an XmlnsDefinition for the global xmlns:
/// [assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", 
///     "Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly.Maui33497Negative")]
/// 
/// Expected behavior: This XmlnsDefinition should be IGNORED by the consuming assembly.
/// External assemblies cannot add types to the global xmlns of consuming assemblies.
/// 
/// The XAML tries to use ExternalGlobalType via global xmlns, which should fail
/// because the external assembly's XmlnsDefinition for global xmlns is not loaded.
/// </summary>
public partial class Maui33497Negative : ContentPage
{
	public Maui33497Negative()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void ExternalAssemblyCannotAddToGlobalXmlns(XamlInflator inflator)
		{
			// This test verifies that XmlnsDefinition for global xmlns from external assemblies
			// is correctly ignored. The type ExternalGlobalType should NOT be resolvable.
			if (inflator == XamlInflator.XamlC)
			{
				// XamlC should throw a BuildException because the type cannot be resolved
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Maui33497Negative)));
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				// SourceGen should produce a diagnostic error
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Maui33497Negative : ContentPage
{
    public Maui33497Negative() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Maui33497Negative));
				Assert.NotEmpty(result.Diagnostics);
			}
		}
	}
}
