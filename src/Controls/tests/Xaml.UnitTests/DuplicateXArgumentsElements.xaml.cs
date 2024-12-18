using System.Linq;
using NuGet.Frameworks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
[XamlProcessing(XamlInflator.Runtime, true)]
public partial class DuplicateXArgumentsElements : BindableObject
{
	public DuplicateXArgumentsElements() => InitializeComponent();

	[TestFixture]
	public static class Tests
	{
		[Test]
		public static void ThrowXamlParseException([Values]XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(DuplicateXArgumentsElements), out var md, out var hasLoggedErrors);
				Assert.That(hasLoggedErrors);
			}
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new DuplicateXArgumentsElements(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(DuplicateXArgumentsElements));
				Assert.That(result.Diagnostics.Any());
			}
		}
	}
}
