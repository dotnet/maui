using System;
using Android.Content;
using Google.Android.Material.TextView;
using Java.Lang;

namespace Microsoft.Maui.Platform;

// TODO: Material3 - make it public in .net 11
internal class MauiMaterialTextView : MaterialTextView
{
	// Track text type to optimize layout events - FormattedText needs span recalculation, plain text doesn't
	bool _isFormatted;

	public MauiMaterialTextView(Context context) : base(MauiMaterialContextThemeWrapper.Create(context))
	{
	}

	internal event EventHandler<LayoutChangedEventArgs>? LayoutChanged;

	public override void SetText(ICharSequence? text, BufferType? type)
	{
		// FormattedText uses SpannableString, plain text uses string-based types
		// Only formatted text needs layout event handling for span position recalculation  
		_isFormatted = text is not Java.Lang.String;
		base.SetText(text, type);
	}

	protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
	{
		base.OnLayout(changed, left, top, right, bottom);

		// Only notify for FormattedText - spans need position updates when layout changes
		if (_isFormatted)
		{
			LayoutChanged?.Invoke(this, new LayoutChangedEventArgs(left, top, right, bottom));
		}
	}
}