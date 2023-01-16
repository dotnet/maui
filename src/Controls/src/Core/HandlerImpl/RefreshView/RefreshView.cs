#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class RefreshView
	{
		public static IPropertyMapper<IRefreshView, RefreshViewHandler> ControlsRefreshViewMapper = new PropertyMapper<RefreshView, RefreshViewHandler>(RefreshViewHandler.Mapper)
		{
#if WINDOWS
			[PlatformConfiguration.WindowsSpecific.RefreshView.RefreshPullDirectionProperty.PropertyName] = MapRefreshPullDirection,
#endif
		};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.RefreshView legacy behaviors
			RefreshViewHandler.Mapper = ControlsRefreshViewMapper;
		}
	}
}