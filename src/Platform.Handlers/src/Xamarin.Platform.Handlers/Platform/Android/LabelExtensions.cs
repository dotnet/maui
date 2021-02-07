using Android.Widget;

namespace Xamarin.Platform
{
	public static class LabelExtensions
	{
		public static void UpdateText(this TextView textView, ILabel label) =>
			textView.Text = label.Text;
	}
}
