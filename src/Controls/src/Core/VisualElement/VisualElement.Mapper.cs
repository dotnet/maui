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
		static VisualElement() => RemapIfNeeded();

		internal static new void RemapIfNeeded()
		{
			RemappingHelper.RemapIfNeeded(typeof(VisualElement), RemapForControls);
		}

		internal static new void RemapForControls()
		{
			RemapForControls(ViewHandler.ViewMapper, ViewHandler.ViewCommandMapper);
		}

		internal static void RemapForControls(
			IPropertyMapper<IView, IViewHandler> viewMapper,
			CommandMapper<IView, IViewHandler> commandMapper)
		{
			Element.RemapIfNeeded();

#if WINDOWS
			viewMapper.ReplaceMapping<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyHorizontalOffsetProperty.PropertyName, MapAccessKeyHorizontalOffset);
			viewMapper.ReplaceMapping<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyPlacementProperty.PropertyName, MapAccessKeyPlacement);
			viewMapper.ReplaceMapping<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyProperty.PropertyName, MapAccessKey);
			viewMapper.ReplaceMapping<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyVerticalOffsetProperty.PropertyName, MapAccessKeyVerticalOffset);
#endif
			viewMapper.ReplaceMapping<IView, IViewHandler>(nameof(BackgroundColor), MapBackgroundColorForControls);
			viewMapper.ReplaceMapping<IView, IViewHandler>(nameof(Page.BackgroundImageSource), MapBackgroundImageSourceForControls);
			viewMapper.ReplaceMapping<IView, IViewHandler>(SemanticProperties.DescriptionProperty.PropertyName, MapSemanticPropertiesDescriptionProperty);
			viewMapper.ReplaceMapping<IView, IViewHandler>(SemanticProperties.HintProperty.PropertyName, MapSemanticPropertiesHintProperty);
			viewMapper.ReplaceMapping<IView, IViewHandler>(SemanticProperties.HeadingLevelProperty.PropertyName, MapSemanticPropertiesHeadingLevelProperty);

			viewMapper.AppendToMapping<VisualElement, IViewHandler>(nameof(IViewHandler.ContainerView), MapContainerView);

			commandMapper.ModifyMapping<VisualElement, IViewHandler>(nameof(IView.Focus), MapFocus);
		}

		public static void MapBackgroundColor(IViewHandler handler, IView view) =>
			handler.UpdateValue(nameof(Background));

		public static void MapBackgroundImageSource(IViewHandler handler, IView view) =>
			handler.UpdateValue(nameof(Background));

		// `nameof(BackgroundColor)`/`nameof(Page.BackgroundImageSource)` are appended (by RemapForControls) after
		// the Core `nameof(IView.Background)` key in `ViewHandler.ViewMapper`, so during the single bulk
		// `UpdateProperties` pass that runs on initial connect (and on handler reuse/reconnect - see
		// `IsMappingProperties`) the canonical `Background` mapping already reads the current composite
		// BackgroundColor/BackgroundImageSource/Background value later in the very same pass, making this
		// redirect redundant. We skip it only while that bulk pass is in flight, so any *dynamic* update that
		// happens once the handler is fully connected still runs through `MapBackgroundColor`/
		// `MapBackgroundImageSource` normally. This relies on `Background` never being reordered ahead of these
		// keys and on no handler mutating `BackgroundColor`/`BackgroundImageSource` from within its own
		// `ConnectHandler` or from a mapper that runs later in the same pass; both invariants are covered by
		// VisualElementMapperTests.
		static void MapBackgroundColorForControls(IViewHandler handler, IView view)
		{
			if (!handler.IsMappingProperties())
			{
				MapBackgroundColor(handler, view);
			}
		}

		static void MapBackgroundImageSourceForControls(IViewHandler handler, IView view)
		{
			if (!handler.IsMappingProperties())
			{
				MapBackgroundImageSource(handler, view);
			}
		}

		static void MapSemanticPropertiesHeadingLevelProperty(IViewHandler handler, IView element) =>
			UpdateSemanticsFromMapper(handler, element);

		static void MapSemanticPropertiesHintProperty(IViewHandler handler, IView element) =>
			UpdateSemanticsFromMapper(handler, element);

		static void MapSemanticPropertiesDescriptionProperty(IViewHandler handler, IView element) =>
			UpdateSemanticsFromMapper(handler, element);

		// Same reasoning as the background guard above: the three `SemanticProperties` keys are appended
		// after the Core `nameof(IView.Semantics)` key, so `MapSemantics` already reads the current composite
		// Semantics value later in the same bulk `UpdateProperties` pass (initial connect or reconnect). We use
		// `IsMappingProperties` (rather than `IsConnectingHandler`) so the same de-duplication also applies to
		// handler reuse (e.g. CollectionView/CarouselView cell recycling), not only to the very first connect.
		static void UpdateSemanticsFromMapper(IViewHandler handler, IView element)
		{
			if (!handler.IsMappingProperties())
			{
				(element as VisualElement)?.UpdateSemanticsFromMapper();
			}
		}

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
