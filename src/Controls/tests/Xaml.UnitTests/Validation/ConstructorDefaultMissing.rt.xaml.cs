using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class TypeWithoutDefaultConstructor : View
{
	public TypeWithoutDefaultConstructor(string requiredParam)
	{
	}
}

public partial class ConstructorDefaultMissing : ContentPage
{
	public ConstructorDefaultMissing() => InitializeComponent();

	[TestFixture]
class Tests
	{
		[Test]
		public void ThrowsOnMissingDefaultConstructor([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(6, 8), () => MockCompiler.Compile(typeof(ConstructorDefaultMissing)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(6, 8), () => new ConstructorDefaultMissing(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class TypeWithoutDefaultConstructor : View
{
	public TypeWithoutDefaultConstructor(string requiredParam)
	{
	}
}

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class ConstructorDefaultMissing : ContentPage
{
	public ConstructorDefaultMissing() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(ConstructorDefaultMissing));
				Assert.That(result.Diagnostics, Is.Not.Empty);
				Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2009"), Is.True);
			}
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
