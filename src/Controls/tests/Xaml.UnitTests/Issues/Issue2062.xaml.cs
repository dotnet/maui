using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2062 : ContentPage
{
	public Issue2062() => InitializeComponent();


	[TestFixture]
	class Tests
	{
		[Test]
		public void LabelWithoutExplicitPropertyElement([Values] XamlInflator inflator)
		{
			var layout = new Issue2062(inflator);
			Assert.AreEqual("text explicitly set to Label.Text", layout.label1.Text);
			Assert.AreEqual("text implicitly set to Text property of Label", layout.label2.Text);
		}
	}
}