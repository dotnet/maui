using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34726 : ContentPage
{
	public Maui34726() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void XKeyWithSpecialCharsProducesValidCode(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Maui34726 : ContentPage
{
	public Maui34726() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Maui34726));
				Assert.Empty(result.Diagnostics);
			}
			else
			{
				var page = new Maui34726(inflator);
				Assert.NotNull(page);
				Assert.Equal(3, page.Resources.Count);
				Assert.True(page.Resources.ContainsKey("Key\"Quote"));
				Assert.True(page.Resources.ContainsKey("Key\\Backslash"));
				Assert.True(page.Resources.ContainsKey("SimpleKey"));
			}
		}
	}
}
