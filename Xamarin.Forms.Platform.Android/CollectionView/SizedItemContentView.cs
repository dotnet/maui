using System;
using Android.Content;

namespace Xamarin.Forms.Platform.Android
{
	internal class SizedItemContentView : ItemContentView
	{
		readonly Func<int> _width;
		readonly Func<int> _height;

		public SizedItemContentView(IVisualElementRenderer content, Context context, Func<int> width, Func<int> height) 
			: base(content, context)
		{
			_width = width;
			_height = height;
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			var targetWidth = _width();
			var targetHeight = _height();

			Content.Element.Measure(Context.FromPixels(targetWidth), Context.FromPixels(targetHeight), 
				MeasureFlags.IncludeMargins);
			
			SetMeasuredDimension(targetWidth, targetHeight);
		}
	}
}
