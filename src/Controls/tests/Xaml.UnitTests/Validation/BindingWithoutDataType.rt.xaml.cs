using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class BindingWithoutDataType : ContentPage
{
	public BindingWithoutDataType() => InitializeComponent();

	[TestFixture]
class Tests
	{
		[Test]
		public void WarnsOnBindingWithoutDataType([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				// This is a warning, not an error, so compilation should succeed
				// The warning is checked through build output, not exception
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(BindingWithoutDataType)));
			}
			else if (inflator == XamlInflator.Runtime)
			{
				// Runtime doesn't produce warnings, just works
				Assert.DoesNotThrow(() => new BindingWithoutDataType(inflator));
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class BindingWithoutDataType : ContentPage
{
	public BindingWithoutDataType() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(BindingWithoutDataType));
				// Warning, not error
				Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2022" && d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Warning), Is.True);
			}
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
