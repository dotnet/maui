using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui29422 : ContentPage
{
	public Maui29422() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void PropertyElementAttributesGenerateWarning([Values] XamlInflator inflator)
		{
			// Property elements with attributes should parse successfully (attributes are ignored with a warning)
			// The warning is logged but parsing continues
			if (inflator == XamlInflator.XamlC)
			{
				// XamlC should compile successfully and log a warning (not throw)
				MockCompiler.Compile(typeof(Maui29422), out var hasLoggedErrors);
				Assert.That(hasLoggedErrors, Is.False);
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				// SourceGen should compile successfully and produce a warning diagnostic
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui29422));
				// The generator should succeed (no errors)
				Assert.That(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty);
				// Should have a warning about property element with attributes (MAUIX2006)
				Assert.That(result.Diagnostics.Where(d => d.Id == "MAUIX2006"), Is.Not.Empty);
			}
			else
			{
				// For runtime, the page should load successfully (warning is just logged via Debug.WriteLine)
				var page = new Maui29422(inflator);
				Assert.That(page, Is.Not.Null);
			}
		}
	}
}
