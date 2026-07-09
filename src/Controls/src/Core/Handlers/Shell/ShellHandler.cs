#nullable disable
#if WINDOWS || TIZEN || ANDROID
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
#if WINDOWS || TIZEN
     [nameof(IToolbarElement.Toolbar)] = (handler, view) => ViewHandler.MapToolbar(handler, view),
     [nameof(IFlyoutView.Flyout)] = MapFlyout,
#endif
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
#if WINDOWS || TIZEN
     [nameof(Shell.Items)] = MapItems,
     [nameof(Shell.FlyoutItems)] = MapFlyoutItems,
     [nameof(Shell.FlyoutIcon)] = MapFlyoutIcon,
#endif
			  [nameof(Shell.FlyoutContentTemplate)] = MapFlyout,
			  [nameof(Shell.FlowDirection)] = MapFlowDirection,
			  [nameof(Shell.FlyoutBackgroundImage)] = MapFlyoutBackgroundImage,
			  [nameof(Shell.FlyoutBackgroundImageAspect)] = MapFlyoutBackgroundImage,
			  [nameof(Shell.FlyoutVerticalScrollMode)] = MapFlyoutVerticalScrollMode,
#if ANDROID
			  [nameof(Shell.FlyoutHeight)] = MapFlyoutHeight,
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
