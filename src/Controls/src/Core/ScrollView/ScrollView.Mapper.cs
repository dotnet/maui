// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class ScrollView
	{
		[Obsolete("Use ScrollViewHandler.Mapper instead.")]
		public static IPropertyMapper<IScrollView, ScrollViewHandler> ControlsScrollViewMapper =
				new PropertyMapper<ScrollView, ScrollViewHandler>(ScrollViewHandler.Mapper);

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.ScrollView legacy behaviors
#if IOS
			ScrollViewHandler.Mapper.ReplaceMapping<ScrollView, IScrollViewHandler>(PlatformConfiguration.iOSSpecific.ScrollView.ShouldDelayContentTouchesProperty.PropertyName, MapShouldDelayContentTouches);
#endif
		}
	}
}
