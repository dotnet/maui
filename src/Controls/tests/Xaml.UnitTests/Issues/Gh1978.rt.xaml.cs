using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh1978 : ContentPage
{
	public Gh1978() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[InlineData(XamlInflator.SourceGen)]
		internal void ReportError(XamlInflator inflator)
		{
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
				Assert.NotEmpty(result.Diagnostics);
			}

		}
	}
}
