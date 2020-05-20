using System;
namespace System.Maui.Platform
{
	public partial class SwitchRenderer
	{
		public static PropertyMapper<ISwitch> SwitchMapper = new PropertyMapper<ISwitch>(ViewRenderer.ViewMapper)
		{
			[nameof(ISwitch.IsOn)] = MapPropertyIsOn,
			[nameof(ISwitch.OnColor)] = MapPropertyOnColor,
			[nameof(ISwitch.ThumbColor)] = MapPropertyThumbColor
		};

		public SwitchRenderer() : base(SwitchMapper) { }

		public SwitchRenderer(PropertyMapper mapper) : base(mapper ?? SwitchMapper) { }

		public static void MapPropertyIsOn(IViewRenderer renderer, ISwitch @switch)
		{
			(renderer as SwitchRenderer)?.UpdateIsOn();
		}

		public static void MapPropertyOnColor(IViewRenderer renderer, ISwitch @switch)
		{
			(renderer as SwitchRenderer)?.UpdateOnColor();
		}

		public static void MapPropertyThumbColor(IViewRenderer renderer, ISwitch @switch)
		{
			(renderer as SwitchRenderer)?.UpdateThumbColor();
		}
	}
}
