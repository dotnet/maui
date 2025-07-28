using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.XamlC)]
public partial class XamlInflatorXamlC : ContentPage
{
	public XamlInflatorXamlC() => InitializeComponent();

	[Test] public void TestRuntimeInflator() => XamlInflatorRuntimeTestsHelpers.TestInflator(typeof(XamlInflatorXamlC), XamlInflator.XamlC);

	[Test]
	public void TestInflation()
	{
		var page = new XamlInflatorXamlC();
		Assert.That(page.label.Text, Is.EqualTo("Welcome to .NET MAUI!"), "Label text should be 'Welcome to .NET MAUI!'");
	}
}