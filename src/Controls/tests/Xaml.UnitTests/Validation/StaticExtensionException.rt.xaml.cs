using System.Linq;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StaticExtensionException : ContentPage
{
	public StaticExtensionException() => InitializeComponent();

	[TestFixture]
	class Issue2115
	{
		[Test]
		public void xStaticThrowsMeaningfullException([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(6, 34), () => new StaticExtensionException(inflator));
			else if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(6, 34), () => MockCompiler.Compile(typeof(StaticExtensionException)));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class StaticExtensionException : ContentPage
{
	public StaticExtensionException() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(StaticExtensionException));
				Assert.That(result.Diagnostics.Any());
			}
			else
				Assert.Ignore("Unknown inflator");
		}
	}
}