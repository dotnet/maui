using Google.Android.Material.MaterialSwitch;

namespace Microsoft.Maui.Handlers;

// TODO: material3 - make it public in .net 11
internal class SwitchHandler2 : SwitchHandler
{
	public static new PropertyMapper<ISwitch, SwitchHandler2> Mapper =
  new(ViewMapper)
  {
	  [nameof(ISwitch.IsOn)] = MapIsOn,
	  [nameof(ISwitch.TrackColor)] = MapTrackColor,
	  [nameof(ISwitch.ThumbColor)] = MapThumbColor,
  };

	public static new CommandMapper<ISwitch, SwitchHandler2> CommandMapper =
	 new(ViewCommandMapper);

	public SwitchHandler2() : base(Mapper, CommandMapper)
	{
	}

	protected override MaterialSwitch CreatePlatformView()
	{
		return new MaterialSwitch(MauiMaterialContextThemeWrapper.Create(Context));
	}

	public static new void MapIsOn(ISwitchHandler handler, ISwitch view)
	{
		(handler.PlatformView as MaterialSwitch)?.UpdateIsOn(view);
	}

	public static new void MapTrackColor(ISwitchHandler handler, ISwitch view)
	{
		(handler.PlatformView as MaterialSwitch)?.UpdateTrackColor(view);
	}

	public static new void MapThumbColor(ISwitchHandler handler, ISwitch view)
	{
		(handler.PlatformView as MaterialSwitch)?.UpdateThumbColor(view);
	}
}