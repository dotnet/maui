using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class NamescopeDuplicate : ContentPage
{
	public NamescopeDuplicate() { }
	internal NamescopeDuplicate(XamlInflator inflator) : this()
	{
		if (inflator == XamlInflator.Runtime)
			this.LoadFromXaml(typeof(NamescopeDuplicate));
	}

	[TestFixture]
class Tests
	{
		[Test]
		public void ThrowsOnDuplicateXName([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(7, 16), () => MockCompiler.Compile(typeof(NamescopeDuplicate)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(7, 16), () => new NamescopeDuplicate(inflator));
			else if (inflator == XamlInflator.SourceGen)
				Assert.Ignore("SourceGen generates duplicate field declarations which cause C# compilation errors - error is correctly detected but prevents build");
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
