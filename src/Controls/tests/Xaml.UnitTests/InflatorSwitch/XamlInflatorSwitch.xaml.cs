using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class XamlInflatorSwitch : ContentPage
{
	public XamlInflatorSwitch() => InitializeComponent();

	[Test] public void TestRuntimeInflator() => XamlInflatorRuntimeTestsHelpers.TestInflator(typeof(XamlInflatorSwitch), XamlInflator.Default, true);

	[Test] public void TestInflation()
	{
		var page = new XamlInflatorSwitch();
		Assert.That(page.label.Text, Is.EqualTo("Welcome to .NET MAUI!"), "Label text should be 'Welcome to .NET MAUI!'");
	}
}