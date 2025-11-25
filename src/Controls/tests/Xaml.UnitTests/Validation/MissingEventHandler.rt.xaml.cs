using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class MissingEventHandler : ContentPage
{
	public MissingEventHandler() => InitializeComponent();

	[TestFixture]
class Tests
	{
		[Test]
		public void ThrowsOnMissingEventHandler([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(5, 17), () => MockCompiler.Compile(typeof(MissingEventHandler)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(5, 17), () => new MissingEventHandler(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class MissingEventHandler : ContentPage
{
	public MissingEventHandler() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(MissingEventHandler));
				Assert.That(result.Diagnostics, Is.Not.Empty);
				Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2007"), Is.True);
			}
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
