using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ResourceDictionaryWithInvalidSource : ContentPage
{
	public ResourceDictionaryWithInvalidSource() => InitializeComponent();

	public class Tests
	{
		[Theory]
		[Values]
		public void InvalidSourceThrows(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				BuildExceptionHelper.AssertThrows(() => MockCompiler.Compile(typeof(ResourceDictionaryWithInvalidSource)), 8, 33);
			else if (inflator == XamlInflator.Runtime)
				XamlParseExceptionHelper.AssertThrows(() => new ResourceDictionaryWithInvalidSource(inflator), 8, 33);
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
			else
			{
				// TODO: Convert to [Theory(Skip="reason")] or use conditional Skip attribute

			}
		}
	}
}