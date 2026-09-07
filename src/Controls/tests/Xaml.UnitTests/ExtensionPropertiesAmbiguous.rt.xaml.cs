using System.Linq;
using Controls.Xaml.UnitTests.ExternalAssembly;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// AmbiguousExtensions declares MyAmbiguous for both IView and BindableObject. Both apply to a Label and
// neither receiver is more derived than the other, so no inflator is allowed to pick one.
public partial class ExtensionPropertiesAmbiguous : ContentPage
{
	public ExtensionPropertiesAmbiguous() => InitializeComponent();

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
			{
				var e = Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(ExtensionPropertiesAmbiguous)));
				Assert.Equal(BuildExceptionCode.ExtensionPropertyResolution, e.Code);
			}
			else if (inflator == XamlInflator.Runtime)
			{
				var e = Assert.Throws<XamlParseException>(() => new ExtensionPropertiesAmbiguous(inflator));
				Assert.Contains("MyAmbiguous", e.Message, System.StringComparison.Ordinal);
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				// the container must be a real extension container, and the mock compilation uses a Roslyn
				// that cannot parse extension blocks, so it is referenced as metadata instead
				var result = CreateMauiCompilation()
					.AddReferences(MetadataReference.CreateFromFile(typeof(AmbiguousExtensions).Assembly.Location))
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class ExtensionPropertiesAmbiguous : ContentPage
{
	public ExtensionPropertiesAmbiguous() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(ExtensionPropertiesAmbiguous));
				Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2015"));
			}
		}
	}
}
