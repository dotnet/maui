#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Platform;
using System.ComponentModel;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class VisualElementRenderer<TElement> : UIView, INativeViewHandler
		where TElement : Element, IView
	{
		public virtual UIViewController? ViewController => null;

		protected virtual void UpdateNativeWidget()
		{

		}

		partial void ElementPropertyChangedPartial(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.XProperty.PropertyName ||
				e.PropertyName == VisualElement.YProperty.PropertyName ||
				e.PropertyName == VisualElement.WidthProperty.PropertyName ||
				e.PropertyName == VisualElement.HeightProperty.PropertyName ||
				e.PropertyName == VisualElement.AnchorXProperty.PropertyName ||
				e.PropertyName == VisualElement.AnchorYProperty.PropertyName ||
				e.PropertyName == VisualElement.TranslationXProperty.PropertyName ||
				e.PropertyName == VisualElement.TranslationYProperty.PropertyName ||
				e.PropertyName == VisualElement.ScaleProperty.PropertyName ||
				e.PropertyName == VisualElement.ScaleXProperty.PropertyName ||
				e.PropertyName == VisualElement.ScaleYProperty.PropertyName ||
				e.PropertyName == VisualElement.RotationProperty.PropertyName ||
				e.PropertyName == VisualElement.RotationXProperty.PropertyName ||
				e.PropertyName == VisualElement.RotationYProperty.PropertyName ||
				e.PropertyName == VisualElement.IsVisibleProperty.PropertyName ||
				e.PropertyName == VisualElement.IsEnabledProperty.PropertyName ||
				e.PropertyName == VisualElement.InputTransparentProperty.PropertyName ||
				e.PropertyName == VisualElement.OpacityProperty.PropertyName ||
				e.PropertyName == Controls.Compatibility.Layout.CascadeInputTransparentProperty.PropertyName)
			{
				UpdateNativeWidget();
			}
		}

		partial void ElementChangedPartial(ElementChangedEventArgs<TElement> e)
		{
			if (e.OldElement is VisualElement oldVe)
			{
				oldVe.SizeChanged -= OnSizeChanged;
				oldVe.BatchCommitted -= OnBatchCommitted;

			}

			if (e.NewElement is VisualElement newVe)
			{
				newVe.SizeChanged += OnSizeChanged;
				newVe.BatchCommitted += OnBatchCommitted;
			}
		}

		void OnBatchCommitted(object? sender, Internals.EventArg<VisualElement> e)
		{
			UpdateNativeWidget();
		}

		void OnSizeChanged(object? sender, EventArgs e)
		{
			UpdateNativeWidget();
		}
	}
}
