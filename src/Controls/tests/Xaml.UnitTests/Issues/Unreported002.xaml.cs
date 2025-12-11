using Microsoft.Maui.Graphics;
using Xunit;

using AbsoluteLayoutCompat = Microsoft.Maui.Controls.Compatibility.AbsoluteLayout;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported002 : ContentPage
{
	public Unreported002() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void TypeConvertersOnAttachedBP(XamlInflator inflator)
		{
			var p = new Unreported002(inflator);
			Assert.Equal(new Rect(0.5, 0.5, 1, -1), AbsoluteLayoutCompat.GetLayoutBounds(p.label));
			Assert.Equal(new Rect(0.7, 0.7, 0.9, -1), Microsoft.Maui.Controls.AbsoluteLayout.GetLayoutBounds(p.label2));
		}
	}
}