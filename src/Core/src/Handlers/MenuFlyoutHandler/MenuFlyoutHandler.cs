using System;
using System.Collections.Generic;
using System.Text;
#if WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.MenuFlyout;
#else
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutHandler : IMenuFlyoutHandler
	{

		public static IPropertyMapper<IMenuFlyout, IMenuFlyoutHandler> Mapper = new PropertyMapper<IMenuFlyout, IMenuFlyoutHandler>(ElementMapper)
		{
		};

		public static CommandMapper<IMenuFlyout, IMenuFlyoutHandler> CommandMapper = new(ElementCommandMapper)
		{
			[nameof(IMenuFlyoutHandler.Add)] = MapAdd,
			[nameof(IMenuFlyoutHandler.Remove)] = MapRemove,
			[nameof(IMenuFlyoutHandler.Clear)] = MapClear,
			[nameof(IMenuFlyoutHandler.Insert)] = MapInsert,
		};

		public MenuFlyoutHandler() : this(Mapper, CommandMapper)
		{

		}

		public MenuFlyoutHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
		{

		}

		public static void MapAdd(IMenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
		{
			if (arg is ContextFlyoutItemHandlerUpdate args)
			{
				handler.Add(args.MenuElement);
			}
		}

		public static void MapRemove(IMenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
		{
			if (arg is ContextFlyoutItemHandlerUpdate args)
			{
				handler.Remove(args.MenuElement);
			}
		}

		public static void MapInsert(IMenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
		{
			if (arg is ContextFlyoutItemHandlerUpdate args)
			{
				handler.Insert(args.Index, args.MenuElement);
			}
		}

		public static void MapClear(IMenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
		{
			handler.Clear();
		}

		IMenuFlyout IMenuFlyoutHandler.VirtualView => VirtualView;

		PlatformView IMenuFlyoutHandler.PlatformView => PlatformView;

		private protected override void OnDisconnectHandler(object platformView)
		{
			base.OnDisconnectHandler(platformView);
			foreach (var item in VirtualView)
			{
				item?.Handler?.DisconnectHandler();
			}
		}
	}
}
