// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class RefreshView
	{
		[Obsolete("Use RefreshViewHandler.Mapper instead.")]
		public static IPropertyMapper<IRefreshView, RefreshViewHandler> ControlsRefreshViewMapper = new PropertyMapper<RefreshView, RefreshViewHandler>(RefreshViewHandler.Mapper);

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.RefreshView legacy behaviors
#if WINDOWS
			RefreshViewHandler.Mapper.ReplaceMapping<RefreshView, IRefreshViewHandler>(PlatformConfiguration.WindowsSpecific.RefreshView.RefreshPullDirectionProperty.PropertyName, MapRefreshPullDirection);
#endif
		}
	}
}