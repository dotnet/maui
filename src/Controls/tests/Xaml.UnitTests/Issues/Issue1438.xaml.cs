using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1438 : ContentPage
{
	public Issue1438() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void XNameForwardDeclaration([Values] XamlInflator inflator)
		{
			var page = new Issue1438(inflator);

			var slider = page.FindByName<Slider>("slider");
			var label = page.FindByName<Label>("label");
			Assert.AreSame(slider, label.BindingContext);
			Assert.That(slider.Parent, Is.TypeOf<StackLayout>());
		}
	}
}