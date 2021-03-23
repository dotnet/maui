namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler
	{
		public static PropertyMapper<ISwitch, SwitchHandler> SwitchMapper = new PropertyMapper<ISwitch, SwitchHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISwitch.IsToggled)] = MapIsToggled,
			[nameof(ISwitch.TrackColor)] = MapTrackColor,
			[nameof(ISwitch.ThumbColor)] = MapThumbColor
		};

		public SwitchHandler() : base(SwitchMapper)
		{

		}

		public SwitchHandler(PropertyMapper? mapper = null) : base(mapper ?? SwitchMapper)
		{

		}
	}
}