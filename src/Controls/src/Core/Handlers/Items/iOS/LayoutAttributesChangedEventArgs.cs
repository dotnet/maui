#nullable disable
using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class LayoutAttributesChangedEventArgs : EventArgs
	{
		public UICollectionViewLayoutAttributes NewAttributes { get; }

		public LayoutAttributesChangedEventArgs(UICollectionViewLayoutAttributes newAttributes) => NewAttributes = newAttributes;
	}
}
