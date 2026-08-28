#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides the base class for all visual elements in .NET MAUI.
	/// </summary>
	public partial class VisualElement
	{
		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
#if WINDOWS
			ViewHandler.ViewMapper.ReplaceMappingForControls<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyHorizontalOffsetProperty.PropertyName, MapAccessKeyHorizontalOffset);
			ViewHandler.ViewMapper.ReplaceMappingForControls<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyPlacementProperty.PropertyName, MapAccessKeyPlacement);
			ViewHandler.ViewMapper.ReplaceMappingForControls<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyProperty.PropertyName, MapAccessKey);
			ViewHandler.ViewMapper.ReplaceMappingForControls<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyVerticalOffsetProperty.PropertyName, MapAccessKeyVerticalOffset);
#endif
			ViewHandler.ViewMapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(BackgroundColor), MapBackgroundColor);
			ViewHandler.ViewMapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(Page.BackgroundImageSource), MapBackgroundImageSource);
			ViewHandler.ViewMapper.ReplaceMappingForControls<IView, IViewHandler>(SemanticProperties.DescriptionProperty.PropertyName, MapSemanticPropertiesDescriptionProperty);
			ViewHandler.ViewMapper.ReplaceMappingForControls<IView, IViewHandler>(SemanticProperties.HintProperty.PropertyName, MapSemanticPropertiesHintProperty);
			ViewHandler.ViewMapper.ReplaceMappingForControls<IView, IViewHandler>(SemanticProperties.HeadingLevelProperty.PropertyName, MapSemanticPropertiesHeadingLevelProperty);

			ViewHandler.ViewMapper.AppendToMappingForControls<VisualElement, IViewHandler>(nameof(IViewHandler.ContainerView), MapContainerView);

			ViewHandler.ViewCommandMapper.ModifyMappingForControls<VisualElement, IViewHandler>(nameof(IView.Focus), MapFocus);
		}

		public static void MapBackgroundColor(IViewHandler handler, IView view) =>
			handler.UpdateValue(nameof(Background));

		public static void MapBackgroundImageSource(IViewHandler handler, IView view) =>
			handler.UpdateValue(nameof(Background));

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

		static void MapContainerView(IViewHandler handler, VisualElement element) =>
			element._platformContainerViewChanged?.Invoke(element, EventArgs.Empty);

		static void MapFocus(IViewHandler handler, VisualElement view, object args, Action<IElementHandler, IElement, object> baseMethod)
		{
			if (args is not FocusRequest fr || view is not VisualElement element)
				return;

			view.MapFocus(fr, baseMethod is null ? null : () => baseMethod?.Invoke(handler, view, args));
		}
	}
}
