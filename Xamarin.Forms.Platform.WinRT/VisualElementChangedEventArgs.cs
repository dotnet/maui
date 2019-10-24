using System;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class VisualElementChangedEventArgs : ElementChangedEventArgs<VisualElement>
	{
		public VisualElementChangedEventArgs(VisualElement oldElement, VisualElement newElement) : base(oldElement, newElement)
		{
		}
	}

	public class ElementChangedEventArgs<TElement> : EventArgs where TElement : Element
	{
		public ElementChangedEventArgs(TElement oldElement, TElement newElement)
		{
			OldElement = oldElement;
			NewElement = newElement;
		}

		public TElement NewElement { get; private set; }

		public TElement OldElement { get; private set; }
	}
}