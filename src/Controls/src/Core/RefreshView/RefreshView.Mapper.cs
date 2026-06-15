#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class RefreshView
	{
		internal override void RemapForControls(HashSet<Type> remapped)
		{
			if (remapped.Add(typeof(RefreshView)))
			{
				base.RemapForControls(remapped);

				// Adjust the mappings to preserve Controls.RefreshView legacy behaviors
#if WINDOWS
				RefreshViewHandler.Mapper.ReplaceMapping<RefreshView, IRefreshViewHandler>(PlatformConfiguration.WindowsSpecific.RefreshView.RefreshPullDirectionProperty.PropertyName, MapRefreshPullDirection);
#endif
			}
		}
	}
}