#nullable disable
using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class LayoutAttributesChangedEventArgs22 : EventArgs
	{
		public UICollectionViewLayoutAttributes NewAttributes { get; }

		public LayoutAttributesChangedEventArgs2(UICollectionViewLayoutAttributes newAttributes) => NewAttributes = newAttributes;
	}
}
