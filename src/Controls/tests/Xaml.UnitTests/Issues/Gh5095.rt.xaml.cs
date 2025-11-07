using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh5095 : ContentPage
{
	public Gh5095() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void ThrowsOnInvalidXaml(XamlInflator inflator)
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
				Assert.Single(result.Diagnostics);
			}
		}
	}
}
