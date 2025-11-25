using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ResourceDictMissingKey : ContentPage
{
	public ResourceDictMissingKey() => InitializeComponent();

	[TestFixture]
class Tests
	{
		[Test]
		public void ThrowsOnResourceWithoutKey([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(7, 4), () => MockCompiler.Compile(typeof(ResourceDictMissingKey)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(7, 4), () => new ResourceDictMissingKey(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class ResourceDictMissingKey : ContentPage
{
	public ResourceDictMissingKey() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(ResourceDictMissingKey));
				Assert.That(result.Diagnostics, Is.Not.Empty);
				Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2126"), Is.True);
			}
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
