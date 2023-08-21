//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal static class AlignmentExtensions
	{
		[PortHandler]
		internal static Microsoft.UI.Xaml.TextAlignment ToPlatformTextAlignment(this TextAlignment alignment, EffectiveFlowDirection flowDirection = default(EffectiveFlowDirection))
		{
			var isLtr = flowDirection.IsLeftToRight();
			switch (alignment)
			{
				case TextAlignment.Center:
					return Microsoft.UI.Xaml.TextAlignment.Center;
				case TextAlignment.End:
					if (isLtr)
						return Microsoft.UI.Xaml.TextAlignment.Right;
					else
						return Microsoft.UI.Xaml.TextAlignment.Left;
				default:
					if (isLtr)
						return Microsoft.UI.Xaml.TextAlignment.Left;
					else
						return Microsoft.UI.Xaml.TextAlignment.Right;
			}
		}
		[PortHandler]
		internal static VerticalAlignment ToPlatformVerticalAlignment(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return VerticalAlignment.Center;
				case TextAlignment.End:
					return VerticalAlignment.Bottom;
				default:
					return VerticalAlignment.Top;
			}
		}

		[PortHandler]
		internal static HorizontalAlignment ToPlatformHorizontalAlignment(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return HorizontalAlignment.Center;
				case TextAlignment.End:
					return HorizontalAlignment.Right;
				default:
					return HorizontalAlignment.Left;
			}
		}
	}
}