using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh4099 : ContentPage
{
	public Gh4099() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[InlineData(XamlInflator.SourceGen)]
		internal void BetterExceptionReport(XamlInflator inflator)
		{
			switch (inflator)
			{
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
			}
		}
	}
}

