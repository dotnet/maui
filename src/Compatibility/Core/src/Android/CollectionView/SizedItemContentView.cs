//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Android.Content;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[System.Obsolete]
	internal class SizedItemContentView : ItemContentView
	{
		readonly Func<int> _width;
		readonly Func<int> _height;

		public SizedItemContentView(Context context, Func<int> width, Func<int> height)
			: base(context)
		{
			_width = width;
			_height = height;
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (Content == null)
			{
				SetMeasuredDimension(0, 0);
				return;
			}

			var targetWidth = _width();
			var targetHeight = _height();

			Content.Element.Measure(Context.FromPixels(targetWidth), Context.FromPixels(targetHeight),
				MeasureFlags.IncludeMargins);

			SetMeasuredDimension(targetWidth, targetHeight);
		}
	}
}
