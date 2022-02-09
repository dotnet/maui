using System;
using System.Collections.Generic;
using System.Text;
#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.MenuFlyoutItem;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutItemHandler : MenuFlyoutItemBaseHandler<IMenuFlyoutItem, PlatformView>,
		IMenuFlyoutItemHandler
	{
		public static IPropertyMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler> Mapper = new PropertyMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler>(ElementMapper)
		{
#if WINDOWS
			[nameof(IMenuFlyoutSubItem.Text)] = MapText,
#endif
		};

		public static CommandMapper<IMenuFlyoutSubItem, IMenuFlyoutItemHandler> CommandMapper = new(ElementCommandMapper)
		{
		};

		public MenuFlyoutItemHandler() : base(Mapper, CommandMapper)
		{

		}

		protected override PlatformView CreateNativeElement()
		{
#if WINDOWS
			return new PlatformView();
#else
			throw new NotImplementedException();
#endif
		}
	}
}
