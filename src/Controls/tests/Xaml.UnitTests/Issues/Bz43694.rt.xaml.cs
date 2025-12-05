using System;
using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz43694 : ContentPage
{
	public Bz43694() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void xStaticWithOnPlatformChildInRD(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				var ex = Assert.ThrowsAny<Exception>(() => MockCompiler.Compile(typeof(Bz43694)));
				Assert.True(ex is Microsoft.Maui.Controls.Build.Tasks.BuildException);
			}
			else if (inflator == XamlInflator.Runtime)
			{
				var ex = Assert.ThrowsAny<Exception>(() => new Bz43694(inflator));
				Assert.True(ex is XamlParseException);
			}
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