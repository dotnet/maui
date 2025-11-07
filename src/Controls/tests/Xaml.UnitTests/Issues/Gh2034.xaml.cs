using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2034 : ContentPage
{
	public Gh2034() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void Compiles(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(Gh2034));
				// TODO: XUnit has no // TODO: XUnit has no Assert.Pass() - test passes if no exception is thrown - test passes if no exception is thrown
			}
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Gh2034));
				Assert.Empty(result.Diagnostics);
			}
		}
	}
}
