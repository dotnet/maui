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
		// Accessing this field from a derived type's static constructor forces
		// VisualElement's static constructor (and transitively Element's) to run
		// first, guaranteeing that base-level mapper remappings are applied before
		// derived ones.
		private protected static new bool s_forceStaticConstructor;

		static VisualElement()
		{
			// Force Element's static constructor to run first so its mapper
			// remappings are applied before VisualElement's.
#if DEBUG
			RemappingDebugHelper.AssertBaseClassForRemapping(typeof(VisualElement), typeof(Element));
#endif
			Element.s_forceStaticConstructor = true;

#if WINDOWS
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyHorizontalOffsetProperty.PropertyName, MapAccessKeyHorizontalOffset);
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyPlacementProperty.PropertyName, MapAccessKeyPlacement);
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyProperty.PropertyName, MapAccessKey);
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyVerticalOffsetProperty.PropertyName, MapAccessKeyVerticalOffset);
#endif
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(nameof(BackgroundColor), MapBackgroundColor);
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(nameof(Page.BackgroundImageSource), MapBackgroundImageSource);
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(SemanticProperties.DescriptionProperty.PropertyName, MapSemanticPropertiesDescriptionProperty);
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(SemanticProperties.HintProperty.PropertyName, MapSemanticPropertiesHintProperty);
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(SemanticProperties.HeadingLevelProperty.PropertyName, MapSemanticPropertiesHeadingLevelProperty);

			ViewHandler.ViewMapper.AppendToMapping<VisualElement, IViewHandler>(nameof(IViewHandler.ContainerView), MapContainerView);

			ViewHandler.ViewCommandMapper.ModifyMapping<VisualElement, IViewHandler>(nameof(IView.Focus), MapFocus);
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
