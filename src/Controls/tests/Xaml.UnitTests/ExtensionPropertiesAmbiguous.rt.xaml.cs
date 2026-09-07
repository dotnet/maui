using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// AmbiguousExtensions declares MyAmbiguous for both IView and BindableObject. Both apply to a Label and
// neither receiver is more derived than the other, so no inflator is allowed to pick one.
public partial class ExtensionPropertiesAmbiguous : ContentPage
{
	public ExtensionPropertiesAmbiguous() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void Throw(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				var e = Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(ExtensionPropertiesAmbiguous)));
				Assert.Equal(BuildExceptionCode.ExtensionPropertyResolution, e.Code);
			}
			else if (inflator == XamlInflator.Runtime)
			{
				var e = Assert.Throws<XamlParseException>(() => new ExtensionPropertiesAmbiguous(inflator));
				Assert.Contains("MyAmbiguous", e.Message, System.StringComparison.Ordinal);
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				// the mock compilation does not reference the test assembly, and its Roslyn cannot parse
				// C# extension blocks, so the container is declared here in its lowered form
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class AmbiguousExtensions
{
	public static string get_MyAmbiguous(global::Microsoft.Maui.IView view) => string.Empty;
	public static void set_MyAmbiguous(global::Microsoft.Maui.IView view, string value) { }
	public static string get_MyAmbiguous(BindableObject bindable) => string.Empty;
	public static void set_MyAmbiguous(BindableObject bindable, string value) { }
}

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class ExtensionPropertiesAmbiguous : ContentPage
{
	public ExtensionPropertiesAmbiguous() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(ExtensionPropertiesAmbiguous));
				Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2015"));
			}
		}
	}
}
