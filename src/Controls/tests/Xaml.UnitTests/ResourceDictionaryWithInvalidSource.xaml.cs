using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class ResourceDictionaryWithInvalidSource : ContentPage
{
	public ResourceDictionaryWithInvalidSource() => InitializeComponent();

	public class Tests
	{
		[Test]
		public void InvalidSourceThrows([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(8, 33), () => MockCompiler.Compile(typeof(ResourceDictionaryWithInvalidSource)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(8, 33), () => new ResourceDictionaryWithInvalidSource(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(ResourceDictionaryWithInvalidSource));
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}
			else
			{
				Assert.Ignore("This test is not yet implemented");
			}
		}
	}
}