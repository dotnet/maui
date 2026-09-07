using System.Linq;
using Controls.Xaml.UnitTests.ExternalAssembly;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// ExternalLabelExtensions is a real extension container, but it declares no extension property named Text.
// Label does have a Text property: stripping the qualifier and assigning it would silently do something the
// markup never asked for, so all three inflators have to fail instead. XamlC and SourceGen fail the build,
// which is also why Label.Text can never be assigned; the runtime case is asserted in ExtensionProperties.
public partial class ExtensionPropertiesQualifiedFallback : ContentPage
{
	public ExtensionPropertiesQualifiedFallback() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void Throw(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(ExtensionPropertiesQualifiedFallback)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new ExtensionPropertiesQualifiedFallback(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.AddReferences(MetadataReference.CreateFromFile(typeof(ExternalLabelExtensions).Assembly.Location))
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class ExtensionPropertiesQualifiedFallback : ContentPage
{
	public ExtensionPropertiesQualifiedFallback() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(ExtensionPropertiesQualifiedFallback));
				Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2002"));
				Assert.DoesNotContain(".Text =", result.GeneratedInitializeComponent(), System.StringComparison.Ordinal);
			}
		}
	}
}
