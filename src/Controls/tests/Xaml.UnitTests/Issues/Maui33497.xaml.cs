using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly.Maui33497;
using Xunit;

// XmlnsDefinition in the CURRENT assembly that points to an EXTERNAL assembly.
// This is the actual user scenario from https://github.com/dotnet/maui/issues/33497
// The user has this in their app's Imports.cs:
//   [assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", 
//       "MyApp.Core.Api", AssemblyName = "MyApp.Core")]
// This should make types from MyApp.Core.Api available via the global xmlns.
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/dotnet/maui/global",
	"Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly.Maui33497",
	AssemblyName = "Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly")]

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// View with a BindableProperty that accepts AlertType enum to test type resolution.
/// </summary>
public class Maui33497View : ContentView
{
	public static readonly BindableProperty AlertProperty =
		BindableProperty.Create(nameof(Alert), typeof(AlertType), typeof(Maui33497View), AlertType.Info);

	public AlertType Alert
	{
		get => (AlertType)GetValue(AlertProperty);
		set => SetValue(AlertProperty, value);
	}
}

/// <summary>
/// Test for https://github.com/dotnet/maui/issues/33497
/// 
/// Scenario: The consuming app (this test assembly) defines an XmlnsDefinition in Imports.cs:
/// [assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", 
///     "Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly.Maui33497",
///     AssemblyName = "Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly")]
/// 
/// This should make AlertType from the external assembly available via the global xmlns.
/// 
/// Bug: XmlnsDefinition with AssemblyName pointing to an external assembly doesn't work
/// for the global xmlns. The type cannot be resolved.
/// </summary>
public partial class Maui33497 : ContentPage
{
	public Maui33497()
	{
		InitializeComponent();
	}

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void GlobalXmlnsFromExternalAssemblyResolvesType(XamlInflator inflator)
		{
			// This should not throw - the type AlertType should be resolved from the external assembly
			// via the XmlnsDefinition for global xmlns defined in that assembly
			var page = new Maui33497(inflator);
			Assert.Equal(AlertType.Warning, page.TestView.Alert);
		}
	}
}
