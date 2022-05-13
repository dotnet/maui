using System;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class TextInputExtensions
	{
		public static int GetCursorPosition(this IUITextInput platformView, int cursorOffset = 0)
		{
			var zeroPosition = platformView.GetPosition(platformView.BeginningOfDocument, 0);
			var currentCursorPosition = platformView.SelectedTextRange?.Start ?? zeroPosition;
			var newCursorPosition = platformView.GetPosition(currentCursorPosition, cursorOffset);

			return (int)platformView.GetOffsetFromPosition(platformView.BeginningOfDocument, newCursorPosition);
		}
	}
}

