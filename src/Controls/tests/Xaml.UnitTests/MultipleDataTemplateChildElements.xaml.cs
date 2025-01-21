using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
[XamlProcessing(XamlInflator.Runtime, true)]
public partial class MultipleDataTemplateChildElements : BindableObject
{
	public static class Tests
	{
		[Test]
		public static void ThrowXamlParseException([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(MultipleDataTemplateChildElements), out var md, out var hasLoggedErrors);
				Assert.That(hasLoggedErrors);
			}					
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new MultipleDataTemplateChildElements(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(MultipleDataTemplateChildElements));
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}
			else
				Assert.Ignore("This test is not yet implemented");
		}
	}
}
