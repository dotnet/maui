using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2034 : ContentPage
{
	public Gh2034() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void Compiles(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(Gh2034));
			}
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Gh2034));
				Assert.Empty(result.Diagnostics);
			}
		}
	}
}
