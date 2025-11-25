using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class ViewWithWriteOnlyProperty : View
{
	public string WriteOnly { set { } }
}

public partial class PropertyResolution : ContentPage
{
	public PropertyResolution() => InitializeComponent();

	[TestFixture]
class Tests
	{
		[Test]
		public void ThrowsOnPropertyWithoutGetter([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(6, 45), () => MockCompiler.Compile(typeof(PropertyResolution)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(6, 45), () => new PropertyResolution(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class ViewWithWriteOnlyProperty : View
{
	public string WriteOnly { set { } }
}

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class PropertyResolution : ContentPage
{
	public PropertyResolution() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(PropertyResolution));
				Assert.That(result.Diagnostics, Is.Not.Empty);
				Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2006"), Is.True);
			}
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
