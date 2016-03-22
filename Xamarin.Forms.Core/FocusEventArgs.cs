using System;

namespace Xamarin.Forms
{
	public class FocusEventArgs : EventArgs
	{
		public FocusEventArgs(VisualElement visualElement, bool isFocused)
		{
			if (visualElement == null)
				throw new ArgumentNullException("visualElement");

			VisualElement = visualElement;
			IsFocused = isFocused;
		}

		public bool IsFocused { get; private set; }

		public VisualElement VisualElement { get; private set; }
	}
}