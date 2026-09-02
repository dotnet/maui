#nullable disable
using System;
using System.Runtime.CompilerServices;
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

			// Record, on the handler, that the canonical `Background`/`Semantics` mapping has just run so
			// the redirects below can skip only the single, immediately-following redundant invocation of
			// each key within *this same* bulk mapping pass - never an explicit/reentrant call, a
			// same-value call, or one that happens before any `Background`/`Semantics` mapping has run at
			// all. See VisualElementMapperTests for coverage of this invariant.
			viewMapper.AppendToMapping<IView, IViewHandler>(nameof(IView.Background), RecordBackgroundMapped);
			viewMapper.AppendToMapping<IView, IViewHandler>(nameof(IView.Semantics), RecordSemanticsMapped);

			viewMapper.AppendToMapping<VisualElement, IViewHandler>(nameof(IViewHandler.ContainerView), MapContainerView);

			commandMapper.ModifyMapping<VisualElement, IViewHandler>(nameof(IView.Focus), MapFocus);
		}

		public static void MapBackgroundColor(IViewHandler handler, IView view) =>
			handler.UpdateValue(nameof(Background));

		public static void MapBackgroundImageSource(IViewHandler handler, IView view) =>
			handler.UpdateValue(nameof(Background));

		// `nameof(BackgroundColor)`/`nameof(Page.BackgroundImageSource)` are appended (by RemapForControls) after
		// the Core `nameof(IView.Background)` key in `ViewHandler.ViewMapper`, so during the single bulk
		// `UpdateProperties` pass that runs on initial connect or reconnect, the canonical `Background`
		// mapping already reads the current composite BackgroundColor/BackgroundImageSource/Background
		// value later in that very same pass, making this redirect redundant. `RecordBackgroundMapped`
		// marks, on the handler, that a pending redundant invocation of this key exists for the pass that
		// just ran `Background`'s mapping; this method consumes that one pending mark (skipping only that
		// single invocation) and applies normally for every other call - whether it happens before
		// `Background` has ever been mapped (e.g. from `ConnectHandler`), after the pending mark has
		// already been consumed once (e.g. a mapper that runs later in the same pass and changes the
		// value), or with an unchanged value. See VisualElementMapperTests for coverage of this invariant.
		static void MapBackgroundColorForControls(IViewHandler handler, IView view)
		{
			var state = GetMappingPassState(handler);

			if (state.BackgroundColorPendingSkip)
			{
				state.BackgroundColorPendingSkip = false;
			}
			else
			{
				ApplyBackgroundRedirect(state, handler, view, MapBackgroundColor);
			}
		}

		// Same reasoning as `MapBackgroundColorForControls` above. This key is registered on the shared
		// `ViewHandler.ViewMapper` used by every view type (even though `BackgroundImageSource` is only a
		// real, settable property on `Page`), but that's harmless here: for non-`Page` views the pending
		// mark set by `RecordBackgroundMapped` is still consumed on the natural bulk-pass turn, so no extra
		// work is ever done for them during a normal pass.
		static void MapBackgroundImageSourceForControls(IViewHandler handler, IView view)
		{
			var state = GetMappingPassState(handler);

			if (state.BackgroundImageSourcePendingSkip)
			{
				state.BackgroundImageSourcePendingSkip = false;
			}
			else
			{
				ApplyBackgroundRedirect(state, handler, view, MapBackgroundImageSource);
			}
		}

		// Applying either background redirect re-invokes `nameof(Background)`'s own mapping (through
		// `MapBackgroundColor`/`MapBackgroundImageSource`), which synchronously re-triggers
		// `RecordBackgroundMapped`. That particular execution of `Background`'s mapping is a leaf, one-off
		// call made *by* a redirect, not a step of the top-level bulk-pass enumeration, so it must not
		// re-arm the pending marks (there is no separate, later redirect turn guaranteed to follow it in
		// this call chain) - `IsApplyingBackgroundRedirect` flags that to `RecordBackgroundMapped` below.
		static void ApplyBackgroundRedirect(MappingPassState state, IViewHandler handler, IView view, Action<IViewHandler, IView> map)
		{
			state.IsApplyingBackgroundRedirect = true;
			try
			{
				map(handler, view);
			}
			finally
			{
				state.IsApplyingBackgroundRedirect = false;
			}
		}

		// Marks, on the handler, that the very next invocation of `MapBackgroundColorForControls`/
		// `MapBackgroundImageSourceForControls` (whichever comes first for each) is a redundant no-op -
		// `Background` was just (re)computed from the current BackgroundColor/BackgroundImageSource, so
		// immediately re-running either redirect within the same pass would just push the same value
		// again. Appended to the `Background` key itself, this runs every time that key's mapping executes,
		// through any path (bulk connect/reconnect pass or an explicit `UpdateValue(nameof(Background))`) -
		// except when it is itself a leaf call made by one of the redirects above (see
		// `ApplyBackgroundRedirect`), which must not arm a pending mark for anything.
		static void RecordBackgroundMapped(IViewHandler handler, IView view)
		{
			var state = GetMappingPassState(handler);

			if (state.IsApplyingBackgroundRedirect)
			{
				return;
			}

			state.BackgroundColorPendingSkip = true;
			state.BackgroundImageSourcePendingSkip = true;
		}

		// Same reasoning as `MapBackgroundColorForControls` above, but for the `SemanticProperties.Description`
		// redirect.
		static void MapSemanticPropertiesDescriptionProperty(IViewHandler handler, IView element)
		{
			var state = GetMappingPassState(handler);

			if (state.SemanticDescriptionPendingSkip)
			{
				state.SemanticDescriptionPendingSkip = false;
			}
			else if (element is VisualElement ve)
			{
				ApplySemanticsRedirect(state, ve);
			}
		}

		// Same reasoning as `MapBackgroundColorForControls` above, but for the `SemanticProperties.Hint`
		// redirect.
		static void MapSemanticPropertiesHintProperty(IViewHandler handler, IView element)
		{
			var state = GetMappingPassState(handler);

			if (state.SemanticHintPendingSkip)
			{
				state.SemanticHintPendingSkip = false;
			}
			else if (element is VisualElement ve)
			{
				ApplySemanticsRedirect(state, ve);
			}
		}

		// Same reasoning as `MapBackgroundColorForControls` above, but for the
		// `SemanticProperties.HeadingLevel` redirect.
		static void MapSemanticPropertiesHeadingLevelProperty(IViewHandler handler, IView element)
		{
			var state = GetMappingPassState(handler);

			if (state.SemanticHeadingLevelPendingSkip)
			{
				state.SemanticHeadingLevelPendingSkip = false;
			}
			else if (element is VisualElement ve)
			{
				ApplySemanticsRedirect(state, ve);
			}
		}

		// See `ApplyBackgroundRedirect`: applying a semantics redirect re-invokes `nameof(Semantics)`'s own
		// mapping, synchronously re-triggering `RecordSemanticsMapped` for a leaf, one-off call that must
		// not re-arm the pending marks.
		static void ApplySemanticsRedirect(MappingPassState state, VisualElement element)
		{
			state.IsApplyingSemanticsRedirect = true;
			try
			{
				element.UpdateSemanticsFromMapper();
			}
			finally
			{
				state.IsApplyingSemanticsRedirect = false;
			}
		}

		// See `RecordBackgroundMapped`: marks, on the handler, that the very next invocation of each of the
		// three `SemanticProperties.*` redirects above is a redundant no-op, since `Semantics` was just
		// (re)computed from the current Description/Hint/HeadingLevel values - except when it is itself a
		// leaf call made by one of those redirects (see `ApplySemanticsRedirect`).
		static void RecordSemanticsMapped(IViewHandler handler, IView view)
		{
			var state = GetMappingPassState(handler);

			if (state.IsApplyingSemanticsRedirect)
			{
				return;
			}

			state.SemanticDescriptionPendingSkip = true;
			state.SemanticHintPendingSkip = true;
			state.SemanticHeadingLevelPendingSkip = true;
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

		// Per-handler (not per-VisualElement) tracking of which `Background`/`Semantics`-derived redirect
		// keys still have a pending, provably-redundant invocation outstanding for the bulk mapping pass
		// that most recently (re-)mapped `Background`/`Semantics`. Keyed on the handler - via a
		// ConditionalWeakTable so no manual cleanup/disconnect handling is needed - because de-duplication
		// must be scoped to "has this specific handler's current pass already produced this key's natural,
		// redundant call", not to any state that outlives a single pass or that lives on the element.
		static readonly ConditionalWeakTable<IViewHandler, MappingPassState> s_mappingPassState = new();

		static MappingPassState GetMappingPassState(IViewHandler handler) =>
			s_mappingPassState.GetValue(handler, static _ => new MappingPassState());

		sealed class MappingPassState
		{
			public bool BackgroundColorPendingSkip;
			public bool BackgroundImageSourcePendingSkip;
			public bool SemanticDescriptionPendingSkip;
			public bool SemanticHintPendingSkip;
			public bool SemanticHeadingLevelPendingSkip;
			public bool IsApplyingBackgroundRedirect;
			public bool IsApplyingSemanticsRedirect;
		}
	}
}
