using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XamlInflatorXamlC : ContentPage
{
	public XamlInflatorXamlC() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Fact]
		public void TestXamlCInflator() => XamlInflatorTestsHelpers.TestInflator(typeof(XamlInflatorXamlC), XamlInflator.XamlC, true);

		[Fact]
		public void TestInflation()
		{
			var page = new XamlInflatorXamlC();
			Assert.Equal("Welcome to .NET MAUI!", page.label.Text);
		}
	}
}