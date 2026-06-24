#nullable disable
using System;
using System.ComponentModel;
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
#pragma warning disable MAUI0003, CS0618 // BackgroundColor mapper registration — kept for backward compatibility with existing XAML and bindings
			viewMapper.ReplaceMapping<IView, IViewHandler>(nameof(BackgroundColor), MapBackgroundColor);
#pragma warning restore MAUI0003, CS0618
			viewMapper.ReplaceMapping<IView, IViewHandler>(nameof(Page.BackgroundImageSource), MapBackgroundImageSource);
			viewMapper.ReplaceMapping<IView, IViewHandler>(SemanticProperties.DescriptionProperty.PropertyName, MapSemanticPropertiesDescriptionProperty);
			viewMapper.ReplaceMapping<IView, IViewHandler>(SemanticProperties.HintProperty.PropertyName, MapSemanticPropertiesHintProperty);
			viewMapper.ReplaceMapping<IView, IViewHandler>(SemanticProperties.HeadingLevelProperty.PropertyName, MapSemanticPropertiesHeadingLevelProperty);

			viewMapper.AppendToMapping<VisualElement, IViewHandler>(nameof(IViewHandler.ContainerView), MapContainerView);

			commandMapper.ModifyMapping<VisualElement, IViewHandler>(nameof(IView.Focus), MapFocus);
		}

		/// <summary>Updates the handler when <see cref="VisualElement.BackgroundColor"/> changes by re-applying <see cref="VisualElement.Background"/>.</summary>
#if NET5_0_OR_GREATER
		[Obsolete("MapBackgroundColor is obsolete and will be removed in .NET 12. BackgroundColor changes are handled via the MAUI0003 diagnostic. Use Background instead.",
			DiagnosticId = MauiObsoleteConstants.BackgroundColorObsolete,
			UrlFormat = "https://aka.ms/maui-obsolete-backgroundcolor")]
#else
		[Obsolete("MapBackgroundColor is obsolete and will be removed in .NET 12. Use Background instead.")]
#endif
		[EditorBrowsable(EditorBrowsableState.Never)]
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
