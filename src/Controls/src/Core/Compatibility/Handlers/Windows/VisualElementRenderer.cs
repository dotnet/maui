using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WRect = Windows.Foundation.Rect;
using WSize = Windows.Foundation.Size;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class VisualElementRenderer<TElement, TPlatformElement>
		: Panel, IDisposable
		where TElement : VisualElement
		where TPlatformElement : FrameworkElement
	{
		TPlatformElement? _nativeView;
		public FrameworkElement ContainerElement => this;

		public TPlatformElement? Control
		{
			get
			{
				var value = ((IElementHandler)this).PlatformView as TPlatformElement;
				if (value != this && value != null)
					return value;

				return _nativeView;
			}
		}

		object? IElementHandler.PlatformView
		{
			get => (_nativeView as object) ?? this;
		}

		public UIElement? GeTPlatformElement() => Control;

		UIElementCollection? _cachedChildren;

		[SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "Panel.Children property is banned to enforce use of this CachedChildren property.")]
		internal UIElementCollection CachedChildren
		{
			get
			{
				_cachedChildren ??= Children;
				return _cachedChildren;
			}
		}

		protected virtual void UpdateNativeControl() { }

		protected void SetNativeControl(TPlatformElement control)
		{
			TPlatformElement? oldControl = Control;
			_nativeView = control;

			if (oldControl != null)
			{
				CachedChildren.Remove(oldControl);
			}

			if (Control == null)
			{
				return;
			}

			Control.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
			Control.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;

			CachedChildren.Add(control);
			UpdateNativeControl();
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		protected override WSize MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			if (Element == null || availableSize.Width * availableSize.Height == 0)
				return new WSize(0, 0);

			Control?.Measure(availableSize);

			var mauiContext = Element?.Handler?.MauiContext;
			var minimumSize = MinimumSize();
			var mauiRect = Control?.DesiredSize ?? minimumSize.ToPlatform();

			if (Element is not IVisualTreeElement vte || mauiContext == null)
				return mauiRect;

			var width = Math.Max(mauiRect.Width, minimumSize.Width);
			var height = Math.Max(mauiRect.Height, minimumSize.Height);

			foreach (var child in vte.GetVisualChildren())
			{
				if (child is Maui.IElement childElement && childElement.Handler is IPlatformViewHandler nvh)
				{
					var size = nvh.GetDesiredSizeFromHandler(availableSize.Width, availableSize.Height);
					height = Math.Max(height, size.Height);
					width = Math.Max(width, size.Width);
				}
			}

			return new WSize(width, height);
		}

		protected override WSize ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			var myRect = new WRect(0, 0, finalSize.Width, finalSize.Height);
			Control?.Arrange(myRect);

			var mauiContext = Element?.Handler?.MauiContext;
			if (Element is not IVisualTreeElement vte || mauiContext == null)
				return finalSize;

			var mauiRect = new Graphics.Rect(0, 0, finalSize.Width, finalSize.Height);
			foreach (var child in vte.GetVisualChildren())
			{
				if (child is Maui.IElement childElement && childElement.Handler is IPlatformViewHandler nvh)
					nvh.PlatformArrangeHandler(mauiRect);
			}

			return finalSize;
		}

		void IDisposable.Dispose()
		{
			(this as IElementHandler).DisconnectHandler();
			Dispose(true);
		}

		protected virtual void SetAutomationPropertiesLabeledBy()
		{
			if (Element != null)
				VisualElement.MapAutomationPropertiesLabeledBy(this, Element);
		}

		protected virtual void SetAutomationPropertiesHelpText()
		{
			if (Element != null)
				VisualElement.MapAutomationPropertiesHelpText(this, Element);
		}


		protected virtual void SetAutomationPropertiesName()
		{
			if (Element != null)
				VisualElement.MapAutomationPropertiesName(this, Element);
		}

		static partial void ProcessAutoPackage(Maui.IElement element)
		{
			if (element.Handler is not IPlatformViewHandler nvh ||
				nvh.ContainerView is not Panel panel)
			{
				return;
			}

#pragma warning disable RS0030 // Do not use banned APIs; Panel.Children is banned for performance reasons. Here we can just cache it.
			var panelChildren = panel.Children;
#pragma warning restore RS0030 // Do not use banned APIs
			panelChildren.Clear();

			if (element is not IVisualTreeElement vte)
				return;

			var mauiContext = element?.Handler?.MauiContext;
			if (mauiContext == null)
				return;

			foreach (var child in vte.GetVisualChildren())
			{
				if (child is Maui.IElement childElement)
					panelChildren.Add(childElement.ToPlatform(mauiContext));
			}
		}

		public static void MapAutomationPropertiesLabeledBy(IPlatformViewHandler handler, TElement view)
		{
			if (handler is VisualElementRenderer<TElement, TPlatformElement> ver)
				ver.SetAutomationPropertiesLabeledBy();
		}

		public static void MapAutomationPropertiesHelpText(IPlatformViewHandler handler, TElement view)
		{
			if (handler is VisualElementRenderer<TElement, TPlatformElement> ver)
				ver.SetAutomationPropertiesHelpText();
		}

		public static void MapAutomationPropertiesName(IPlatformViewHandler handler, TElement view)
		{
			if (handler is VisualElementRenderer<TElement, TPlatformElement> ver)
				ver.SetAutomationPropertiesName();
		}
	}
}