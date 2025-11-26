using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class MissingEventHandler : ContentPage
{
	public MissingEventHandler() => InitializeComponent();

	[TestFixture]
class Tests
	{
		[Test]
		public void ThrowsOnMissingEventHandler([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(6, 10), () => MockCompiler.Compile(typeof(MissingEventHandler)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(6, 10), () => new MissingEventHandler(inflator));
			else if (inflator == XamlInflator.SourceGen)
				Assert.Ignore("SourceGen does not yet report missing event handler errors");
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
