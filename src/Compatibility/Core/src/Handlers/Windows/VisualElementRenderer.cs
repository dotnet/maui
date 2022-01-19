#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class VisualElementRenderer<TElement, TNativeElement>
		: Panel, IDisposable
		where TElement : VisualElement
		where TNativeElement : FrameworkElement
	{
		TNativeElement? _nativeView;
		public FrameworkElement ContainerElement => this;

		public TNativeElement? Control => ((IElementHandler)this).NativeView as TNativeElement ?? _nativeView;
		public UIElement? GetNativeElement() => Control;

		protected virtual void UpdateNativeControl() { }

		protected void SetNativeControl(TNativeElement control)
		{
			TNativeElement? oldControl = Control;
			_nativeView = control;

			if (oldControl != null)
			{
				Children.Remove(oldControl);
			}

			if (Control == null)
			{
				return;
			}

			Control.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
			Control.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;

			Children.Add(control);
			UpdateNativeControl();
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			if (Children.Count > 0)
			{
				var platformView = Children[0];
				if (platformView != null)
				{
					platformView.Measure(availableSize);
					return platformView.DesiredSize;
				}
			}

			return base.MeasureOverride(availableSize);
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			if (Children.Count > 0)
			{
				var platformView = Children[0];
				if (platformView != null)
				{
					platformView.Arrange(new global::Windows.Foundation.Rect(0, 0, finalSize.Width, finalSize.Height));
					return finalSize;
				}
			}

			return base.ArrangeOverride(finalSize);
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
			if (element.Handler is not INativeViewHandler nvh ||
				nvh.ContainerView is not Panel panel)
			{
				return;
			}

			panel.Children.Clear();

			if (element is not IVisualTreeElement vte)
				return;

			var mauiContext = element?.Handler?.MauiContext;
			if (mauiContext == null)
				return;

			foreach (var child in vte.GetVisualChildren())
			{
				if (child is Maui.IElement childElement)
					panel.Children.Add(childElement.ToNative(mauiContext));
			}
		}

		public static void MapAutomationPropertiesLabeledBy(INativeViewHandler handler, TElement view)
		{
			if (handler is VisualElementRenderer<TElement, TNativeElement> ver)
				ver.SetAutomationPropertiesLabeledBy();
		}

		public static void MapAutomationPropertiesHelpText(INativeViewHandler handler, TElement view)
		{
			if (handler is VisualElementRenderer<TElement, TNativeElement> ver)
				ver.SetAutomationPropertiesHelpText();
		}

		public static void MapAutomationPropertiesName(INativeViewHandler handler, TElement view)
		{
			if (handler is VisualElementRenderer<TElement, TNativeElement> ver)
				ver.SetAutomationPropertiesName();
		}
	}
}