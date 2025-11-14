using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported009 : ContentPage
{
	public Unreported009() => InitializeComponent();

	class Tests
	{
		[Test]
		public void AllowSetterValueAsElementProperties([Values] XamlInflator inflator)
		{
			var p = new Unreported009(inflator);
			var s = p.Resources["Default"] as Style;
			Assert.AreEqual("Bananas!", (s.Setters[0].Value as Label).Text);
		}
	}
}
