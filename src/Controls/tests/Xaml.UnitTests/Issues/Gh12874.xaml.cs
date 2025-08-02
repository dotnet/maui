using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh12874 : ContentPage
{
	public Gh12874() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void RevertToStyleValue([Values] XamlInflator inflator)
		{
			var layout = new Gh12874(inflator);
			Assert.That(layout.label0.HorizontalOptions, Is.EqualTo(LayoutOptions.Start));
			Assert.That(layout.label1.HorizontalOptions, Is.EqualTo(LayoutOptions.Start));
			layout.label0.ClearValue(Label.HorizontalOptionsProperty);
			layout.label1.ClearValue(Label.HorizontalOptionsProperty);
			Assert.That(layout.label0.HorizontalOptions, Is.EqualTo(LayoutOptions.Center));
			Assert.That(layout.label1.HorizontalOptions, Is.EqualTo(LayoutOptions.Center));
		}
	}
}
