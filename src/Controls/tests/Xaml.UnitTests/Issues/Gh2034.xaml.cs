using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2034 : ContentPage
{
	public Gh2034() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void Compiles([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(Gh2034));
				Assert.Pass();
			}
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Gh2034));
				Assert.That(result.Diagnostics, Is.Empty);
			}
		}
	}
}
