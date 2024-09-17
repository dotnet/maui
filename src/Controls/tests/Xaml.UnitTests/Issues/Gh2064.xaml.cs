using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh2064 : ContentPage
{
	public Gh2064() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ReportMissingTargetTypeOnStyle([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh2064)));
			if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh2064(inflator));
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Gh2064));
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}
		}
	}
}
