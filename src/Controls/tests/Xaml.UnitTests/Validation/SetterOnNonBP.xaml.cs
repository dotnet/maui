using Microsoft.Maui.Controls.Xaml;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class FakeView : View
{
	public string NonBindable { get; set; }
}

[XamlProcessing(XamlInflator.Runtime, true)]
[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class SetterOnNonBP : ContentPage
{
	public SetterOnNonBP() => InitializeComponent();

	public class SetterOnNonBPTests
	{
		[Test]
		public void ShouldThrow([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(10, 13), () => MockCompiler.Compile(typeof(SetterOnNonBP)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(10, 13), () => new SetterOnNonBP(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(SetterOnNonBP));
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}
			else
				Assert.Ignore("Unknown inflator");
		}
	}
}