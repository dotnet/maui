using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2578 : ContentPage
{
	public Issue2578() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Ignore("[Bug] NamedSizes don't work in triggers: https://github.com/xamarin/Microsoft.Maui.Controls/issues/13831")]
		[Test]
		public void MultipleTriggers([Values] XamlInflator inflator)
		{
			Issue2578 layout = new Issue2578(inflator);

			Assert.AreEqual(null, layout.label.Text);
			Assert.AreEqual(null, layout.label.BackgroundColor);
			Assert.AreEqual(Colors.Olive, layout.label.TextColor);
			layout.label.Text = "Foo";
			Assert.AreEqual(Colors.Red, layout.label.BackgroundColor);
		}
	}
}