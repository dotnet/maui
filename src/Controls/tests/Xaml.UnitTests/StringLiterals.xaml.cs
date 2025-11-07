using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StringLiterals : ContentPage
{
	public StringLiterals()
	{
		InitializeComponent();
	}


	public class Tests
	{
		[Theory]
		[Values]
		public void EscapedStringsAreTreatedAsLiterals(XamlInflator inflator)
		{
			var layout = new StringLiterals(inflator);
			Assert.Equal("Foo", layout.label0.Text);
			Assert.Equal("{Foo}", layout.label1.Text);
			Assert.Equal("Foo", layout.label2.Text);
			Assert.Equal("Foo", layout.label3.Text);
		}
	}
}