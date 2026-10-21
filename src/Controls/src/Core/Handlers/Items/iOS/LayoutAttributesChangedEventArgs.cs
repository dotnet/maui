#nullable disable
using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	[Obsolete("This type is obsolete on iOS and Mac Catalyst. Use Microsoft.Maui.Controls.Handlers.Items2.LayoutAttributesChangedEventArgs2 instead.")]
	public class LayoutAttributesChangedEventArgs : EventArgs
	{
		public UICollectionViewLayoutAttributes NewAttributes { get; }

		public LayoutAttributesChangedEventArgs(UICollectionViewLayoutAttributes newAttributes) => NewAttributes = newAttributes;
	}
}
