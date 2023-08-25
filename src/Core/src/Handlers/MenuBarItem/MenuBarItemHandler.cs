using System;
using System.Collections.Generic;
using System.Text;
#if IOS || MACCATALYST
using PlatformView = UIKit.UIMenu;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.MenuBarItem;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarItemHandler : IMenuBarItemHandler
	{

		public static IPropertyMapper<IMenuBarItem, IMenuBarItemHandler> Mapper = new PropertyMapper<IMenuBarItem, IMenuBarItemHandler>(ElementMapper)
		{
#if WINDOWS
			[nameof(IMenuBarItem.Text)] = MapText,
#endif
#if MACCATALYST || IOS || WINDOWS
			[nameof(IMenuBarItem.IsEnabled)] = MapIsEnabled,
#endif
		};

		public static CommandMapper<IMenuBarItem, IMenuBarItemHandler> CommandMapper = new(ElementCommandMapper)
		{
			[nameof(IMenuBarItemHandler.Add)] = MapAdd,
			[nameof(IMenuBarItemHandler.Remove)] = MapRemove,
			[nameof(IMenuBarItemHandler.Clear)] = MapClear,
			[nameof(IMenuBarItemHandler.Insert)] = MapInsert,
		};
#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		public MenuBarItemHandler() : this(Mapper, CommandMapper)
		{

		}
#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		public MenuBarItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
		{

		}

		public static void MapAdd(IMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
		{
			if (arg is MenuBarItemHandlerUpdate args)
			{
				handler.Add(args.MenuElement);
			}
		}

		public static void MapRemove(IMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
		{
			if (arg is MenuBarItemHandlerUpdate args)
			{
				handler.Remove(args.MenuElement);
			}
		}

		public static void MapInsert(IMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
		{
			if (arg is MenuBarItemHandlerUpdate args)
			{
				handler.Insert(args.Index, args.MenuElement);
			}
		}

		public static void MapClear(IMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
		{
			handler.Clear();
		}
#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		IMenuBarItem IMenuBarItemHandler.VirtualView => VirtualView;
#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		PlatformView IMenuBarItemHandler.PlatformView => PlatformView;
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
