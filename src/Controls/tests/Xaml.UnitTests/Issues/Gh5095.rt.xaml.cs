using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh5095 : ContentPage
{
	public Gh5095() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ThrowsOnInvalidXaml([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh5095)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh5095(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh5095 : ContentPage
{
	public Gh5095() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Gh5095));

				//FIXME check the diagnostic code
				Assert.That(result.Diagnostics.Length, Is.EqualTo(1));
			}
		}
	}
}
