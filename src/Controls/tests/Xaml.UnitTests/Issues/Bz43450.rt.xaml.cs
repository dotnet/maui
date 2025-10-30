using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz43450 : ContentPage
{
	public Bz43450() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void DoesNotAllowGridRowDefinition([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Bz43450)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Bz43450(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Bz43450 : ContentPage
{
	public Bz43450() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Bz43450));
				Assert.That(result.Diagnostics, Is.Not.Empty);

			}
			else
				Assert.Ignore("Unknown inflator");
		}
	}
}