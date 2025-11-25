using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XDataTypeSyntax : ContentPage
{
	public XDataTypeSyntax() => InitializeComponent();

	[TestFixture]
class Tests
	{
		[Test]
		public void ThrowsOnInvalidXDataTypeSyntax([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(5, 2), () => MockCompiler.Compile(typeof(XDataTypeSyntax)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(5, 2), () => new XDataTypeSyntax(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class XDataTypeSyntax : ContentPage
{
	public XDataTypeSyntax() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(XDataTypeSyntax));
				Assert.That(result.Diagnostics, Is.Not.Empty);
				Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2102"), Is.True);
			}
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
