using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class InlineCSS : ContentPage
{
	public InlineCSS() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void InlineCSSParsed([Values] XamlInflator inflator)
		{
			var layout = new InlineCSS(inflator);
			Assert.That(layout.label.TextColor, Is.EqualTo(Colors.Pink));
		}

		[Test]
		public void InitialValue([Values] XamlInflator inflator)
		{
			var layout = new InlineCSS(inflator);
			Assert.That(layout.BackgroundColor, Is.EqualTo(Colors.Green));
			Assert.That(layout.stack.BackgroundColor, Is.EqualTo(Colors.Green));
			Assert.That(layout.button.BackgroundColor, Is.EqualTo(Colors.Green));
			Assert.That(layout.label.BackgroundColor, Is.EqualTo(VisualElement.BackgroundColorProperty.DefaultValue));
			Assert.That(layout.label.TextTransform, Is.EqualTo(TextTransform.Uppercase));
		}
	}
}
