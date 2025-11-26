using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh1978 : ContentPage
{
	public Gh1978() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ReportError([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh1978)));
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh1978 : ContentPage
{
	public Gh1978() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Gh1978));
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}

		}
	}
}
