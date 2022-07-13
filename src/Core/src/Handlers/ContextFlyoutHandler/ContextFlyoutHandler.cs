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
	public partial class ContextFlyoutHandler : IContextFlyoutHandler
	{

		public static IPropertyMapper<IContextFlyout, IContextFlyoutHandler> Mapper = new PropertyMapper<IContextFlyout, IContextFlyoutHandler>(ElementMapper)
		{
		};

		public static CommandMapper<IContextFlyout, IContextFlyoutHandler> CommandMapper = new(ElementCommandMapper)
		{
			[nameof(IContextFlyoutHandler.Add)] = MapAdd,
			[nameof(IContextFlyoutHandler.Remove)] = MapRemove,
			[nameof(IContextFlyoutHandler.Clear)] = MapClear,
			[nameof(IContextFlyoutHandler.Insert)] = MapInsert,
		};

		public ContextFlyoutHandler() : this(Mapper, CommandMapper)
		{

		}

		public ContextFlyoutHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
		{

		}

		public static void MapAdd(IContextFlyoutHandler handler, IContextFlyout menuElement, object? arg)
		{
			if (arg is ContextFlyoutItemHandlerUpdate args)
			{
				handler.Add(args.MenuElement);
			}
		}

		public static void MapRemove(IContextFlyoutHandler handler, IContextFlyout menuElement, object? arg)
		{
			if (arg is ContextFlyoutItemHandlerUpdate args)
			{
				handler.Remove(args.MenuElement);
			}
		}

		public static void MapInsert(IContextFlyoutHandler handler, IContextFlyout menuElement, object? arg)
		{
			if (arg is ContextFlyoutItemHandlerUpdate args)
			{
				handler.Insert(args.Index, args.MenuElement);
			}
		}

		public static void MapClear(IContextFlyoutHandler handler, IContextFlyout menuElement, object? arg)
		{
			handler.Clear();
		}

		IContextFlyout IContextFlyoutHandler.VirtualView => VirtualView;

		PlatformView IContextFlyoutHandler.PlatformView => PlatformView;

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
