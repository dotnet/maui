using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3082 : ContentPage
{
	public Gh3082() => InitializeComponent();

	static async Task OnClicked(object sender, EventArgs e) => await Task.Delay(1000);

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ThrowsOnWrongEventHandlerSignature(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh3082)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh3082(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh3082 : ContentPage
{
	public Gh3082() => InitializeComponent();
			}
""")
					.RunMauiSourceGenerator(typeof(Gh3082));
				Assert.Single(result.Diagnostics);
			}

		}
	}
}
