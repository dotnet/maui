using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TestXmlnsUsing : ContentPage
{
	public TestXmlnsUsing() => InitializeComponent();

	class Tests
	{
		[TearDown] public void TearDown() => Application.Current = null;

		[Test]
		public void SupportUsingXmlns([Values] XamlInflator inflator)
		{
			var page = new TestXmlnsUsing(inflator);
			Assert.That(page.Content, Is.Not.Null);
			Assert.That(page.CustomView, Is.TypeOf<CustomXamlView>());
			Assert.That(page.Radio1.Value, Is.EqualTo(1));
			Assert.That(page.Radio2.Value, Is.EqualTo(2));
		}
	}
}
