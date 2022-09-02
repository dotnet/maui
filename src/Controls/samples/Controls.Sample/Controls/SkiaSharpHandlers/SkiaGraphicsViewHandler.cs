/*
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

#if TIZEN
using Tizen.UIExtensions.NUI.GraphicsView;
#elif !(NETSTANDARD || !PLATFORM)
using Microsoft.Maui.Graphics.Skia.Views;
#else
using SkiaGraphicsView = System.Object;
#endif

namespace Maui.Controls.Sample.Controls
{
	public partial class SkiaGraphicsViewHandler : ViewHandler<IGraphicsView, SkiaGraphicsView>
	{
		public static IPropertyMapper<IGraphicsView, SkiaGraphicsViewHandler> GraphicsViewMapper =
			new PropertyMapper<IGraphicsView, SkiaGraphicsViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IGraphicsView.Drawable)] = MapDrawable
			};

		public SkiaGraphicsViewHandler()
			: base(GraphicsViewMapper)
		{
		}

		protected override SkiaGraphicsView CreatePlatformView()
		{
#if __ANDROID__
			return new SkiaGraphicsView(Context);
#else
			return new SkiaGraphicsView();
#endif
		}

		public static void MapDrawable(SkiaGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
#if !(NETSTANDARD || !PLATFORM)
			handler.PlatformView.Drawable = graphicsView.Drawable;
#endif
		}
	}
}
*/