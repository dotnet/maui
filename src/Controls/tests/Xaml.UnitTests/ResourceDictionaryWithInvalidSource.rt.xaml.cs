using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ResourceDictionaryWithInvalidSource : ContentPage
{
	public ResourceDictionaryWithInvalidSource() => InitializeComponent();

	class Tests
	{
		[Test]
		public void InvalidSourceThrows([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(8, 33), () => MockCompiler.Compile(typeof(ResourceDictionaryWithInvalidSource)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(8, 33), () => new ResourceDictionaryWithInvalidSource(inflator));
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
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}
			else
			{
				Assert.Ignore("Nothing to test here");
			}
		}
	}
}