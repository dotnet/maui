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
			var newCursorPosition = cursorOffset + (int)platformView.GetOffsetFromPosition(platformView.BeginningOfDocument, currentCursorPosition);

			return newCursorPosition < 0 ? 0 : newCursorPosition;
		}

		public static void SetCursorPosition(this IUITextInput platformView, int newCursorPosition)
		{
			var cursorPosition = platformView.GetPosition(platformView.BeginningOfDocument, newCursorPosition);
			platformView.SelectedTextRange = platformView.GetTextRange(cursorPosition, cursorPosition);
		}
	}
}

