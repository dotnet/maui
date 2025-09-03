using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

using AbsoluteLayout = Compatibility.AbsoluteLayout;

public partial class Gh11551 : ContentPage
{
	public Gh11551() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void RectBoundsDoesntThrow([Values] XamlInflator inflator)
		{
			var layout = new Gh11551(inflator);
			var bounds = AbsoluteLayout.GetLayoutBounds(layout.label);
			Assert.That(bounds, Is.EqualTo(new Rect(1, .5, -1, 22)));
		}
	}
}
