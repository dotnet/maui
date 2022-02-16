using System;
using System.Collections.Generic;
using System.Text;
#if IOS || MACCATALYST
using PlatformView = UIKit.UIMenu;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.MenuFlyoutSubItem;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSubItemHandler : ElementHandler<IMenuFlyoutSubItem, PlatformView>, IMenuFlyoutSubItemHandler
	{
		public static IPropertyMapper<IMenuFlyoutSubItem, IMenuFlyoutSubItemHandler> Mapper = new PropertyMapper<IMenuFlyoutSubItem, IMenuFlyoutSubItemHandler>(ElementMapper)
		{
#if WINDOWS
			[nameof(IMenuFlyoutSubItem.Text)] = MapText,
			[nameof(IMenuFlyoutSubItem.Source)] = MapSource
#endif
		};

		public static CommandMapper<IMenuFlyoutSubItem, IMenuFlyoutSubItemHandler> CommandMapper = new(ElementCommandMapper)
		{
			[nameof(IMenuFlyoutSubItemHandler.Add)] = MapAdd,
			[nameof(IMenuFlyoutSubItemHandler.Remove)] = MapRemove,
			[nameof(IMenuFlyoutSubItemHandler.Clear)] = MapClear,
			[nameof(IMenuFlyoutSubItemHandler.Insert)] = MapInsert,
		};

		public MenuFlyoutSubItemHandler() : base(Mapper, CommandMapper)
		{

		}

		public static void MapAdd(IMenuFlyoutSubItemHandler handler, IMenuFlyoutItemBase layout, object? arg)
		{
			if (arg is MenuBarItemHandlerUpdate args)
			{
				handler.Add(args.MenuBarItemItem);
			}
		}

		public static void MapRemove(IMenuFlyoutSubItemHandler handler, IMenuFlyoutItemBase layout, object? arg)
		{
			if (arg is MenuBarItemHandlerUpdate args)
			{
				handler.Remove(args.MenuBarItemItem);
			}
		}

		public static void MapInsert(IMenuFlyoutSubItemHandler handler, IMenuFlyoutItemBase layout, object? arg)
		{
			if (arg is MenuBarItemHandlerUpdate args)
			{
				handler.Insert(args.Index, args.MenuBarItemItem);
			}
		}

		public static void MapClear(IMenuFlyoutSubItemHandler handler, IMenuFlyoutItemBase layout, object? arg)
		{
			handler.Clear();
		}

		IMenuFlyoutSubItem IMenuFlyoutSubItemHandler.VirtualView => VirtualView;

		PlatformView IMenuFlyoutSubItemHandler.NativeView => NativeView;
	}
}
