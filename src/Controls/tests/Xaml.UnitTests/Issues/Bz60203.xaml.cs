using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz60203 : ContentPage
{
	public Bz60203() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void CanCompileMultiTriggersWithDifferentConditions(XamlInflator inflator)
		{
			var layout = new Bz60203(inflator);
			Assert.Equal(BackgroundColorProperty.DefaultValue, layout.label.BackgroundColor);
			layout.BindingContext = new { Text = "Foo" };
			layout.label.TextColor = Colors.Blue;
			Assert.Equal(Colors.Pink, layout.label.BackgroundColor);
		}
	}
}