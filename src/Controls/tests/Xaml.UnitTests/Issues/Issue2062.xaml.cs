using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2062 : ContentPage
{
	public Issue2062() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void LabelWithoutExplicitPropertyElement(XamlInflator inflator)
		{
			var layout = new Issue2062(inflator);
			Assert.Equal("text explicitly set to Label.Text", layout.label1.Text);
			Assert.Equal("text implicitly set to Text property of Label", layout.label2.Text);
		}
	}
}