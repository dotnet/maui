#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Platform;
using System.ComponentModel;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class VisualElementRenderer<TElement> : UIView, INativeViewHandler
		where TElement : Element, IView
	{
		public virtual UIViewController? ViewController => null;
	}
}
