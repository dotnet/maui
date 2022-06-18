using Android.Graphics;
using Android.Text;

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformStringSizeService : IStringSizeService
	{
		public SizeF GetStringSize(string value, IFont font, float fontSize)
		{
			if (value == null) return new SizeF();

			var textPaint = new TextPaint { TextSize = fontSize };
			textPaint.SetTypeface(font?.ToTypeface() ?? Typeface.Default);

			var staticLayout = TextLayoutUtils.CreateLayout(value, textPaint, null, Layout.Alignment.AlignNormal);
			var size = staticLayout.GetTextSizeAsSizeF(false);
			staticLayout.Dispose();
			return size;
		}

		public SizeF GetStringSize(string aString, IFont font, float aFontSize, HorizontalAlignment aHorizontalAlignment, VerticalAlignment aVerticalAlignment)
		{
			if (aString == null) return new SizeF();

			var vTextPaint = new TextPaint { TextSize = aFontSize };
			vTextPaint.SetTypeface(font?.ToTypeface() ?? Typeface.Default);

			Layout.Alignment vAlignment;
			switch (aHorizontalAlignment)
			{
				case HorizontalAlignment.Center:
					vAlignment = Layout.Alignment.AlignCenter;
					break;
				case HorizontalAlignment.Right:
					vAlignment = Layout.Alignment.AlignOpposite;
					break;
				default:
					vAlignment = Layout.Alignment.AlignNormal;
					break;
			}

			StaticLayout vLayout = TextLayoutUtils.CreateLayout(aString, vTextPaint, null, vAlignment);
			SizeF vSize = vLayout.GetTextSizeAsSizeF(false);
			vLayout.Dispose();
			return vSize;
		}
	}
}
