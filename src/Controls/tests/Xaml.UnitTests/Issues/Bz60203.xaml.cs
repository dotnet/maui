using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz60203 : ContentPage
{
	public Bz60203() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void CanCompileMultiTriggersWithDifferentConditions([Values] XamlInflator inflator)
		{
			var layout = new Bz60203(inflator);
			Assert.That(layout.label.BackgroundColor, Is.EqualTo(BackgroundColorProperty.DefaultValue));
			layout.BindingContext = new { Text = "Foo" };
			layout.label.TextColor = Colors.Blue;
			Assert.That(layout.label.BackgroundColor, Is.EqualTo(Colors.Pink));
		}
	}
}