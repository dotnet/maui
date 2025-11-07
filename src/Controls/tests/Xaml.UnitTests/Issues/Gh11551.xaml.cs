using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

using AbsoluteLayout = Compatibility.AbsoluteLayout;

public partial class Gh11551 : ContentPage
{
	public Gh11551() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void RectBoundsDoesntThrow(XamlInflator inflator)
		{
			var layout = new Gh11551(inflator);
			var bounds = AbsoluteLayout.GetLayoutBounds(layout.label);
			Assert.Equal(new Rect(1, .5, -1, 22), bounds);
		}
	}
}
