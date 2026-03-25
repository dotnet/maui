using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Platform
{
	public static partial class SemanticExtensions
	{
		internal static bool HasAccessibleTapGesture(this View virtualView) =>
			HasAccessibleTapGesture(virtualView, out _);

		internal static bool HasAccessibleTapGesture(
			this View virtualView,
			[NotNullWhen(true)] out TapGestureRecognizer? tapGestureRecognizer)
		{
			foreach (var gesture in virtualView.GestureRecognizers)
			{
				//Accessibility can't handle Tap Recognizers with > 1 tap
				if (gesture is TapGestureRecognizer tgr && tgr.NumberOfTapsRequired == 1)
				{
					tapGestureRecognizer = tgr;
					return (tgr.Buttons & ButtonsMask.Primary) == ButtonsMask.Primary;
				}
			}
			tapGestureRecognizer = null;
			return false;
		}
	}
}
