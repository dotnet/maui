using Android.Text;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
    internal static class TextViewExtensions
    {
		public static void SetLineBreakMode(this TextView textView, LineBreakMode lineBreakMode)
		{
			switch (lineBreakMode)
			{
				case LineBreakMode.NoWrap:
					textView.SetMaxLines(1);
					textView.SetSingleLine(true);
					textView.Ellipsize = null;
					break;
				case LineBreakMode.WordWrap:
					textView.Ellipsize = null;
					textView.SetMaxLines(100);
					textView.SetSingleLine(false);
					break;
				case LineBreakMode.CharacterWrap:
					textView.Ellipsize = null;
					textView.SetMaxLines(100);
					textView.SetSingleLine(false);
					break;
				case LineBreakMode.HeadTruncation:
					textView.SetMaxLines(1);
					textView.SetSingleLine(true);
					textView.Ellipsize = TextUtils.TruncateAt.Start;
					break;
				case LineBreakMode.TailTruncation:
					textView.SetMaxLines(1);
					textView.SetSingleLine(true);
					textView.Ellipsize = TextUtils.TruncateAt.End;
					break;
				case LineBreakMode.MiddleTruncation:
					textView.SetMaxLines(1);
					textView.SetSingleLine(true);
					textView.Ellipsize = TextUtils.TruncateAt.Middle;
					break;
			}
		}
    }
}
