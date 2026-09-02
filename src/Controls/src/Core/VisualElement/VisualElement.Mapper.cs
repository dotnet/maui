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
			// each key within *this same bulk `UpdateProperties` pass* - identified via
			// PropertyMapperPassScope, not merely "the next time this key happens to run". This never
			// suppresses an explicit/reentrant call, a same-value call, a call that happens before any
			// `Background`/`Semantics` mapping has run at all, or a call triggered by `Background`/
			// `Semantics` running *outside* of a bulk pass (e.g. a platform mapper that reacts to some
			// other property by directly calling `Handler.UpdateValue(nameof(Background))` /
			// `nameof(Semantics))` well after connect - see PickerHandler.Windows.MapTitle for a real
			// example of this shape). See VisualElementMapperTests for coverage of this invariant.
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
		// marks, on the handler, that a pending redundant invocation of this key exists for the *specific*
		// bulk pass that just ran `Background`'s mapping (identified by `PropertyMapperPassScope`'s pass
		// id); this method consumes that one pending mark only if it is still the current pass (skipping
		// only that single invocation) and applies normally for every other call - whether it happens
		// before `Background` has ever been mapped (e.g. from `ConnectHandler`), after the pending mark has
		// already been consumed once (e.g. a mapper that runs later in the same pass and changes the
		// value), with an unchanged value, or after the pass that armed the mark has since ended (whether
		// normally or because a mapper threw) - a stale mark from a pass that is no longer current can never
		// be consumed. See VisualElementMapperTests for coverage of this invariant.
		static void MapBackgroundColorForControls(IViewHandler handler, IView view)
		{
			var state = GetMappingPassState(handler);
			var currentPassId = PropertyMapperPassScope.GetCurrentPassId(handler);

			if (state.BackgroundColorPendingSkipPassId != 0 && state.BackgroundColorPendingSkipPassId == currentPassId)
			{
				state.BackgroundColorPendingSkipPassId = 0;
			}
			else
			{
				state.BackgroundColorPendingSkipPassId = 0;
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
			var currentPassId = PropertyMapperPassScope.GetCurrentPassId(handler);

			if (state.BackgroundImageSourcePendingSkipPassId != 0 && state.BackgroundImageSourcePendingSkipPassId == currentPassId)
			{
				state.BackgroundImageSourcePendingSkipPassId = 0;
			}
			else
			{
				state.BackgroundImageSourcePendingSkipPassId = 0;
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
		// `MapBackgroundImageSourceForControls` (whichever comes first for each) is a redundant no-op for
		// *this specific* bulk pass - `Background` was just (re)computed from the current
		// BackgroundColor/BackgroundImageSource, so immediately re-running either redirect within the same
		// pass would just push the same value again. Appended to the `Background` key itself, this runs
		// every time that key's mapping executes, through any path - except: (1) when it is itself a leaf
		// call made by one of the redirects above (see `ApplyBackgroundRedirect`), which must not arm a
		// pending mark for anything, and (2) when `Background` is being mapped *outside* of a bulk
		// `UpdateProperties` pass entirely (`PropertyMapperPassScope.GetCurrentPassId` returns 0) - e.g. a
		// platform mapper reacting to some unrelated property by directly calling
		// `Handler.UpdateValue(nameof(Background))` well after connect. In that case there is no guarantee
		// whatsoever that either redirect's turn is still coming, so nothing may be armed.
		static void RecordBackgroundMapped(IViewHandler handler, IView view)
		{
			var state = GetMappingPassState(handler);

			if (state.IsApplyingBackgroundRedirect)
			{
				return;
			}

			var passId = PropertyMapperPassScope.GetCurrentPassId(handler);
			if (passId == 0)
			{
				return;
			}

			state.BackgroundColorPendingSkipPassId = passId;
			state.BackgroundImageSourcePendingSkipPassId = passId;
		}

		// Same reasoning as `MapBackgroundColorForControls` above, but for the `SemanticProperties.Description`
		// redirect.
		static void MapSemanticPropertiesDescriptionProperty(IViewHandler handler, IView element)
		{
			var state = GetMappingPassState(handler);
			var currentPassId = PropertyMapperPassScope.GetCurrentPassId(handler);

			if (state.SemanticDescriptionPendingSkipPassId != 0 && state.SemanticDescriptionPendingSkipPassId == currentPassId)
			{
				state.SemanticDescriptionPendingSkipPassId = 0;
			}
			else
			{
				state.SemanticDescriptionPendingSkipPassId = 0;
				if (element is VisualElement ve)
				{
					ApplySemanticsRedirect(state, ve);
				}
			}
		}

		// Same reasoning as `MapBackgroundColorForControls` above, but for the `SemanticProperties.Hint`
		// redirect.
		static void MapSemanticPropertiesHintProperty(IViewHandler handler, IView element)
		{
			var state = GetMappingPassState(handler);
			var currentPassId = PropertyMapperPassScope.GetCurrentPassId(handler);

			if (state.SemanticHintPendingSkipPassId != 0 && state.SemanticHintPendingSkipPassId == currentPassId)
			{
				state.SemanticHintPendingSkipPassId = 0;
			}
			else
			{
				state.SemanticHintPendingSkipPassId = 0;
				if (element is VisualElement ve)
				{
					ApplySemanticsRedirect(state, ve);
				}
			}
		}

		// Same reasoning as `MapBackgroundColorForControls` above, but for the
		// `SemanticProperties.HeadingLevel` redirect.
		static void MapSemanticPropertiesHeadingLevelProperty(IViewHandler handler, IView element)
		{
			var state = GetMappingPassState(handler);
			var currentPassId = PropertyMapperPassScope.GetCurrentPassId(handler);

			if (state.SemanticHeadingLevelPendingSkipPassId != 0 && state.SemanticHeadingLevelPendingSkipPassId == currentPassId)
			{
				state.SemanticHeadingLevelPendingSkipPassId = 0;
			}
			else
			{
				state.SemanticHeadingLevelPendingSkipPassId = 0;
				if (element is VisualElement ve)
				{
					ApplySemanticsRedirect(state, ve);
				}
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
		// three `SemanticProperties.*` redirects above is a redundant no-op for *this specific* bulk pass,
		// since `Semantics` was just (re)computed from the current Description/Hint/HeadingLevel values -
		// except when it is itself a leaf call made by one of those redirects (see
		// `ApplySemanticsRedirect`), or when `Semantics` is being mapped outside of any bulk
		// `UpdateProperties` pass entirely (e.g. `PickerHandler.Windows.MapTitle`, which reacts to `Title`
		// changing - possibly well after connect - by directly calling
		// `Handler.UpdateValue(nameof(IView.Semantics))`; that call must never be allowed to suppress a
		// later, unrelated `SemanticProperties.*` update).
		static void RecordSemanticsMapped(IViewHandler handler, IView view)
		{
			var state = GetMappingPassState(handler);

			if (state.IsApplyingSemanticsRedirect)
			{
				return;
			}

			var passId = PropertyMapperPassScope.GetCurrentPassId(handler);
			if (passId == 0)
			{
				return;
			}

			state.SemanticDescriptionPendingSkipPassId = passId;
			state.SemanticHintPendingSkipPassId = passId;
			state.SemanticHeadingLevelPendingSkipPassId = passId;
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
		// keys still have a pending, provably-redundant invocation outstanding for the *specific* bulk
		// mapping pass (see `Microsoft.Maui.PropertyMapperPassScope`) that most recently (re-)mapped
		// `Background`/`Semantics`. Keyed on the handler - via a ConditionalWeakTable so no manual
		// cleanup/disconnect handling is needed - because de-duplication must be scoped to "has this
		// specific handler's *current* bulk pass already produced this key's natural, redundant call", not
		// to any state that outlives a single pass or that lives on the element.
		static readonly ConditionalWeakTable<IViewHandler, MappingPassState> s_mappingPassState = new();

		static MappingPassState GetMappingPassState(IViewHandler handler) =>
			s_mappingPassState.GetValue(handler, static _ => new MappingPassState());

		sealed class MappingPassState
		{
			// Each field is 0 ("no pending skip") or the id (from PropertyMapperPassScope) of the bulk
			// pass that armed it. A pending skip is honored only while PropertyMapperPassScope currently
			// reports that exact same pass id as active for this handler - once the pass that armed a
			// field has ended (normally, or because a mapper threw and PropertyMapper.UpdateProperties'
			// finally ran), that id can never again match, so the field is permanently inert even if
			// never explicitly reset to 0.
			public int BackgroundColorPendingSkipPassId;
			public int BackgroundImageSourcePendingSkipPassId;
			public int SemanticDescriptionPendingSkipPassId;
			public int SemanticHintPendingSkipPassId;
			public int SemanticHeadingLevelPendingSkipPassId;
			public bool IsApplyingBackgroundRedirect;
			public bool IsApplyingSemanticsRedirect;
		}
	}
}
