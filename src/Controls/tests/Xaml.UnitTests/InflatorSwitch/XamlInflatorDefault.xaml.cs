using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default)]
public partial class XamlInflatorDefault : ContentPage
{
	public XamlInflatorDefault()
	{
		InitializeComponent();
	}
	[Test] public void TestRuntimeInflator() => XamlInflatorRuntimeTestsHelpers.TestInflator(typeof(XamlInflatorDefault), XamlInflator.Runtime);

	[Test] public void TestInflation()
	{
		var page = new XamlInflatorDefault();
		Assert.That(page.label.Text, Is.EqualTo("Welcome to .NET MAUI!"), "Label text should be 'Welcome to .NET MAUI!'");
	}
}