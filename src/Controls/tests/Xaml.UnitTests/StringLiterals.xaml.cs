using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StringLiterals : ContentPage
{
	public StringLiterals()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void EscapedStringsAreTreatedAsLiterals([Values] XamlInflator inflator)
		{
			var layout = new StringLiterals(inflator);
			Assert.AreEqual("Foo", layout.label0.Text);
			Assert.AreEqual("{Foo}", layout.label1.Text);
			Assert.AreEqual("Foo", layout.label2.Text);
			Assert.AreEqual("Foo", layout.label3.Text);
		}
	}
}