namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualElement']/Docs" />
	public partial class VisualElement
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='ControlsVisualElementMapper']/Docs" />
		public static IPropertyMapper<IView, IViewHandler> ControlsVisualElementMapper = new PropertyMapper<View, IViewHandler>(Element.ControlsElementMapper)
		{
#if WINDOWS
			[nameof(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyHorizontalOffsetProperty.PropertyName)] = MapAccessKeyHorizontalOffset,
			[nameof(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyPlacementProperty.PropertyName)] = MapAccessKeyPlacement,
			[nameof(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyProperty.PropertyName)] = MapAccessKey,
			[nameof(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyVerticalOffsetProperty.PropertyName)] = MapAccessKeyVerticalOffset,
#endif
			[nameof(BackgroundColor)] = MapBackgroundColor,
		};

		internal static void RemapForControls()
		{
			ViewHandler.ViewMapper = ControlsVisualElementMapper;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='MapBackgroundColor']/Docs" />
		public static void MapBackgroundColor(IViewHandler handler, View view)
		{
			handler.UpdateValue(nameof(Background));
		}
	}
}
