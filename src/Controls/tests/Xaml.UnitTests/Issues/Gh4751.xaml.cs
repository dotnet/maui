using Microsoft.Maui.Controls.Build.Tasks;
using NuGet.Frameworks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4751VM
{
	public string Title { get; }
	public Gh4751VM(string title = null) => Title = title; //a .ctor with a default value IS NOT a default .ctor
}

[XamlCompilation(XamlCompilationOptions.Skip)]
[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh4751 : ContentPage
{
	public Gh4751() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ErrorOnMissingDefaultCtor([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh4751)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh4751(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Gh4751));
				//FIXME check the diagnostic code
				Assert.That(result.Diagnostics.Length, Is.EqualTo(1));
			}
			else if (inflator == XamlInflator.Default)
				Assert.Ignore("Default inflator not set");
		}
	}
}
