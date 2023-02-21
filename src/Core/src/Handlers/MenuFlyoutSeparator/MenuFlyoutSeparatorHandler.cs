#if IOS || MACCATALYST
using PlatformView = UIKit.UIMenu;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS)
using System;
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSeparatorHandler : ElementHandler<IMenuFlyoutSeparator, PlatformView>, IMenuFlyoutSeparatorHandler
	{
		public static IPropertyMapper<IMenuFlyoutSeparator, IMenuFlyoutSeparatorHandler> Mapper = new PropertyMapper<IMenuFlyoutSeparator, IMenuFlyoutSeparatorHandler>(ElementMapper)
		{
		};

		public static CommandMapper<IMenuFlyoutSeparator, IMenuFlyoutSeparatorHandler> CommandMapper = new(ElementCommandMapper)
		{
		};

#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		public MenuFlyoutSeparatorHandler() : this(Mapper, CommandMapper)
		{

		}

#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		public MenuFlyoutSeparatorHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
		{

		}

#if !WINDOWS && !IOS
		protected override PlatformView CreatePlatformElement()
		{
			throw new NotImplementedException();
		}
#endif

#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		IMenuFlyoutSeparator IMenuFlyoutSeparatorHandler.VirtualView => VirtualView;
#if IOS
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
		PlatformView IMenuFlyoutSeparatorHandler.PlatformView => PlatformView;
	}
}
