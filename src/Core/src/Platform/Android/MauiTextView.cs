using Android.Content;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// This type extends the features of the base AndroidX AppCompatTextView with
	/// additional features requried by .NET MAUI.
	/// </summary>
	public class MauiTextView : AppCompatTextView
	{
		/// <inheritdoc/>
		public MauiTextView(Context context) : base(context)
		{
		}
	}
}
