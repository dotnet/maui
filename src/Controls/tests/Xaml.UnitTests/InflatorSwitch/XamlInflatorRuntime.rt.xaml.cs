using NUnit.Framework;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XamlInflatorRuntime : ContentPage
{
	public XamlInflatorRuntime() => InitializeComponent();
	[Test] public void TestRuntimeInflator() => XamlInflatorTestsHelpers.TestInflator(typeof(XamlInflatorRuntime), XamlInflator.Runtime, true);

	[Test]
	public void TestInflation()
	{
		var page = new XamlInflatorRuntime();
		Assert.That(page.label.Text, Is.EqualTo("Welcome to .NET MAUI!"), "Label text should be 'Welcome to .NET MAUI!'");
	}
}