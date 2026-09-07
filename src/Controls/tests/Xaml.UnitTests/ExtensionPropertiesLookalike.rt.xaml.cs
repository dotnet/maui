using System.Linq;
using Controls.Xaml.UnitTests.ExternalAssembly;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// LookalikeExtensions is a plain static class whose members merely have the shape of lowered extension
// property accessors. It carries no extension declaration, so it is not an extension container and none of
// the inflators may assign through it. See ExtensionPropertyConventions.
public partial class ExtensionPropertiesLookalike : ContentPage
{
	public ExtensionPropertiesLookalike() => InitializeComponent();

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
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(ExtensionPropertiesLookalike)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new ExtensionPropertiesLookalike(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.AddReferences(MetadataReference.CreateFromFile(typeof(LookalikeExtensions).Assembly.Location))
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class ExtensionPropertiesLookalike : ContentPage
{
	public ExtensionPropertiesLookalike() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(ExtensionPropertiesLookalike));
				Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2002"));
				Assert.DoesNotContain("set_Lookalike", result.GeneratedInitializeComponent(), System.StringComparison.Ordinal);
			}
		}
	}
}
