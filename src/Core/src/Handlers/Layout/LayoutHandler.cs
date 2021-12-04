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
			[nameof(ILayout.Background)] = MapBackground
		};

		public static CommandMapper<ILayout, ILayoutHandler> LayoutCommandMapper = new(ViewCommandMapper)
		{
			[nameof(ILayoutHandler.Add)] = MapAdd,
			[nameof(ILayoutHandler.Remove)] = MapRemove,
			[nameof(ILayoutHandler.Clear)] = MapClear,
			[nameof(ILayoutHandler.Insert)] = MapInsert,
			[nameof(ILayoutHandler.Update)] = MapUpdate,
			[nameof(ILayoutHandler.UpdateZIndex)] = MapUpdateZIndex,
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
			((NativeView?)handler.NativeView)?.UpdateBackground(layout);
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

		private static void MapUpdateZIndex(ILayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is IView view)
			{
				handler.UpdateZIndex(view);
			}
		}
	}
}
