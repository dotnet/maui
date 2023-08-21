// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Controls.Platform
{
	public class VisualElementChangedEventArgs : ElementChangedEventArgs<VisualElement>
	{
		public VisualElementChangedEventArgs(VisualElement? oldElement, VisualElement? newElement) : base(oldElement, newElement)
		{
		}
	}

	public class ElementChangedEventArgs<TElement> : EventArgs where TElement : Element
	{
		public ElementChangedEventArgs(TElement? oldElement, TElement? newElement)
		{
			OldElement = oldElement;
			NewElement = newElement;
		}

		public TElement? NewElement { get; private set; }

		public TElement? OldElement { get; private set; }
	}
}