using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz37306 : ContentPage
{
	public Bz37306() => InitializeComponent();


	[TestFixture]
	class Tests
	{
		[Test]
		public void xStringInResourcesDictionaries([Values] XamlInflator inflator)
		{
			var layout = new Bz37306(inflator);
			Assert.AreEqual("Mobile App", layout.Resources["AppName"]);
			Assert.AreEqual("Mobile App", layout.Resources["ApplicationName"]);
		}
	}
}
