using Xunit;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XamlInflatorRuntime : ContentPage
{
	public XamlInflatorRuntime() => InitializeComponent();
	[Fact] public void TestRuntimeInflator() => XamlInflatorTestsHelpers.TestInflator(typeof(XamlInflatorRuntime), XamlInflator.Runtime, true);

	[Fact]
	public void TestInflation()
	{
		var page = new XamlInflatorRuntime();
		Assert.Equal("Welcome to .NET MAUI!", page.label.Text);
	}
}