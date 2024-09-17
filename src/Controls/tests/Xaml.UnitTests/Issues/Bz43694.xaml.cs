using System.Linq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Bz43694 : ContentPage
{
	public Bz43694() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test] public void xStaticWithOnPlatformChildInRD([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(9, 6), () => MockCompiler.Compile(typeof(Bz43694)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(9, 6), () => new Bz43694(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.CreateMauiCompilation().RunMauiSourceGenerator(typeof(Bz43694));
				Assert.That(result.Diagnostics.Any());
			}
			else
				Assert.Ignore("Unknown inflator");
		}
	}
}