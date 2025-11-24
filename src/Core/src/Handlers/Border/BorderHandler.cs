#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.ContentView;
#elif __ANDROID__
using PlatformView = Microsoft.Maui.Platform.ContentViewGroup;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.ContentPanel;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.ContentViewGroup;
#elif (NETSTANDARD || !PLATFORM)
using PlatformView = System.Object;
#endif

using System;
using System.Runtime.CompilerServices;
#if IOS || MACCATALYST
using CoreAnimation;
using CoreGraphics;
#endif
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Represents the view handler for the abstract <see cref="IBorderView"/> view and its platform-specific implementation.
	/// </summary>
	/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/handlers/">Conceptual documentation on handlers</seealso>
	public partial class BorderHandler : IBorderHandler
	{
		public static IPropertyMapper<IBorderView, IBorderHandler> Mapper = new PropertyMapper<IBorderView, IBorderHandler>(ViewMapper)
		{
#if __ANDROID__
			[nameof(IBorderView.Height)] = MapHeight,
			[nameof(IBorderView.Width)] = MapWidth,
#endif
#if IOS || MACCATALYST
			[nameof(IView.Shadow)] = MapShadow,
#endif
			[nameof(IBorderView.Background)] = MapBackground,
			[nameof(IBorderView.Content)] = MapContent,
			[nameof(IBorderView.Shape)] = MapStrokeShape,
			[nameof(IBorderView.Stroke)] = MapStroke,
			[nameof(IBorderView.StrokeThickness)] = MapStrokeThickness,
			[nameof(IBorderView.StrokeLineCap)] = MapStrokeLineCap,
			[nameof(IBorderView.StrokeLineJoin)] = MapStrokeLineJoin,
			[nameof(IBorderView.StrokeDashPattern)] = MapStrokeDashPattern,
			[nameof(IBorderView.StrokeDashOffset)] = MapStrokeDashOffset,
			[nameof(IBorderView.StrokeMiterLimit)] = MapStrokeMiterLimit
		};

		public static CommandMapper<IBorderView, BorderHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		private Size _lastSize;

		public BorderHandler() : base(Mapper, CommandMapper)
		{

		}

		public BorderHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public BorderHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IBorderView IBorderHandler.VirtualView => VirtualView;

		PlatformView IBorderHandler.PlatformView => PlatformView;

		/// <inheritdoc />
		public override void PlatformArrange(Rect rect)
		{
			base.PlatformArrange(rect);

			if (_lastSize != rect.Size)
			{
				_lastSize = rect.Size;
				UpdateValue(nameof(IBorderStroke.Shape));
			}
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Background"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapBackground(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateBackground(border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Shadow"/> property to the platform-specific implementations while preserving existing transforms on iOS/Mac Catalyst.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		static void MapShadow(IBorderHandler handler, IBorderView border)
		{
#if IOS || MACCATALYST
			const double epsilon = 0.0001d;
			CATransform3D existingTransform = CATransform3D.Identity;
			CGPoint existingAnchorPoint = new CGPoint(0.5f, 0.5f);
			bool hasCustomTransform = false;
			bool transformFromWrapper = false;

			if (handler is IPlatformViewHandler prePlatformHandler &&
				prePlatformHandler.ContainerView is Microsoft.Maui.Platform.WrapperView existingWrapper &&
				existingWrapper.Layer is CALayer existingWrapperLayer)
			{
				existingTransform = existingWrapperLayer.Transform;
				existingAnchorPoint = existingWrapperLayer.AnchorPoint;
				hasCustomTransform = !existingTransform.IsIdentity ||
					Math.Abs(existingAnchorPoint.X - 0.5) > epsilon ||
					Math.Abs(existingAnchorPoint.Y - 0.5) > epsilon;
				transformFromWrapper = hasCustomTransform;
			}

			if (!transformFromWrapper && handler.PlatformView?.Layer is CALayer existingChildLayer)
			{
				existingTransform = existingChildLayer.Transform;
				existingAnchorPoint = existingChildLayer.AnchorPoint;
				hasCustomTransform = !existingTransform.IsIdentity ||
					Math.Abs(existingAnchorPoint.X - 0.5) > epsilon ||
					Math.Abs(existingAnchorPoint.Y - 0.5) > epsilon;
			}

			ViewHandler.MapShadow((IViewHandler)handler, border);

			if (!hasCustomTransform)
			{
				return;
			}

			if (handler is IPlatformViewHandler platformHandlerAfter &&
				platformHandlerAfter.ContainerView is Microsoft.Maui.Platform.WrapperView wrapperAfter &&
				wrapperAfter.Layer is CALayer wrapperLayerAfter &&
				handler.PlatformView?.Layer is CALayer childLayerAfter)
			{
				wrapperLayerAfter.Transform = existingTransform;
				wrapperLayerAfter.AnchorPoint = existingAnchorPoint;

				childLayerAfter.Transform = CATransform3D.Identity;
				childLayerAfter.AnchorPoint = new CGPoint(0.5f, 0.5f);
				handler.PlatformView.Frame = wrapperAfter.Bounds;
			}
			else if (handler.PlatformView?.Layer is CALayer childLayerFinal)
			{
				childLayerFinal.Transform = existingTransform;
				childLayerFinal.AnchorPoint = existingAnchorPoint;
			}
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool ShouldSkipStrokeMappings(IBorderHandler handler)
		{
#if __IOS__ || MACCATALYST || ANDROID
			// During the initial connection, the `MapBackground` takes care of updating the stroke properties
			// so we can skip the stroke mappings to avoid repetitive and useless updates.
			return handler.IsConnectingHandler();
#else
			return false;
#endif
		}

		/// <summary>
		/// Maps the abstract <see cref="IBorderStroke.Shape"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeShape(IBorderHandler handler, IBorderView border)
		{
			if (ShouldSkipStrokeMappings(handler))
			{
				return;
			}

			((PlatformView?)handler.PlatformView)?.UpdateStrokeShape(border);
			MapBackground(handler, border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.Stroke"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStroke(IBorderHandler handler, IBorderView border)
		{
			if (ShouldSkipStrokeMappings(handler))
			{
				return;
			}

			((PlatformView?)handler.PlatformView)?.UpdateStroke(border);
			MapBackground(handler, border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.StrokeThickness"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeThickness(IBorderHandler handler, IBorderView border)
		{
			if (ShouldSkipStrokeMappings(handler))
			{
				return;
			}

			((PlatformView?)handler.PlatformView)?.UpdateStrokeThickness(border);
			MapBackground(handler, border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.StrokeLineCap"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeLineCap(IBorderHandler handler, IBorderView border)
		{
			if (ShouldSkipStrokeMappings(handler))
			{
				return;
			}

			((PlatformView?)handler.PlatformView)?.UpdateStrokeLineCap(border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.StrokeLineJoin"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeLineJoin(IBorderHandler handler, IBorderView border)
		{
			if (ShouldSkipStrokeMappings(handler))
			{
				return;
			}

			((PlatformView?)handler.PlatformView)?.UpdateStrokeLineJoin(border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.StrokeDashPattern"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeDashPattern(IBorderHandler handler, IBorderView border)
		{
			if (ShouldSkipStrokeMappings(handler))
			{
				return;
			}

			((PlatformView?)handler.PlatformView)?.UpdateStrokeDashPattern(border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.StrokeDashOffset"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeDashOffset(IBorderHandler handler, IBorderView border)
		{
			if (ShouldSkipStrokeMappings(handler))
			{
				return;
			}

			((PlatformView?)handler.PlatformView)?.UpdateStrokeDashOffset(border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.StrokeMiterLimit"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeMiterLimit(IBorderHandler handler, IBorderView border)
		{
			if (ShouldSkipStrokeMappings(handler))
			{
				return;
			}

			((PlatformView?)handler.PlatformView)?.UpdateStrokeMiterLimit(border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IContentView.Content"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapContent(IBorderHandler handler, IBorderView border)
		{
			UpdateContent(handler);
		}

		static partial void UpdateContent(IBorderHandler handler);

#if __ANDROID__
		/// <summary>
		/// Maps the abstract <see cref="IView.Width"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static partial void MapWidth(IBorderHandler handler, IBorderView border);

		/// <summary>
		/// Maps the abstract <see cref="IView.Height"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static partial void MapHeight(IBorderHandler handler, IBorderView border);
#endif
	}
}
