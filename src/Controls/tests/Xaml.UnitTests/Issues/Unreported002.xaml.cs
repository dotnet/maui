using Microsoft.Maui.Graphics;
using NUnit.Framework;

using AbsoluteLayoutCompat = Microsoft.Maui.Controls.Compatibility.AbsoluteLayout;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported002 : ContentPage
{
	public Unreported002() => InitializeComponent();

	class Tests
	{
		[Test]
		public void TypeConvertersOnAttachedBP([Values] XamlInflator inflator)
		{
			var p = new Unreported002(inflator);
			Assert.AreEqual(new Rect(0.5, 0.5, 1, -1), AbsoluteLayoutCompat.GetLayoutBounds(p.label));
			Assert.AreEqual(new Rect(0.7, 0.7, 0.9, -1), Microsoft.Maui.Controls.AbsoluteLayout.GetLayoutBounds(p.label2));
		}
	}
}