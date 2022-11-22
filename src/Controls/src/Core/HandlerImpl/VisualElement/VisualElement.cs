using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualElement']/Docs/*" />
	public partial class VisualElement
	{
		public static IPropertyMapper<IView, IViewHandler> ControlsVisualElementMapper =
			new PropertyMapper<IView, IViewHandler>(Element.ControlsElementMapper)
			{
#if WINDOWS
				[PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyHorizontalOffsetProperty.PropertyName] = MapAccessKeyHorizontalOffset,
				[PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyPlacementProperty.PropertyName] = MapAccessKeyPlacement,
				[PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyProperty.PropertyName] = MapAccessKey,
				[PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyVerticalOffsetProperty.PropertyName] = MapAccessKeyVerticalOffset,
#endif
				[nameof(BackgroundColor)] = MapBackgroundColor,
				[nameof(Page.BackgroundImageSource)] = MapBackgroundImageSource,
				[SemanticProperties.DescriptionProperty.PropertyName] = MapSemanticPropertiesDescriptionProperty,
				[SemanticProperties.HintProperty.PropertyName] = MapSemanticPropertiesHintProperty,
				[SemanticProperties.HeadingLevelProperty.PropertyName] = MapSemanticPropertiesHeadingLevelProperty,
			};

		internal static void RemapForControls()
		{
			ViewHandler.ViewMapper = ControlsVisualElementMapper;
		}

		public static void MapBackgroundColor(IViewHandler handler, IView view)
		{
			handler.UpdateValue(nameof(Background));
		}

		public static void MapBackgroundImageSource(IViewHandler handler, IView view)
		{
			handler.UpdateValue(nameof(Background));
		}

		static void MapSemanticPropertiesHeadingLevelProperty(IViewHandler handler, IView element) =>
			(element as VisualElement)?.UpdateSemanticsFromMapper();

		static void MapSemanticPropertiesHintProperty(IViewHandler handler, IView element) =>
			(element as VisualElement)?.UpdateSemanticsFromMapper();

		static void MapSemanticPropertiesDescriptionProperty(IViewHandler handler, IView element) =>
			(element as VisualElement)?.UpdateSemanticsFromMapper();

		void UpdateSemanticsFromMapper()
		{
			UpdateSemantics();
			Handler?.UpdateValue(nameof(IView.Semantics));
		}
	}
}
