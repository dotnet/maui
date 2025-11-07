using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh11711 : ContentPage
{
	public Gh11711() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void FormatExceptionAreCaught(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh11711)));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh11711 : ContentPage
{
	public Gh11711() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Gh11711));

				Assert.NotEmpty(result.Diagnostics);
			}
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh11711(inflator));
		}
	}
}
