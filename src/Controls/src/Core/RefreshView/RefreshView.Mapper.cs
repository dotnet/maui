#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class RefreshView
	{
		static RefreshView()
		{
			// Register dependency: Command depends on CommandParameter for CanExecute evaluation
			// See https://github.com/dotnet/maui/issues/31939
			CommandProperty.DependsOn(CommandParameterProperty);
		}

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.RefreshView legacy behaviors
#if WINDOWS
			RefreshViewHandler.Mapper.ReplaceMapping<RefreshView, IRefreshViewHandler>(PlatformConfiguration.WindowsSpecific.RefreshView.RefreshPullDirectionProperty.PropertyName, MapRefreshPullDirection);
#endif
		}
	}
}