using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	[XamlProcessing(XamlInflator.Runtime, true)]
	public partial class Gh5378_1 : ContentPage
	{
		public Gh5378_1() => InitializeComponent();

		[TestFixture]
		class Tests
		{
			[Test]
			public void ReportSyntaxError([Values] XamlInflator inflator)
			{
				if (inflator == XamlInflator.XamlC)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh5378_1)));
				else if (inflator == XamlInflator.Runtime)
					Assert.Throws<XamlParseException>(() => new Gh5378_1(inflator));
				else if (inflator == XamlInflator.SourceGen)
				{
					var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Gh5378_1));
					//FIXME check diagnostic code
					Assert.That(result.Diagnostics.Length, Is.EqualTo(1));
				}					
				else
					Assert.Ignore($"Test not supported for {inflator}");
			}
		}
	}
}