using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz60788 : ContentPage
{
	public Bz60788() => InitializeComponent();

	[TestFixture]
	class Tests
	{

		[Test]
		public void KeyedRDWithImplicitStyles([Values] XamlInflator inflator)
		{
			var layout = new Bz60788(inflator);
			Assert.That(layout.Resources.Count, Is.EqualTo(2));
			Assert.That(((ResourceDictionary)layout.Resources["RedTextBlueBackground"]).Count, Is.EqualTo(3));
			Assert.That(((ResourceDictionary)layout.Resources["BlueTextRedBackground"]).Count, Is.EqualTo(3));
		}
	}
}