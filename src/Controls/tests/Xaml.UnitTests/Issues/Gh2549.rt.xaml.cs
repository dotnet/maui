using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2549 : ContentPage
{
	public Gh2549() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ErrorOnUnknownXmlnsForDataType(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh2549)));
			if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh2549(inflator));
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh2549 : ContentPage
{
	public Gh2549() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Gh2549));
				Assert.NotEmpty(result.Diagnostics);
			}
		}
	}
}
