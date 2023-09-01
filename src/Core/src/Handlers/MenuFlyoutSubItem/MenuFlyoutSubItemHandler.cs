using System;
using System.Collections.Generic;
using System.Text;
#if IOS || MACCATALYST
using PlatformView = UIKit.UIMenu;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.MenuFlyoutSubItem;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
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
			[nameof(IMenuFlyoutSubItem.KeyboardAccelerators)] = MapKeyboardAccelerators,
			[nameof(IMenuFlyoutSubItem.Source)] = MapSource,
#endif
#if MACCATALYST || IOS || WINDOWS
			[nameof(IMenuFlyoutSubItem.IsEnabled)] = MapIsEnabled,
#endif
		};

		public static CommandMapper<IMenuFlyoutSubItem, IMenuFlyoutSubItemHandler> CommandMapper = new(ElementCommandMapper)
		{
			[nameof(IMenuFlyoutSubItemHandler.Add)] = MapAdd,
			[nameof(IMenuFlyoutSubItemHandler.Remove)] = MapRemove,
			[nameof(IMenuFlyoutSubItemHandler.Clear)] = MapClear,
			[nameof(IMenuFlyoutSubItemHandler.Insert)] = MapInsert,
		};

#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		public MenuFlyoutSubItemHandler() : this(Mapper, CommandMapper)
		{

		}

#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		public MenuFlyoutSubItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
		{

		}

		public static void MapAdd(IMenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
		{
			if (arg is MenuFlyoutSubItemHandlerUpdate args)
			{
				handler.Add(args.MenuElement);
			}
		}

		public static void MapRemove(IMenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
		{
			if (arg is MenuFlyoutSubItemHandlerUpdate args)
			{
				handler.Remove(args.MenuElement);
			}
		}

		public static void MapInsert(IMenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
		{
			if (arg is MenuFlyoutSubItemHandlerUpdate args)
			{
				handler.Insert(args.Index, args.MenuElement);
			}
		}

		public static void MapClear(IMenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
		{
			handler.Clear();
		}
#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		IMenuFlyoutSubItem IMenuFlyoutSubItemHandler.VirtualView => VirtualView;
#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		PlatformView IMenuFlyoutSubItemHandler.PlatformView => PlatformView;
#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		private protected override void OnDisconnectHandler(object platformView)
		{
			base.OnDisconnectHandler(platformView);
			foreach (var item in VirtualView)
				item?.Handler?.DisconnectHandler();
		}
	}
}
