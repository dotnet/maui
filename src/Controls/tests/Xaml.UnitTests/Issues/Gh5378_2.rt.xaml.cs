using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh5378_2 : ContentPage
{
	public Gh5378_2() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ReportSyntaxError([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh5378_2)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh5378_2(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh5378_2 : ContentPage
{
	public Gh5378_2() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Gh5378_2));
				//FIXME check diagnostic code
				Assert.That(result.Diagnostics.Length, Is.EqualTo(1));
			}
			else
				Assert.Ignore($"Test not supported for {inflator}");
		}
	}
}