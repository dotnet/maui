#nullable disable
using System;
using System.Threading;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class ScrollView
	{
		static int s_remappedForControls;

		internal override void RemapForControls()
		{
			if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
				return;

			base.RemapForControls();

				// Adjust the mappings to preserve Controls.ScrollView legacy behaviors
#if IOS
			ScrollViewHandler.Mapper.ReplaceMapping<ScrollView, IScrollViewHandler>(PlatformConfiguration.iOSSpecific.ScrollView.ShouldDelayContentTouchesProperty.PropertyName, MapShouldDelayContentTouches);
#endif
		}
	}
}
