using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
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
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Gh2007));
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}
		}
	}
}
