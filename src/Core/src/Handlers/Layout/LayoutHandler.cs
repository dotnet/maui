#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.LayoutView;
#elif __ANDROID__
using PlatformView = Microsoft.Maui.Platform.LayoutViewGroup;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.LayoutPanel;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.LayoutViewGroup;
#elif (NETSTANDARD || !PLATFORM)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Represents the view handler for the abstract <see cref="ILayout"/> view and its platform-specific implementation.
	/// </summary>
	/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/handlers/">Conceptual documentation on handlers</seealso>
	public partial class LayoutHandler : ILayoutHandler
	{
		public static IPropertyMapper<ILayout, ILayoutHandler> Mapper = new PropertyMapper<ILayout, ILayoutHandler>(ViewMapper)
		{
			[nameof(ILayout.Background)] = MapBackground,
			[nameof(ILayout.ClipsToBounds)] = MapClipsToBounds,
#if ANDROID || WINDOWS
			[nameof(IView.InputTransparent)] = MapInputTransparent,
#endif
		};

		public static CommandMapper<ILayout, ILayoutHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(ILayoutHandler.Add)] = MapAdd,
			[nameof(ILayoutHandler.Remove)] = MapRemove,
			[nameof(ILayoutHandler.Clear)] = MapClear,
			[nameof(ILayoutHandler.Insert)] = MapInsert,
			[nameof(ILayoutHandler.Update)] = MapUpdate,
			[nameof(ILayoutHandler.UpdateZIndex)] = MapUpdateZIndex,
		};

		public LayoutHandler() : base(Mapper, CommandMapper)
		{
		}

		public LayoutHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{

		}

		ILayout ILayoutHandler.VirtualView => VirtualView;

		PlatformView ILayoutHandler.PlatformView => PlatformView;

		/// <summary>
		/// Maps the abstract <see cref="IView.Background"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="layout">The associated <see cref="ILayout"/> instance.</param>
		public static partial void MapBackground(ILayoutHandler handler, ILayout layout);

#if ANDROID || WINDOWS
		/// <summary>
		/// Maps the abstract <see cref="IView.InputTransparent"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="layout">The associated <see cref="ILayout"/> instance.</param>
		public static partial void MapInputTransparent(ILayoutHandler handler, ILayout layout);
#endif

		/// <summary>
		/// Maps the abstract <see cref="ILayout.ClipsToBounds"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="layout">The associated <see cref="ILayout"/> instance.</param>
		public static void MapClipsToBounds(ILayoutHandler handler, ILayout layout)
		{
			((PlatformView?)handler.PlatformView)?.UpdateClipsToBounds(layout);
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

		static void MapUpdate(ILayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is LayoutHandlerUpdate args)
			{
				handler.Update(args.Index, args.View);
			}
		}

		static void MapUpdateZIndex(ILayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is IView view)
			{
				handler.UpdateZIndex(view);
			}
		}

		/// <summary>
		/// Converts a FlowDirection to the appropriate FlowDirection for cross-platform layout 
		/// </summary>
		/// <param name="flowDirection"></param>
		/// <returns>The FlowDirection to assume for cross-platform layout</returns>
		internal static FlowDirection GetLayoutFlowDirection(FlowDirection flowDirection)
		{
#if WINDOWS
			// The native LayoutPanel in Windows will automagically flip our layout coordinates if it's in RTL mode.
			// So for cross-platform layout purposes, we just always treat things as being LTR and let the Panel sort out the rest.
			return FlowDirection.LeftToRight;
#else
			return flowDirection;
#endif
		}
	}
}
