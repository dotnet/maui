using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XStaticResolution : ContentPage
{
	public XStaticResolution() => InitializeComponent();

	[TestFixture]
class Tests
	{
		[Test]
		public void ThrowsOnUnresolvedXStatic([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(6, 17), () => MockCompiler.Compile(typeof(XStaticResolution)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(6, 17), () => new XStaticResolution(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class XStaticResolution : ContentPage
{
	public XStaticResolution() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(XStaticResolution));
				Assert.That(result.Diagnostics, Is.Not.Empty);
				Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2101"), Is.True);
			}
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
