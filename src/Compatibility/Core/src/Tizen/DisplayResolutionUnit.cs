using System;
using TDisplayResolutionUnit = Tizen.UIExtensions.Common.DisplayResolutionUnit;

namespace Microsoft.Maui.Controls.Compatibility
{
	public class DisplayResolutionUnit
	{
		public static DisplayResolutionUnit Pixel(bool useDeviceScale = false)
		{
			return new DisplayResolutionUnit()
			{
				UseDP = false,
				UseDeviceScale = useDeviceScale
			};
		}

		public static DisplayResolutionUnit DP(bool useDeviceScale = false)
		{
			return new DisplayResolutionUnit()
			{
				UseDP = true,
				UseDeviceScale = useDeviceScale
			};
		}

		public static DisplayResolutionUnit VP(double width)
		{
			if (width <= 0)
				throw new ArgumentException("width must be bigger than 0", "width");

			return new DisplayResolutionUnit()
			{
				UseVP = true,
				UseDeviceScale = false,
				ViewportWidth = width
			};
		}

		internal static DisplayResolutionUnit FromInit(bool useDP)
		{
			return useDP ? DP() : Pixel();
		}

		DisplayResolutionUnit() { }

		public bool UseDP { get; private set; }

		public bool UseDeviceScale { get; private set; }

		public bool UseVP { get; private set; }

		public double ViewportWidth { get; private set; } = -1;
	}

	public static class DisplayResolutionUnitConverter
	{
		public static DisplayResolutionUnit ToCompatibility(this TDisplayResolutionUnit unit, double width = 0) => unit switch
		{
			TDisplayResolutionUnit.DP => DisplayResolutionUnit.DP(),
			TDisplayResolutionUnit.DeviceScaledDP => DisplayResolutionUnit.DP(true),
			TDisplayResolutionUnit.Pixel => DisplayResolutionUnit.Pixel(),
			TDisplayResolutionUnit.DeviceScaledPixel => DisplayResolutionUnit.Pixel(true),
			TDisplayResolutionUnit.VP => DisplayResolutionUnit.VP(width),
			_ => DisplayResolutionUnit.DP(),
		};

		public static TDisplayResolutionUnit ToDeviceInfo(this DisplayResolutionUnit unit)
		{
			if (unit.UseDP)
			{
				return unit.UseDeviceScale ? TDisplayResolutionUnit.DeviceScaledDP : TDisplayResolutionUnit.DP;
			}
			else if (unit.UseVP)
			{
				Tizen.UIExtensions.Common.DeviceInfo.ViewPortWidth = unit.ViewportWidth;
				return TDisplayResolutionUnit.VP;
			}
			else
			{
				return unit.UseDeviceScale ? TDisplayResolutionUnit.DeviceScaledPixel : TDisplayResolutionUnit.Pixel;
			}
		}
	}
}
