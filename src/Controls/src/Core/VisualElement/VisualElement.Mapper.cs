#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides the base class for all visual elements in .NET MAUI.
	/// </summary>
	public partial class VisualElement
	{
		// Snapshots of the values last observed by RecordBackgroundMapped/RecordSemanticsMapped - see their
		// doc comments below for how these are used to make the BackgroundColor/BackgroundImageSource/
		// SemanticProperties.* mapper redirects skip only genuine no-ops.
		Color _lastMappedBackgroundColor;
		ImageSource _lastMappedBackgroundImageSource;
		string _lastMappedSemanticDescription;
		string _lastMappedSemanticHint;
		SemanticHeadingLevel _lastMappedSemanticHeadingLevel;

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

			// Record, on the element itself, the exact BackgroundColor/BackgroundImageSource and
			// SemanticProperties.* values that were current the last time the canonical `Background`/
			// `Semantics` mapping actually ran (through any path - initial connect, reconnect, or an
			// explicit `UpdateValue` call). The redirects above compare against these snapshots so they
			// only skip an invocation that is a genuine no-op, never one that would apply a value that
			// hasn't been pushed yet. See VisualElementMapperTests for coverage of that invariant.
			viewMapper.AppendToMapping<VisualElement, IViewHandler>(nameof(IView.Background), RecordBackgroundMapped);
			viewMapper.AppendToMapping<VisualElement, IViewHandler>(nameof(IView.Semantics), RecordSemanticsMapped);

			viewMapper.AppendToMapping<VisualElement, IViewHandler>(nameof(IViewHandler.ContainerView), MapContainerView);

			commandMapper.ModifyMapping<VisualElement, IViewHandler>(nameof(IView.Focus), MapFocus);
		}

		public static void MapBackgroundColor(IViewHandler handler, IView view) =>
			handler.UpdateValue(nameof(Background));

		public static void MapBackgroundImageSource(IViewHandler handler, IView view) =>
			handler.UpdateValue(nameof(Background));

		// `nameof(BackgroundColor)`/`nameof(Page.BackgroundImageSource)` are appended (by RemapForControls) after
		// the Core `nameof(IView.Background)` key in `ViewHandler.ViewMapper`, so whenever the canonical
		// `Background` mapping has *already* run with the current BackgroundColor/BackgroundImageSource value
		// (recorded by `RecordBackgroundMapped`), re-running it here would be a pure no-op. We only skip in
		// that exact case - comparing the live property value against the snapshot taken the last time
		// `Background` was actually mapped - so this is safe regardless of connect/reconnect/dynamic-update
		// timing: any call (including an explicit `UpdateValue` from `ConnectHandler` or from another mapper
		// that runs later in the same pass) whose value hasn't been applied yet still goes through normally.
		// See VisualElementMapperTests for coverage of this invariant.
		static void MapBackgroundColorForControls(IViewHandler handler, IView view)
		{
			if (view is not VisualElement element || element.BackgroundColor != element._lastMappedBackgroundColor)
			{
				MapBackgroundColor(handler, view);
			}
		}

		// `BackgroundImageSource` is only ever a real, settable property on `Page`, but this key is registered
		// on the shared `ViewHandler.ViewMapper` used by every view type, so this redirect is invoked for
		// non-`Page` views too. For those, there is nothing to (re-)apply, so we never invoke the redirect;
		// for `Page`, apply only when the value differs from the snapshot taken the last time `Background`
		// was actually mapped, for the same reason as `MapBackgroundColorForControls` above.
		static void MapBackgroundImageSourceForControls(IViewHandler handler, IView view)
		{
			if (view is Page page && page.BackgroundImageSource != page._lastMappedBackgroundImageSource)
			{
				MapBackgroundImageSource(handler, view);
			}
		}

		// Snapshots the BackgroundColor/BackgroundImageSource values that were just used to compute the
		// composite `Background` that was mapped, so `MapBackgroundColorForControls`/
		// `MapBackgroundImageSourceForControls` can tell a genuine no-op apart from a value that has changed
		// since. Appended to the `Background` key itself, this runs every time that key's mapping executes,
		// through any path (bulk connect/reconnect pass or an explicit `UpdateValue(nameof(Background))`).
		static void RecordBackgroundMapped(IViewHandler handler, VisualElement element)
		{
			element._lastMappedBackgroundColor = element.BackgroundColor;

			if (element is Page page)
			{
				page._lastMappedBackgroundImageSource = page.BackgroundImageSource;
			}
		}

		static void MapSemanticPropertiesHeadingLevelProperty(IViewHandler handler, IView element) =>
			UpdateSemanticsFromMapper(handler, element);

		static void MapSemanticPropertiesHintProperty(IViewHandler handler, IView element) =>
			UpdateSemanticsFromMapper(handler, element);

		static void MapSemanticPropertiesDescriptionProperty(IViewHandler handler, IView element) =>
			UpdateSemanticsFromMapper(handler, element);

		// Same reasoning as the background guard above: the three `SemanticProperties` keys are appended
		// after the Core `nameof(IView.Semantics)` key, so we compare the live SemanticProperties.* values
		// against the snapshot taken the last time `Semantics` was actually mapped (recorded by
		// `RecordSemanticsMapped`) and only skip when nothing has changed since. Any explicit `UpdateValue`
		// call - from `ConnectHandler`, from a later mapper in the same pass, or from a dynamic property
		// change - whose value hasn't been applied yet still runs normally.
		static void UpdateSemanticsFromMapper(IViewHandler handler, IView element)
		{
			if (element is not VisualElement ve ||
				SemanticProperties.GetDescription(ve) != ve._lastMappedSemanticDescription ||
				SemanticProperties.GetHint(ve) != ve._lastMappedSemanticHint ||
				SemanticProperties.GetHeadingLevel(ve) != ve._lastMappedSemanticHeadingLevel)
			{
				(element as VisualElement)?.UpdateSemanticsFromMapper();
			}
		}

		// See RecordBackgroundMapped: runs every time the `Semantics` key's mapping executes, through any
		// path, so the SemanticProperties.* redirects above can tell a genuine no-op apart from a value that
		// has changed since.
		static void RecordSemanticsMapped(IViewHandler handler, VisualElement element)
		{
			element._lastMappedSemanticDescription = SemanticProperties.GetDescription(element);
			element._lastMappedSemanticHint = SemanticProperties.GetHint(element);
			element._lastMappedSemanticHeadingLevel = SemanticProperties.GetHeadingLevel(element);
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
