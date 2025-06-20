#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class RefreshView
	{
		static RefreshView() => RemapForControls();

		private static new void RemapForControls()
		{
			VisualElement.RemapIfNeeded();

			// Adjust the mappings to preserve Controls.RefreshView legacy behaviors
#if WINDOWS
			RefreshViewHandler.Mapper.ReplaceMapping<RefreshView, IRefreshViewHandler>(PlatformConfiguration.WindowsSpecific.RefreshView.RefreshPullDirectionProperty.PropertyName, MapRefreshPullDirection);
#endif
		}
	}
}