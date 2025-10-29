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

			return Math.Max(0, newCursorPosition);
		}

		public static void SetTextRange(this IUITextInput platformView, int start, int selectedTextLength)
		{
			int end = start + selectedTextLength;

			// Let's be sure we have positive positions
			start = Math.Max(start, 0);
			end = Math.Max(end, 0);

			// Switch start and end positions if necessary
			start = Math.Min(start, end);
			end = Math.Max(start, end);

			var startPosition = platformView.GetPosition(platformView.BeginningOfDocument, start);
			var endPosition = platformView.GetPosition(platformView.BeginningOfDocument, end);
			platformView.SelectedTextRange = platformView.GetTextRange(startPosition, endPosition);
		}

		public static int GetSelectedTextLength(this IUITextInput platformView)
		{
			var selectedTextRange = platformView.SelectedTextRange;

			if (selectedTextRange == null)
				return 0;

			return (int)platformView.GetOffsetFromPosition(selectedTextRange.Start, selectedTextRange.End);
		}
	}
}

