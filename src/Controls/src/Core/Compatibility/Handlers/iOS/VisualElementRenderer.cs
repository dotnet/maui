using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class VisualElementRenderer<TElement> : UIView, IPlatformViewHandler, IElementHandler, IPlatformMeasureInvalidationController
		where TElement : Element, IView
	{
		bool _invalidateParentWhenMovedToWindow;
		object? IElementHandler.PlatformView => Subviews.Length > 0 ? Subviews[0] : this;

		public virtual UIViewController? ViewController => null;

		static partial void ProcessAutoPackage(Maui.IElement element)
		{
			if (element?.Handler?.PlatformView is not UIView viewGroup)
				return;

			viewGroup.ClearSubviews();

			if (element is not IVisualTreeElement vte)
				return;

			var mauiContext = element?.Handler?.MauiContext;
			if (mauiContext == null)
				return;

			foreach (var child in vte.GetVisualChildren())
			{
				if (child is Maui.IElement childElement)
					viewGroup.AddSubview(childElement.ToPlatform(mauiContext));
			}
		}

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
#pragma warning disable CS0618 // Type or member is obsolete
				e.PropertyName == Controls.Compatibility.Layout.CascadeInputTransparentProperty.PropertyName
#pragma warning restore CS0618 // Type or member is obsolete
				)
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

		void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
		{
			_invalidateParentWhenMovedToWindow = true;
		}

		bool IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
		{
			SetNeedsLayout();
			return true;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			if (_invalidateParentWhenMovedToWindow)
			{
				_invalidateParentWhenMovedToWindow = false;
				this.InvalidateAncestorsMeasures();
			}
		}
	}
}
