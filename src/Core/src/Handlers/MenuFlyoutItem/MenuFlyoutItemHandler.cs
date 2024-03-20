using System;
using System.Collections.Generic;
using System.Text;
#if IOS || MACCATALYST
using PlatformView = UIKit.UIMenuElement;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.MenuFlyoutItem;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif GTK

#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutItemHandler : ElementHandler<IMenuFlyoutItem, MauiMenuItem>,
		IMenuFlyoutItemHandler
	{
		public static IPropertyMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler> Mapper = new PropertyMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler>(ElementMapper)
		{
#if WINDOWS
			[nameof(IMenuFlyoutSubItem.Text)] = MapText,
			[nameof(IMenuFlyoutItem.KeyboardAccelerators)] = MapKeyboardAccelerators,
			[nameof(IMenuElement.Source)] = MapSource,
#endif
#if MACCATALYST || IOS || WINDOWS || GTK
			[nameof(IMenuElement.IsEnabled)] = MapIsEnabled
#endif
		};

		public static CommandMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler> CommandMapper = new(ElementCommandMapper)
		{
		};

#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		public MenuFlyoutItemHandler() : base(Mapper, CommandMapper)
		{

		}

#if !WINDOWS && !IOS && !GTK
		protected override PlatformView CreatePlatformElement()
		{
			throw new NotImplementedException();
		}
#endif
#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		IMenuFlyoutItem IMenuFlyoutItemHandler.VirtualView => VirtualView;
#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		MauiMenuItem IMenuFlyoutItemHandler.PlatformView => PlatformView;
	}
}
