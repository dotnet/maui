﻿using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Handlers
{
	public partial class ShapeViewHandler : ViewHandler<IShapeView, W2DGraphicsView>
	{
		protected override W2DGraphicsView CreatePlatformView()
			=> new W2DGraphicsView();

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			base.NeedsContainer;

		public static void MapBackground(IShapeViewHandler handler, IShapeView shapeView)
		{
			// If Fill and Background are not null, will use Fill for the Shape background
			// and Background for the ShapeView background.
			if (shapeView.Background is not null && shapeView.Fill is not null)
			{
				handler.UpdateValue(nameof(IViewHandler.ContainerView));
				handler.ToPlatform().UpdateBackground(shapeView);
			}

			if (shapeView.Background is not null || shapeView.Fill is not null)
			{
				handler.PlatformView?.InvalidateShape(shapeView);
			}
		}

		public static void MapShape(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.UpdateShape(shapeView);
		}

		public static void MapAspect(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		public static void MapFill(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		public static void MapStroke(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		public static void MapStrokeThickness(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		public static void MapStrokeDashPattern(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		public static void MapStrokeDashOffset(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		public static void MapStrokeLineCap(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		public static void MapStrokeLineJoin(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		public static void MapStrokeMiterLimit(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}
	}
}