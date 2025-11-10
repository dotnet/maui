using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh4099 : ContentPage
{
	public Gh4099() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void BetterExceptionReport(XamlInflator inflator)
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
						Assert.Equal(5, xpe.XmlInfo.LineNumber);
						return;
					}
					Assert.Fail("Expected BuildException was not thrown");
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
					Assert.True(result.Diagnostics.Any());
					return;
				default:
					return;
			}
		}
	}
}

