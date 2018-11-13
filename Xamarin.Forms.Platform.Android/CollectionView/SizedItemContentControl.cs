using System;
using Android.Content;

namespace Xamarin.Forms.Platform.Android
{
	internal class SizedItemContentControl : ItemContentControl
	{
		readonly Func<int> _width;
		readonly Func<int> _height;

		public SizedItemContentControl(IVisualElementRenderer content, Context context, Func<int> width, Func<int> height) 
			: base(content, context)
		{
			_width = width;
			_height = height;
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			var pixelWidth = _width();
			var pixelHeight =_height();

			Content.Element.Measure(pixelWidth, pixelHeight, MeasureFlags.IncludeMargins);
			SetMeasuredDimension(pixelWidth, pixelHeight);
		}
	}
}