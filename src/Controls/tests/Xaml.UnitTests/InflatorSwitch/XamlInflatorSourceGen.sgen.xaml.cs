using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XamlInflatorSourceGen : ContentPage
{
	public XamlInflatorSourceGen() => InitializeComponent();

	[Test] public void TestSourceGenInflator() => XamlInflatorTestsHelpers.TestInflator(typeof(XamlInflatorSourceGen), XamlInflator.SourceGen, true);

	[Test]
	public void TestInflation()
	{
		var page = new XamlInflatorSourceGen();
		Assert.That(page.label.Text, Is.EqualTo("Welcome to .NET MAUI!"), "Label text should be 'Welcome to .NET MAUI!'");
	}
}