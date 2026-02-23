using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation feature")]
public partial class MultipleDataTemplateChildElements : BindableObject
{
	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void ThrowXamlParseException(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(MultipleDataTemplateChildElements), out var md, out var hasLoggedErrors);
				Assert.True(hasLoggedErrors);
			}
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new MultipleDataTemplateChildElements(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class MultipleDataTemplateChildElements : BindableObject
{
	public MultipleDataTemplateChildElements() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(MultipleDataTemplateChildElements));
				Assert.NotEmpty(result.Diagnostics);
			}
			// else - nothing to test for other inflators
		}
	}
}
