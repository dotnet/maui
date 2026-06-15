#nullable disable
using System;
using System.Threading;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;
#if IOS || MACCATALYST
using Microsoft.Maui.Controls.Platform;
using UIKit;
#endif

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides the base class for all visual elements in .NET MAUI.
	/// </summary>
	public partial class VisualElement
	{
		static int s_remappedForControls;

		internal override void RemapForControls()
		{
			if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
				return;

			base.RemapForControls();

#if WINDOWS
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyHorizontalOffsetProperty.PropertyName, MapAccessKeyHorizontalOffset);
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyPlacementProperty.PropertyName, MapAccessKeyPlacement);
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyProperty.PropertyName, MapAccessKey);
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(PlatformConfiguration.WindowsSpecific.VisualElement.AccessKeyVerticalOffsetProperty.PropertyName, MapAccessKeyVerticalOffset);
#endif
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(nameof(BackgroundColor), MapBackgroundColorForControls);
			ViewHandler.ViewMapper.ReplaceMapping<IView, IViewHandler>(nameof(Page.BackgroundImageSource), MapBackgroundImageSourceForControls);
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

		// `nameof(BackgroundColor)`/`nameof(Page.BackgroundImageSource)` are appended (by RemapForControls)
		// after the Core `nameof(IView.Background)` key, so during a bulk `UpdateProperties` pass - initial
		// connect, reconnect/handler reuse, or any other full remap - the canonical `Background` mapping
		// has already read the current composite BackgroundColor/BackgroundImageSource/Background value by
		// the time this key's own turn comes up, making that one turn a pure no-op.
		//
		// `IsRedundantBulkPassRedirect` skips *only* that turn: it returns true exclusively when the
		// innermost bulk pass on this thread belongs to this handler, is currently executing this very key's
		// step, and already executed the `Background` key earlier in the same pass. Crucially it asks
		// nothing about *which* mapping `Background` resolves to, so it works identically when a derived
		// handler owns/overrides that key (ButtonHandler, PageHandler, LabelHandler, ...) - which is exactly
		// what re-dispatching through `MapBackgroundColor` would have invoked again anyway.
		//
		// Every other invocation still applies normally: a call made before `Background` has ever been
		// mapped (e.g. from `ConnectHandler`), a re-entrant call raised by a mapper that runs later in the
		// same pass and legitimately changes the value, an explicit `Handler.UpdateValue(...)` with an
		// unchanged value, a call arriving while a nested pass for another handler is on top, and any call
		// made outside of a bulk pass entirely. See VisualElementMapperTests for coverage of this invariant.
		static void MapBackgroundColorForControls(IViewHandler handler, IView view)
		{
			if (PropertyMapperPassScope.IsRedundantBulkPassRedirect(handler, nameof(BackgroundColor), nameof(IView.Background)))
			{
				return;
			}

			MapBackgroundColor(handler, view);
		}

		// Same reasoning as `MapBackgroundColorForControls` above. This key is registered on the shared
		// `ViewHandler.ViewMapper` used by every view type (even though `BackgroundImageSource` is only a
		// real, settable property on `Page`), but that's harmless: for non-`Page` views the redirect's
		// bulk-pass turn is skipped just the same, so no extra work is ever done for them during a pass.
		static void MapBackgroundImageSourceForControls(IViewHandler handler, IView view)
		{
			if (PropertyMapperPassScope.IsRedundantBulkPassRedirect(handler, nameof(Page.BackgroundImageSource), nameof(IView.Background)))
			{
				return;
			}

			MapBackgroundImageSource(handler, view);
		}

		// Same reasoning as `MapBackgroundColorForControls` above, but redirecting into the canonical
		// `nameof(IView.Semantics)` key instead. Note that `IView.Semantics` recomputes the composite value
		// on read (see `VisualElement.UpdateSemantics`), so the canonical mapping running earlier in the
		// pass has already observed the current Description/Hint/HeadingLevel.
		static void MapSemanticPropertiesDescriptionProperty(IViewHandler handler, IView element) =>
			MapSemanticPropertyForControls(handler, element, SemanticProperties.DescriptionProperty.PropertyName);

		// Same reasoning as `MapSemanticPropertiesDescriptionProperty` above.
		static void MapSemanticPropertiesHintProperty(IViewHandler handler, IView element) =>
			MapSemanticPropertyForControls(handler, element, SemanticProperties.HintProperty.PropertyName);

		// Same reasoning as `MapSemanticPropertiesDescriptionProperty` above.
		static void MapSemanticPropertiesHeadingLevelProperty(IViewHandler handler, IView element) =>
			MapSemanticPropertyForControls(handler, element, SemanticProperties.HeadingLevelProperty.PropertyName);

		static void MapSemanticPropertyForControls(IViewHandler handler, IView element, string redirectKey)
		{
			if (PropertyMapperPassScope.IsRedundantBulkPassRedirect(handler, redirectKey, nameof(IView.Semantics)))
			{
				return;
			}

			if (element is VisualElement ve)
			{
				ve.UpdateSemanticsFromMapper();
			}
		}

		void UpdateSemanticsFromMapper()
		{
			UpdateSemantics();
			Handler?.UpdateValue(nameof(IView.Semantics));

#if IOS || MACCATALYST
			UpdateCollectionViewAccessibilityTraits();
#endif
		}

#if IOS || MACCATALYST
		void UpdateCollectionViewAccessibilityTraits()
		{
			if (this.FindParentOfType<CollectionView>() is not CollectionView collectionView ||
				Handler?.PlatformView is not UIView platformView)
			{
				return;
			}

			UIView current = platformView;
			while (current is not null && current is not UICollectionViewCell)
			{
				current = current.Superview;
			}

			if (current is UICollectionViewCell cell)
			{
				cell.UpdateAccessibilityTraits(collectionView, invalidateAccessibilityTarget: true);
			}
		}
#endif

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
