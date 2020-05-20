using System;
using System.Collections.Generic;
using System.Text;
using AppKit;

namespace System.Maui.Platform
{
	public partial class LayoutRenderer : AbstractViewRenderer<ILayout, NSView>
	{
		protected override NSView CreateView()
		{
			return new NSView();
		}
	}
}
