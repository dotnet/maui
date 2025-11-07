using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz43694 : ContentPage
{
	public Bz43694() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void xStaticWithOnPlatformChildInRD(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.ThrowsAny<Exception>(() => MockCompiler.Compile(typeof(Bz43694))); // TODO: Verify this is BuildException
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Bz43694(inflator)); // TODO: Verify line/column
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Bz43694 : ContentPage
{
	public Bz43694() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Bz43694));
				var generated = result.GeneratedInitializeComponent();
				Assert.True(result.Diagnostics.Any());
			}
		}
	}
}