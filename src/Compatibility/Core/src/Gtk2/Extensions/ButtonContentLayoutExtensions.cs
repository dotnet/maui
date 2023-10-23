using System;
using Gtk;
using static Microsoft.Maui.Controls.Compatibility.Button;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Extensions
{
	public static class ButtonContentLayoutExtensions
	{
		public static PositionType AsPositionType(this ButtonContentLayout.ImagePosition position)
		{
			switch (position)
			{
				case ButtonContentLayout.ImagePosition.Bottom:
					return PositionType.Bottom;
				case ButtonContentLayout.ImagePosition.Left:
					return PositionType.Left;
				case ButtonContentLayout.ImagePosition.Right:
					return PositionType.Right;
				case ButtonContentLayout.ImagePosition.Top:
					return PositionType.Top;
				default:
					throw new ArgumentOutOfRangeException(nameof(position));
			}
		}
	}
}
