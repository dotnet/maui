using System.Linq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
[XamlProcessing(XamlInflator.Runtime, true)]
public partial class DuplicatePropertyElements : BindableObject
{
	public DuplicatePropertyElements(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	public static class Tests
	{
		[Test] public static void ThrowXamlParseException([Values]XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC){
				MockCompiler.Compile(typeof(DuplicatePropertyElements), out var md, out var hasLoggedErrors);
				Assert.That(hasLoggedErrors);
			}
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new DuplicatePropertyElements(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(DuplicatePropertyElements));
				Assert.That(result.Diagnostics.Any());
			}				
		}
	}
}
