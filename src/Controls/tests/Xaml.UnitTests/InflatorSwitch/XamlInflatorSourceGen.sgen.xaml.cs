using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XamlInflatorSourceGen : ContentPage
{
	public XamlInflatorSourceGen() => InitializeComponent();

	[Fact] public void TestSourceGenInflator() => XamlInflatorTestsHelpers.TestInflator(typeof(XamlInflatorSourceGen), XamlInflator.SourceGen, true);

	[Fact]
	public void TestInflation()
	{
		var page = new XamlInflatorSourceGen();
		Assert.Equal("Welcome to .NET MAUI!", page.label.Text);
	}
}