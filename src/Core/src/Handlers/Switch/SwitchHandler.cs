#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UISwitch;
#elif MONOANDROID
using PlatformView = AndroidX.AppCompat.Widget.SwitchCompat;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.ToggleSwitch;
#elif TIZEN
using PlatformView = ElmSharp.Check;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ISwitchHandler
	{
		public static IPropertyMapper<ISwitch, ISwitchHandler> Mapper = new PropertyMapper<ISwitch, ISwitchHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISwitch.IsOn)] = MapIsOn,
			[nameof(ISwitch.ThumbColor)] = MapThumbColor,
			[nameof(ISwitch.TrackColor)] = MapTrackColor,
		};

		public static CommandMapper<ISwitch, ISwitchHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public SwitchHandler() : base(Mapper)
		{
		}

		public SwitchHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{
		}

		ISwitch ISwitchHandler.VirtualView => VirtualView;

		PlatformView ISwitchHandler.PlatformView => PlatformView;
	}
}