using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XmlnsUndeclared : ContentPage
{
	public XmlnsUndeclared() { }
	internal XmlnsUndeclared(XamlInflator inflator) : this()
	{
		if (inflator == XamlInflator.Runtime)
			this.LoadFromXaml(typeof(XmlnsUndeclared));
	}

	[TestFixture]
class Tests
	{
		[Test]
		public void ThrowsOnUndeclaredXmlns([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(5, 8), () => MockCompiler.Compile(typeof(XmlnsUndeclared)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(5, 8), () => new XmlnsUndeclared(inflator));
			else if (inflator == XamlInflator.SourceGen)
				Assert.Ignore("SourceGen cannot process files with undeclared xmlns prefixes - XAML parsing fails before code generation");
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
