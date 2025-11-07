using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2064 : ContentPage
{
	public Gh2064() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void ReportMissingTargetTypeOnStyle(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh2064)));
			if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh2064(inflator));
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh2064 : ContentPage
{
	public Gh2064() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Gh2064));
				Assert.NotEmpty(result.Diagnostics);
			}
		}
	}
}
