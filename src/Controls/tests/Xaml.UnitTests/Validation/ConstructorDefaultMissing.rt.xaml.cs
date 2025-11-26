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
				Assert.Throws(new BuildExceptionConstraint(7, 3), () => MockCompiler.Compile(typeof(ConstructorDefaultMissing)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(7, 3), () => new ConstructorDefaultMissing(inflator));
			else if (inflator == XamlInflator.SourceGen)
				Assert.Ignore("SourceGen does not yet report missing default constructor errors");
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
