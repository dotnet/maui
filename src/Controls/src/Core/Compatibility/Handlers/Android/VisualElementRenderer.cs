using System;
using System.ComponentModel;
using Android.Content;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class VisualElementRenderer<TElement> : AViewGroup, IPlatformViewHandler
		where TElement : Element, IView
	{
		object? IElementHandler.PlatformView
		{
			get => ChildCount > 0 ? GetChildAt(0) : this;
		}

		static partial void ProcessAutoPackage(Maui.IElement element)
		{
			if (element?.Handler?.PlatformView is not AViewGroup viewGroup)
				return;

			viewGroup.RemoveAllViews();

			if (element is not IVisualTreeElement vte)
				return;

			var mauiContext = element?.Handler?.MauiContext;
			if (mauiContext == null)
				return;

			foreach (var child in vte.GetVisualChildren())
			{
				if (child is Maui.IElement childElement)
					viewGroup.AddView(childElement.ToPlatform(mauiContext));
			}
		}

		public void UpdateLayout()
		{
			if (Element != null)
				this.InvalidateMeasure(Element);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (ChildCount > 0)
			{
				var platformView = GetChildAt(0);
				platformView?.Layout(0, 0, r - l, b - t);
			}
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (ChildCount > 0)
			{
				var platformView = GetChildAt(0);
				if (platformView != null)
				{
					platformView.Measure(widthMeasureSpec, heightMeasureSpec);
					SetMeasuredDimension(platformView.MeasuredWidth, platformView.MeasuredHeight);
					return;
				}
			}

			SetMeasuredDimension(0, 0);
		}
	}
}
