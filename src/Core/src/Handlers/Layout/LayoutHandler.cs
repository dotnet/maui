#nullable enable
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIView;
#elif __ANDROID__
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ILayoutHandler
	{
		public static IPropertyMapper<ILayout, ILayoutHandler> LayoutMapper = new PropertyMapper<ILayout, ILayoutHandler>(ViewMapper)
		{
			[nameof(ILayout.Background)] = MapBackground,
			[nameof(ILayout.Shape)] = MapStrokeShape,
			[nameof(ILayout.Stroke)] = MapStroke,
			[nameof(ILayout.StrokeThickness)] = MapStrokeThickness,
			[nameof(ILayout.StrokeLineCap)] = MapStrokeLineCap,
			[nameof(ILayout.StrokeLineJoin)] = MapStrokeLineJoin,
			[nameof(ILayout.StrokeDashPattern)] = MapStrokeDashPattern,
			[nameof(ILayout.StrokeDashOffset)] = MapStrokeDashOffset,
			[nameof(ILayout.StrokeMiterLimit)] = MapStrokeMiterLimit
		};

		public static CommandMapper<ILayout, ILayoutHandler> LayoutCommandMapper = new(ViewCommandMapper)
		{
			[nameof(ILayoutHandler.Add)] = MapAdd,
			[nameof(ILayoutHandler.Remove)] = MapRemove,
			[nameof(ILayoutHandler.Clear)] = MapClear,
			[nameof(ILayoutHandler.Insert)] = MapInsert,
			[nameof(ILayoutHandler.Update)] = MapUpdate,
		};

		public LayoutHandler() : base(LayoutMapper, LayoutCommandMapper)
		{

		}

		public LayoutHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
			: base(mapper ?? LayoutMapper, commandMapper ?? LayoutCommandMapper)
		{

		}

		public static void MapBackground(ILayoutHandler handler, ILayout layout)
		{
#if WINDOWS
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			bool hasBorder = layout.Shape != null && layout.Stroke != null;

 			if(!hasBorder)
 				((NativeView?)handler.NativeView)?.UpdateBackground(layout);
 			else
 				((WrapperView?)handler.ContainerView)?.UpdateBackground(layout);
#endif
			((NativeView?)handler.NativeView)?.UpdateBackground(layout);
		}
	
		public static void MapStrokeShape(ILayoutHandler handler, ILayout layout)
		{
#if WINDOWS
#else
			((NativeView?)handler.NativeView)?.UpdateStrokeShape(layout);
#endif

			MapBackground(handler, layout);
		}

		public static void MapStroke(ILayoutHandler handler, ILayout layout)
		{
#if WINDOWS
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			((WrapperView?)handler.ContainerView)?.UpdateStroke(layout);
#else
			((NativeView?)handler.NativeView)?.UpdateStroke(layout);
#endif
			MapBackground(handler, layout);
		}

		public static void MapStrokeThickness(ILayoutHandler handler, ILayout layout)
		{
#if WINDOWS
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			((WrapperView?)handler.ContainerView)?.UpdateStrokeThickness(layout);
#else
			((NativeView?)handler.NativeView)?.UpdateStrokeThickness(layout);
#endif
			MapBackground(handler, layout);
		}

		public static void MapStrokeLineCap(ILayoutHandler handler, ILayout layout)
		{
#if WINDOWS
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			((WrapperView?)handler.ContainerView)?.UpdateStrokeLineCap(layout);
#else
			((NativeView?)handler.NativeView)?.UpdateStrokeLineCap(layout);
#endif
		}

		public static void MapStrokeLineJoin(ILayoutHandler handler, ILayout layout)
		{
#if WINDOWS
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			((WrapperView?)handler.ContainerView)?.UpdateStrokeLineJoin(layout);
#else
			((NativeView?)handler.NativeView)?.UpdateStrokeLineJoin(layout);
#endif
		}

		public static void MapStrokeDashPattern(ILayoutHandler handler, ILayout layout)
		{
#if WINDOWS
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			((WrapperView?)handler.ContainerView)?.UpdateStrokeDashPattern(layout);
#else
			((NativeView?)handler.NativeView)?.UpdateStrokeDashPattern(layout);
#endif
		}

		public static void MapStrokeDashOffset(ILayoutHandler handler, ILayout layout)
		{
#if WINDOWS
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			((WrapperView?)handler.ContainerView)?.UpdateStrokeDashOffset(layout);
#else
			((NativeView?)handler.NativeView)?.UpdateStrokeDashOffset(layout);
#endif
		}

		public static void MapStrokeMiterLimit(ILayoutHandler handler, ILayout layout)
		{
#if WINDOWS
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			((WrapperView?)handler.ContainerView)?.UpdateStrokeMiterLimit(layout);
#else
			((NativeView?)handler.NativeView)?.UpdateStrokeMiterLimit(layout);
#endif
		}

		public static void MapAdd(ILayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is LayoutHandlerUpdate args)
			{
				handler.Add(args.View);
			}
		}

		public static void MapRemove(ILayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is LayoutHandlerUpdate args)
			{
				handler.Remove(args.View);
			}
		}

		public static void MapInsert(ILayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is LayoutHandlerUpdate args)
			{
				handler.Insert(args.Index, args.View);
			}
		}

		public static void MapClear(ILayoutHandler handler, ILayout layout, object? arg)
		{
			handler.Clear();
		}

		private static void MapUpdate(ILayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is LayoutHandlerUpdate args)
			{
				handler.Update(args.Index, args.View);
			}
		}
	}
}
