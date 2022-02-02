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
#if WINDOWS
					[nameof(IToolbarElement.Toolbar)] = (handler, view) => ViewHandler.MapToolbar(handler, view),
					[nameof(IFlyoutView.Flyout)] = MapFlyout
					[nameof(IFlyoutView.IsPresented)] = MapIsPresented,
					[nameof(IFlyoutView.FlyoutBehavior)] = MapFlyoutBehavior,
					[nameof(IFlyoutView.FlyoutWidth)] = MapFlyoutWidth,
					[nameof(Shell.FlyoutBackground)] = MapFlyoutBackground,
					[nameof(Shell.FlyoutBackgroundColor)] = MapFlyoutBackground,
#endif
				};

		public static CommandMapper<Shell, ShellHandler> CommandMapper =
				new CommandMapper<Shell, ShellHandler>(ElementCommandMapper);

		public ShellHandler() : base(Mapper, CommandMapper)
		{
		}
	}
}
