using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// OutOfScopeLabelExtensions lives in a namespace no xmlns maps, so the name resolves to nothing.
public partial class ScopedExtensionPropertiesOutOfScope : ContentPage
{
	public ScopedExtensionPropertiesOutOfScope() => InitializeComponent();

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
				var e = Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(ScopedExtensionPropertiesOutOfScope)));
				Assert.Equal(BuildExceptionCode.MemberResolution, e.Code);
			}
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new ScopedExtensionPropertiesOutOfScope(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation("Microsoft.Maui.Controls.Xaml.UnitTests")
					.AddReferences(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(ScopedExtensionPropertiesOutOfScope).Assembly.Location))
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class ScopedExtensionPropertiesOutOfScope : ContentPage
{
	public ScopedExtensionPropertiesOutOfScope() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(ScopedExtensionPropertiesOutOfScope));
				Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2002"));
			}
		}
	}
}
