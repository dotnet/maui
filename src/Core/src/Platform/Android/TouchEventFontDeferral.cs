using Android.Widget;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Postpones font changes that are requested while an EditText is handling a touch event or a long click
	/// until TextView has finished handling it.
	/// </summary>
	/// <remarks>
	/// Changing the typeface or text size nulls the TextView's text layout. An EditText gains focus in the middle
	/// of handling a tap (ACTION_UP) or a long click, so a font change made synchronously on focus (for example by
	/// a trigger on IsFocused) would null the layout before TextView finishes, and TextView would then skip
	/// everything it only does with a valid layout: cursor placement, word selection, the insertion handle and
	/// the soft keyboard request (#38819). Applying the font right afterwards keeps the native handling intact.
	/// </remarks>
	internal sealed class TouchEventFontDeferral
	{
		int _depth;
		ITextStyle? _pendingTextStyle;
		IFontManager? _pendingFontManager;

		internal void OnHandlingStarted() => _depth++;

		internal void OnHandlingFinished(TextView textView)
		{
			if (--_depth > 0 || _pendingTextStyle is not ITextStyle textStyle || _pendingFontManager is not IFontManager fontManager)
			{
				return;
			}

			_pendingTextStyle = null;
			_pendingFontManager = null;

			textView.UpdateFont(textStyle, fontManager);
		}

		internal bool TryDefer(ITextStyle textStyle, IFontManager fontManager)
		{
			if (_depth == 0)
			{
				return false;
			}

			// UpdateFont reads the current font when it runs, so only the latest request needs to be kept
			_pendingTextStyle = textStyle;
			_pendingFontManager = fontManager;

			return true;
		}
	}
}
