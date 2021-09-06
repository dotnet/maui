#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler
	{
		public static IPropertyMapper<ISwitch, SwitchHandler> SwitchMapper = new PropertyMapper<ISwitch, SwitchHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISwitch.IsOn)] = MapIsOn,
			[nameof(ISwitch.ThumbColor)] = MapThumbColor,
			[nameof(ISwitch.TrackColor)] = MapTrackColor,
		};

		public SwitchHandler() : base(SwitchMapper)
		{

		}

		public SwitchHandler(IPropertyMapper? mapper = null) : base(mapper ?? SwitchMapper)
		{

		}
	}
}