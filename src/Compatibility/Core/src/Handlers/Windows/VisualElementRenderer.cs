#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class VisualElementRenderer<TElement, TNativeElement>
		: Panel//, IVisualNativeElementRenderer, IDisposable, IEffectControlProvider 
		where TElement : VisualElement
		where TNativeElement : FrameworkElement
	{
		//protected virtual bool PreventGestureBubbling { get; set; } = false;

		TNativeElement? _nativeView;
		public FrameworkElement ContainerElement
		{
			get { return this; }
		}

		public TNativeElement? Control => ((IElementHandler)this).NativeView as TNativeElement ?? _nativeView;



		internal void SetNativeControl(TNativeElement control)
		{
			TNativeElement? oldControl = Control;
			_nativeView = control;

			if (oldControl != null)
			{
				Children.Remove(oldControl);
			}

			if (Control == null)
			{
				//	_controlChanged?.Invoke(this, EventArgs.Empty);
				return;
			}

			Control.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
			Control.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;

			Children.Add(control);
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
	}
}