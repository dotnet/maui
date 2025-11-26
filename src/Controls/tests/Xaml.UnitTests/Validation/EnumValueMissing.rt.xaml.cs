using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class EnumValueMissing : ContentPage
{
	public EnumValueMissing() => InitializeComponent();

	[TestFixture]
class Tests
	{
		[Test]
		public void ThrowsOnInvalidEnumValue([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(6, 10), () => MockCompiler.Compile(typeof(EnumValueMissing)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(6, 10), () => new EnumValueMissing(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class EnumValueMissing : ContentPage
{
	public EnumValueMissing() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(EnumValueMissing));
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
