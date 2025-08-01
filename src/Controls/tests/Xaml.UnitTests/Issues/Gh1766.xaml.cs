using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh1766 : ContentPage
{
	public Gh1766() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void CSSPropertiesNotInerited([Values] XamlInflator inflator)
		{
			var layout = new Gh1766(inflator);
			Assert.That(layout.stack.BackgroundColor, Is.EqualTo(Colors.Pink));
			Assert.That(layout.entry.BackgroundColor, Is.EqualTo(VisualElement.BackgroundColorProperty.DefaultValue));
		}
	}
}
