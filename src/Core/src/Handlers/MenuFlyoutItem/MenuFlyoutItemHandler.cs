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
using PlatformView = ElmSharp.EvasObject;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutItemHandler : ElementHandler<IMenuFlyoutItem, PlatformView>,
		IMenuFlyoutItemHandler
	{
		public static IPropertyMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler> Mapper = new PropertyMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler>(ElementMapper)
		{
#if WINDOWS
			[nameof(IMenuFlyoutSubItem.Text)] = MapText,
			[nameof(IMenuElement.Source)] = MapSource,
			[nameof(IMenuElement.IsEnabled)] = MapIsEnabled
#endif
		};

		public static CommandMapper<IMenuFlyoutSubItem, IMenuFlyoutItemHandler> CommandMapper = new(ElementCommandMapper)
		{
		};

		public MenuFlyoutItemHandler() : base(Mapper, CommandMapper)
		{

		}

#if !WINDOWS && !IOS
		protected override PlatformView CreatePlatformElement()
		{
			throw new NotImplementedException();
		}
#endif

		IMenuFlyoutItem IMenuFlyoutItemHandler.VirtualView => VirtualView;

		PlatformView IMenuFlyoutItemHandler.PlatformView => PlatformView;
	}
}
