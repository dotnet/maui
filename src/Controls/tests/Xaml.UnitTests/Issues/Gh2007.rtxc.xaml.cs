using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2007 : ContentPage
{
	public Gh2007() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void UsefullxResourceErrorMessages([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.Runtime || inflator == XamlInflator.XamlC)
				Assert.Throws<XamlParseException>(() => new Gh2007(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime | XamlInflator.XamlC, true)]
public partial class Gh2007 : ContentPage
{
	public Gh2007() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Gh2007));
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}
			else
				Assert.Ignore("ignoring test for {inflator} as it is not supported in this context");
		}
	}
}
