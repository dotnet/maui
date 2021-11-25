using System;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;

namespace GraphicsTester.Maui
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformGraphicsView>
	{
		protected override PlatformGraphicsView CreateNativeView()
			=> new PlatformGraphicsView(Context);
	}
}

