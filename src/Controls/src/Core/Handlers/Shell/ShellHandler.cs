#nullable disable
#if WINDOWS || TIZEN
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler
	{
		public static PropertyMapper<Shell, ShellHandler> Mapper =
				new PropertyMapper<Shell, ShellHandler>(ElementMapper)
				{
					[nameof(IToolbarElement.Toolbar)] = (handler, view) => ViewHandler.MapToolbar(handler, view),
					[nameof(IFlyoutView.Flyout)] = MapFlyout,
					[nameof(IFlyoutView.IsPresented)] = MapIsPresented,
					[nameof(IFlyoutView.FlyoutBehavior)] = MapFlyoutBehavior,
					[nameof(IFlyoutView.FlyoutWidth)] = MapFlyoutWidth,
					[nameof(Shell.FlyoutBackground)] = MapFlyoutBackground,
					[nameof(Shell.FlyoutBackgroundColor)] = MapFlyoutBackground,
					[nameof(Shell.FlyoutContent)] = MapFlyout,
					[nameof(Shell.CurrentItem)] = MapCurrentItem,
					[nameof(Shell.FlyoutBackdrop)] = MapFlyoutBackdrop,
					[nameof(Shell.FlyoutFooter)] = MapFlyoutFooter,
					[nameof(Shell.FlyoutFooterTemplate)] = MapFlyoutFooter,
					[nameof(Shell.FlyoutHeader)] = MapFlyoutHeader,
					[nameof(Shell.FlyoutHeaderTemplate)] = MapFlyoutHeader,
					[nameof(Shell.FlyoutHeaderBehavior)] = MapFlyoutHeaderBehavior,
					[nameof(Shell.Items)] = MapItems,
					[nameof(Shell.FlyoutItems)] = MapFlyoutItems,
#if WINDOWS
					[nameof(Shell.FlyoutIcon)] = MapFlyoutIcon,
					[nameof(Shell.FlyoutContentTemplate)] = MapFlyout,
					[nameof(Shell.FlowDirection)] = MapFlowDirection,
					[nameof(Shell.FlyoutBackgroundImage)] = MapFlyoutBackgroundImage,
					[nameof(Shell.FlyoutBackgroundImageAspect)] = MapFlyoutBackgroundImage,
#endif
				};

		public static CommandMapper<Shell, ShellHandler> CommandMapper =
				new CommandMapper<Shell, ShellHandler>(ElementCommandMapper);

		public ShellHandler() : base(Mapper, CommandMapper)
		{
		}
	}
}
#endif
