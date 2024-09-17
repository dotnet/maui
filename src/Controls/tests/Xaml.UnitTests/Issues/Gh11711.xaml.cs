using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
[XamlProcessing(XamlInflator.Runtime|XamlInflator.Runtime, true)]
public partial class Gh11711 : ContentPage
{
	public Gh11711() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void FormatExceptionAreCaught([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh11711)));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Gh11711));
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh11711(inflator));
		}
	}
}
