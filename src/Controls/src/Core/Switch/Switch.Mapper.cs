#nullable enable
using Microsoft.Maui.Handlers;
#if IOS || MACCATALYST
using Microsoft.Maui.Platform;
#endif

namespace Microsoft.Maui.Controls
{
	public partial class Switch
	{
		static Switch() => RemapForControls();

		private new static void RemapForControls()
		{
			VisualElement.RemapIfNeeded();

#if IOS || MACCATALYST
			// Prepended, so an OnColor mapping that an app adds before the first Switch is created still runs
			SwitchHandler.Mapper.PrependToMapping<Switch, ISwitchHandler>(nameof(OnColor), MapOnColor);
#endif
		}

#if IOS || MACCATALYST
		// ISwitch.TrackColor is OnColor only while the switch is on, so the on color is also set while it is off: a tap that turns the
		// switch on then doesn't change it while the switch animates (https://github.com/dotnet/maui/issues/34560)
		static void MapOnColor(ISwitchHandler handler, Switch view) =>
			handler.PlatformView?.UpdateOnTintColor(view.OnColor?.ToPlatform());
#endif
	}
}
