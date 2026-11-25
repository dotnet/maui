using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class OnPlatformOptimization : ContentPage
{
	public OnPlatformOptimization() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[InlineData("net7.0-ios", XamlInflator.SourceGen)]
		[InlineData("net7.0-android", XamlInflator.SourceGen)]
		internal void OnPlatformExtensionsAreSimplified(string targetFramework, XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class OnPlatformOptimization : ContentPage
{
	public OnPlatformOptimization() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(OnPlatformOptimization), targetFramework: targetFramework);
				Assert.Empty(result.Diagnostics);

				var generated = result.GeneratedInitializeComponent();
				Assert.DoesNotContain("OnPlatformExtension", generated, System.StringComparison.Ordinal);
				var expected = targetFramework.EndsWith("-ios") ? "bar" : "foo";
				Assert.Contains($"SetValue(global::Microsoft.Maui.Controls.Label.TextProperty, \"{expected}\");", generated, System.StringComparison.Ordinal);
			}
		}

		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void ValuesAreSet(XamlInflator inflator)
		{
			var p = new OnPlatformOptimization(inflator);
			Assert.Equal("ringo", p.label0.Text);
			Assert.Equal("foo", p.label1.Text);
		}
	}
}