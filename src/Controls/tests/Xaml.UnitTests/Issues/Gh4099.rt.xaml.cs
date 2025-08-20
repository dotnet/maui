using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh4099 : ContentPage
{
	public Gh4099() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void BetterExceptionReport([Values] XamlInflator inflator)
		{
			switch (inflator)
			{
				case XamlInflator.XamlC:
					try
					{
						MockCompiler.Compile(typeof(Gh4099));
					}
					catch (BuildException xpe)
					{
						Assert.That(xpe.XmlInfo.LineNumber, Is.EqualTo(5));
						Assert.Pass();
					}
					Assert.Fail();
					break;
				case XamlInflator.SourceGen:
					var result = CreateMauiCompilation()
						.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh4099 : ContentPage
{
	public Gh4099() => InitializeComponent();
}
""")
						.RunMauiSourceGenerator(typeof(Gh4099));
					Assert.That(result.Diagnostics.Any());
					return;
				default:
					Assert.Ignore();
					break;
			}
		}
	}
}

