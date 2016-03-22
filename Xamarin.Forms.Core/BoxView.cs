using System;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_BoxViewRenderer))]
	public class BoxView : View
	{
		public static readonly BindableProperty ColorProperty = BindableProperty.Create("Color", typeof(Color), typeof(BoxView), Color.Default);

		public Color Color
		{
			get { return (Color)GetValue(ColorProperty); }
			set { SetValue(ColorProperty, value); }
		}

		[Obsolete("Use OnMeasure")]
		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(40, 40));
		}
	}
}