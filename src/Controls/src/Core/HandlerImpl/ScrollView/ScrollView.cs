#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class ScrollView
	{
		public static IPropertyMapper<IScrollView, ScrollViewHandler> ControlsScrollViewMapper =
				new PropertyMapper<ScrollView, ScrollViewHandler>(ScrollViewHandler.Mapper)
				{
#if IOS
					[PlatformConfiguration.iOSSpecific.ScrollView.ShouldDelayContentTouchesProperty.PropertyName] = MapShouldDelayContentTouches,
#endif
				};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.ScrollView legacy behaviors
			ScrollViewHandler.Mapper = ControlsScrollViewMapper;
		}
	}
}
