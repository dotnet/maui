using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Google.Android.Material.TextField;

namespace Microsoft.Maui.Platform;

internal class MauiMaterialEditText : TextInputEditText
{
	public event EventHandler? SelectionChanged;

	public MauiMaterialEditText(Context context) : base(MauiMaterialContextThemeWrapper.Create(context))
	{
	}

	protected MauiMaterialEditText(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
	{
	}

	public MauiMaterialEditText(Context context, IAttributeSet? attrs) : base(context, attrs)
	{
	}

	public MauiMaterialEditText(Context context, IAttributeSet? attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
	{
	}

	protected override void OnSelectionChanged(int selStart, int selEnd)
	{
		base.OnSelectionChanged(selStart, selEnd);

		SelectionChanged?.Invoke(this, EventArgs.Empty);
	}
}