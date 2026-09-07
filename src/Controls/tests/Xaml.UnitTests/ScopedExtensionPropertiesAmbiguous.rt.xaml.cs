using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Two containers in scope declare ScopedAmbiguous for unrelated receivers, both applicable to a Label.
public partial class ScopedExtensionPropertiesAmbiguous : ContentPage
{
	public ScopedExtensionPropertiesAmbiguous() => InitializeComponent();

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
				var e = Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(ScopedExtensionPropertiesAmbiguous)));
				Assert.Equal(BuildExceptionCode.ExtensionPropertyAmbiguous, e.Code);
			}
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new ScopedExtensionPropertiesAmbiguous(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation("Microsoft.Maui.Controls.Xaml.UnitTests")
					.AddReferences(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(ScopedExtensionPropertiesAmbiguous).Assembly.Location))
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class ScopedExtensionPropertiesAmbiguous : ContentPage
{
	public ScopedExtensionPropertiesAmbiguous() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(ScopedExtensionPropertiesAmbiguous));
				Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2016"));
			}
		}
	}
}
