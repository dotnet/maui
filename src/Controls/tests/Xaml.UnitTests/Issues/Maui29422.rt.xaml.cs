using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui29422 : ContentPage
{
	public Maui29422() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void PropertyElementAttributesGenerateWarning(XamlInflator inflator)
		{
			// Property elements with attributes should parse successfully (attributes are ignored with a warning)
			// The warning is logged but parsing continues
			if (inflator == XamlInflator.XamlC)
			{
				// XamlC should compile successfully and log a warning (not throw)
				MockCompiler.Compile(typeof(Maui29422), out var hasLoggedErrors);
				Assert.False(hasLoggedErrors);
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				// SourceGen should compile successfully and produce a warning diagnostic
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui29422));
				// The generator should succeed (no errors)
				Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
				// Should have a warning about property element with attributes (MAUIX2006)
				Assert.NotEmpty(result.Diagnostics.Where(d => d.Id == "MAUIX2006"));
			}
			else
			{
				// For runtime, the page should load successfully (warning is just logged via Debug.WriteLine)
				var page = new Maui29422(inflator);
				Assert.NotNull(page);
			}
		}
	}
}
