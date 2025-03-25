using System;
using Android.Content;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Platform
{
	public class MauiAppCompatEditText : AppCompatEditText
	{
		public event EventHandler? SelectionChanged;

		public MauiAppCompatEditText(Context context) : base(context)
		{
		}

		protected override void OnSelectionChanged(int selStart, int selEnd)
		{
			base.OnSelectionChanged(selStart, selEnd);

			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
