using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// XAML has no equivalent of a `using` directive, so an unqualified attribute name is never resolved
// against extension containers. The three inflators must agree and reject it.
public partial class ExtensionPropertiesUnqualified : ContentPage
{
	public ExtensionPropertiesUnqualified() => InitializeComponent();

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
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(ExtensionPropertiesUnqualified)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new ExtensionPropertiesUnqualified(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class ExtensionPropertiesUnqualified : ContentPage
{
	public ExtensionPropertiesUnqualified() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(ExtensionPropertiesUnqualified));
				Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2002"));
			}
		}
	}
}
