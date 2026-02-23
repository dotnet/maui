using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StaticExtensionException : ContentPage
{
	public StaticExtensionException() => InitializeComponent();

	[Collection("Issue")]
	public class Issue2115 : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void xStaticThrowsMeaningfullException(XamlInflator inflator)
		{
			if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new StaticExtensionException(inflator));
			else if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(StaticExtensionException)));
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
				Assert.True(result.Diagnostics.Any());
			}
		}
	}
}