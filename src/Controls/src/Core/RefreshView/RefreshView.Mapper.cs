#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class RefreshView
	{
		[Obsolete("Use RefreshViewHandler.Mapper instead.")]
		public static IPropertyMapper<IRefreshView, RefreshViewHandler> ControlsRefreshViewMapper = new ControlsMapper<RefreshView, RefreshViewHandler>(RefreshViewHandler.Mapper);

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.RefreshView legacy behaviors
#if WINDOWS
			RefreshViewHandler.Mapper.ReplaceMapping<RefreshView, IRefreshViewHandler>(PlatformConfiguration.WindowsSpecific.RefreshView.RefreshPullDirectionProperty.PropertyName, MapRefreshPullDirection);
#endif
		}
	}
}