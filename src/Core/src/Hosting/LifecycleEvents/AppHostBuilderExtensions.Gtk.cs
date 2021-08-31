using Microsoft.Maui.Hosting;
using Gdk;

namespace Microsoft.Maui.LifecycleEvents
{

	public static partial class AppHostBuilderExtensions
	{

		internal static MauiAppBuilder ConfigureCrossPlatformLifecycleEvents(this MauiAppBuilder builder) =>
			builder.ConfigureLifecycleEvents(events => events.AddGtk(OnConfigureLifeCycle));

		[MissingMapper("Stopped(), Resumed()")]
		static void OnConfigureLifeCycle(IGtkLifecycleBuilder gtk)
		{
			gtk
			   .OnCreated((window, args) =>
				{
					window.GetWindow().Created();
				})
			   .OnShown((window, args) =>
				{
					window.GetWindow().Activated();
				})
			   .OnStateChanged((window, args) =>
				{

					// TODO: changedmask can be removed or added to newwindowstate
					switch (args.Event.ChangedMask)
					{
						case WindowState.Withdrawn:
							break;
						case WindowState.Iconified:
							break;
						case WindowState.Maximized:
							break;
						case WindowState.Sticky:
							break;
						case WindowState.Fullscreen:
							break;
						case WindowState.Above:
							break;
						case WindowState.Below:
							break;
						case WindowState.Focused:
							break;
						case WindowState.Tiled:
							break;
						default:
							break;
					}

					;
				})
			   .OnClosed((window, args) =>
				{
					window.GetWindow().Deactivated();
				})
			   .OnDelete((window, args) =>
				{
					window.GetWindow().Destroying();
				})
				;

		}

	}

}