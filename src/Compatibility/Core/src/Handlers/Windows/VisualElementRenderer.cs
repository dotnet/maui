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
	}
}