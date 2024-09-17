using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Bz43450 : ContentPage
{
	public Bz43450() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void DoesNotAllowGridRowDefinition([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Bz43450)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Bz43450(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.CreateMauiCompilation().RunMauiSourceGenerator(typeof(Bz43450));
				Assert.That(result.Diagnostics.Any() );
			} else
				Assert.Ignore("Unknown inflator");
		}
	}
}