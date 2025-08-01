using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class LabelHtml : ContentPage
{
	public LabelHtml() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void HtmlInCDATA([Values] XamlInflator inflator)
		{
			var html = "<h1>Hello World!</h1><br/>SecondLine";
			var layout = new LabelHtml(inflator);
			Assert.That(layout.label0.Text, Is.EqualTo(html));
			Assert.That(layout.label1.Text, Is.EqualTo(html));
			Assert.That(layout.label2.Text, Is.EqualTo(html));
			Assert.That(layout.label3.Text, Is.EqualTo(html));
			Assert.That(layout.label4.Text, Is.EqualTo(html));
		}
	}
}