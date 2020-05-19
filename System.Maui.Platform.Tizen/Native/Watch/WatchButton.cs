using System;
using ElmSharp;
using System.Maui.PlatformConfiguration.TizenSpecific;
using ESize = ElmSharp.Size;

namespace System.Maui.Platform.Tizen.Native.Watch
{
	public class WatchButton : Button, IMeasurable
	{
		public WatchButton(EvasObject parent) : base(parent)
		{
		}

		public override ESize Measure(int availableWidth, int availableHeight)
		{
			if (Style == ButtonStyle.Default)
			{
				//Should gurantee the finger size (40)
				MinimumWidth = MinimumWidth < 40 ? 40 : MinimumWidth;
				if (Image != null)
					MinimumWidth += Image.Geometry.Width;
				var rawSize = TextHelper.GetRawTextBlockSize(this);
				return new ESize(rawSize.Width + MinimumWidth, Math.Max(MinimumHeight, rawSize.Height));
			}
			else
			{
				return new ESize(MinimumWidth, MinimumHeight);
			}
		}
	}
}