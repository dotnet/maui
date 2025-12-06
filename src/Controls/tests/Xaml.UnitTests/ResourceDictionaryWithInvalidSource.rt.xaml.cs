using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation feature")]
public partial class ResourceDictionaryWithInvalidSource : ContentPage
{
	public ResourceDictionaryWithInvalidSource() => InitializeComponent();

	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void InvalidSourceThrows(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				XamlExceptionAssert.ThrowsBuildException(8, 33, () => MockCompiler.Compile(typeof(ResourceDictionaryWithInvalidSource)));
			else if (inflator == XamlInflator.Runtime)
				XamlExceptionAssert.ThrowsXamlParseException(8, 33, () => new ResourceDictionaryWithInvalidSource(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class ResourceDictionaryWithInvalidSource : ContentPage
{
	public ResourceDictionaryWithInvalidSource() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(ResourceDictionaryWithInvalidSource));
				Assert.NotEmpty(result.Diagnostics);
			}
			// else - nothing to test for other inflators
		}
	}
}