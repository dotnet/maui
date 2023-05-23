using System;
namespace Microsoft.Maui.Controls;

public partial class Page
{
	public static IPropertyMapper<IContentView, IPageHandler> ControlsPageMapper =
			new PropertyMapper<IContentView, IPageHandler>(ContentViewHandler.Mapper)
			{
#if IOS
				[PlatformConfiguration.iOSSpecific.Page.SafeAreaInsetsProperty.PropertyName] = MapUpdateSafeAreaInsets,
#endif
			};

	internal static new void RemapForControls()
	{
		// Adjust the mappings to preserve Controls.Page legacy behaviors
		PageHandler.Mapper = ControlsPageMapper;
	}
}
