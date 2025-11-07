using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh1766 : ContentPage
{
	public Gh1766() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void CSSPropertiesNotInerited(XamlInflator inflator)
		{
			var layout = new Gh1766(inflator);
			Assert.Equal(Colors.Pink, layout.stack.BackgroundColor);
			Assert.Equal(VisualElement.BackgroundColorProperty.DefaultValue, layout.entry.BackgroundColor);
		}
	}
}
