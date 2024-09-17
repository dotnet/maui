using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class XamlInflatorSourceGen : ContentPage
{
	public XamlInflatorSourceGen() => InitializeComponent();

	[Test] public void TestRuntimeInflator() => XamlInflatorRuntimeTestsHelpers.TestInflator(typeof(XamlInflatorSourceGen), XamlInflator.SourceGen);

	[Test] public void TestInflation()
	{
		var page = new XamlInflatorSourceGen();
		Assert.That(page.label.Text, Is.EqualTo("Welcome to .NET MAUI!"), "Label text should be 'Welcome to .NET MAUI!'");
	}
}