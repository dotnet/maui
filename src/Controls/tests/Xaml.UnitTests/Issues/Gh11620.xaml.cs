using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh11620 : ContentPage
{
	public Gh11620() => InitializeComponent();


	[TestFixture]
	class Tests
	{
		[Test]
		public void AddValueType([Values] XamlInflator inflator)
		{
			var layout = new Gh11620(inflator);
			var arr = layout.Resources["myArray"];
			Assert.That(arr, Is.TypeOf<object[]>());
			Assert.That(((object[])arr).Length, Is.EqualTo(3));
			Assert.That(((object[])arr)[2], Is.EqualTo(32));
		}
	}
}