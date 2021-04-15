#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler
	{
		public static PropertyMapper<ISwitch, SwitchHandler> SwitchMapper = new PropertyMapper<ISwitch, SwitchHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISwitch.IsOn)] = MapIsOn,
			[nameof(ISwitch.ThumbColor)] = MapThumbColor,
			[nameof(ISwitch.TrackColor)] = MapTrackColor,
		};

		public SwitchHandler() : base(SwitchMapper)
		{

		}

		public SwitchHandler(PropertyMapper? mapper = null) : base(mapper ?? SwitchMapper)
		{

		}
	}
}