using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native;

namespace Microsoft.Maui
{
	public partial class VisualDiagnosticsLayer : IVisualDiagnosticsLayer, IDrawable
	{
		public bool DisableUITouchEventPassthrough { get; set; }

		public NativeGraphicsView? VisualDiagnosticsGraphicsView { get; internal set; }

		public void InitializeNativeLayer(IMauiContext context, UIKit.UIViewController nativeLayer)
		{
			this.IsNativeViewInitialized = true;
		}

		public void Invalidate()
		{
			this.VisualDiagnosticsGraphicsView?.InvalidateDrawable();
		}
	}
}
