using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XamlInflatorSwitch : ContentPage
{
	public XamlInflatorSwitch() => InitializeComponent();


	public class Tests
	{
		[Fact]
		public void TestRuntimeInflator() => XamlInflatorTestsHelpers.TestInflator(typeof(XamlInflatorSwitch), XamlInflator.Runtime, true);

		[Fact]
		public void TestInflation()
		{
			var page = new XamlInflatorSwitch();
			Assert.Equal("Welcome to .NET MAUI!", page.label.Text);
		}
	}
}