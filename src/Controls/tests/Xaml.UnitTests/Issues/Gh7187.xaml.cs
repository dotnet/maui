using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh7187 : ContentPage
{
	public Gh7187() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void InvalidMarkupAssignmentThrowsXPE([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh7187)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh7187(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Gh7187));
				//FIXME check diagnostic code
				Assert.That(result.Diagnostics.Length, Is.EqualTo(1));
			}
			else
				Assert.Ignore($"XamlInflator {inflator} not tested");
				
		}
	}
}
