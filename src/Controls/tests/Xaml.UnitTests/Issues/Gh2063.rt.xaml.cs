using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2063 : ContentPage
{
	public Gh2063() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void DetailedErrorMessageOnMissingXmlnsDeclaration(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh2063)));
			if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh2063(inflator));
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh2063 : ContentPage
{
	public Gh2063() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Gh2063));
				Assert.NotEmpty(result.Diagnostics);
			}
		}
	}
}