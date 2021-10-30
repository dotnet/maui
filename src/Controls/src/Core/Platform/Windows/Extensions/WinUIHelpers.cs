using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WThickness = Microsoft.UI.Xaml.Thickness;
using WCornerRadius = Microsoft.UI.Xaml.CornerRadius;
using WGridLength = Microsoft.UI.Xaml.GridLength;
using UwpGridUnitType = Microsoft.UI.Xaml.GridUnitType;

namespace Microsoft.Maui.Controls.Platform
{
	static class WinUIHelpers
	{
		public static WThickness CreateThickness(double left, double top, double right, double bottom)
		{
			return new WThickness
			{
				Left = left,
				Top = top,
				Right = right,
				Bottom = bottom
			};
		}

		public static WThickness CreateThickness(double all)
		{
			return new WThickness
			{
				Left = all,
				Top = all,
				Right = all,
				Bottom = all
			};
		}
		public static WCornerRadius CreateCornerRadius(double left, double top, double right, double bottom)
		{
			return new WCornerRadius
			{
				TopLeft = left,
				TopRight = top,
				BottomRight = right,
				BottomLeft = bottom
			};
		}

		public static WCornerRadius CreateCornerRadius(double all)
		{
			return new WCornerRadius
			{
				TopLeft = all,
				TopRight = all,
				BottomRight = all,
				BottomLeft = all
			};
		}

		public static WGridLength CreateGridLength(int v, UwpGridUnitType auto)
		{
			return new WGridLength(v, auto);
		}
	}
}
