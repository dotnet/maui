using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System;
using static System.String;

#if __UNIFIED__
using UIKit;

#else
using MonoTouch.UIKit;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public static class UIViewExtensions
	{
		public static IEnumerable<UIView> Descendants(this UIView self)
		{
			if (self.Subviews == null)
				return Enumerable.Empty<UIView>();
			return self.Subviews.Concat(self.Subviews.SelectMany(s => s.Descendants()));
		}

		public static SizeRequest GetSizeRequest(this UIView self, double widthConstraint, double heightConstraint, double minimumWidth = -1, double minimumHeight = -1)
		{
			var s = self.SizeThatFits(new SizeF((float)widthConstraint, (float)heightConstraint));
			var request = new Size(s.Width == float.PositiveInfinity ? double.PositiveInfinity : s.Width, s.Height == float.PositiveInfinity ? double.PositiveInfinity : s.Height);
			var minimum = new Size(minimumWidth < 0 ? request.Width : minimumWidth, minimumHeight < 0 ? request.Height : minimumHeight);
			return new SizeRequest(request, minimum);
		}

		public static void SetBinding(this UIView view, string propertyName, BindingBase bindingBase, string updateSourceEventName = null)
		{
			var binding = bindingBase as Binding;
			//This will allow setting bindings from Xaml by reusing the MarkupExtension
			updateSourceEventName = updateSourceEventName ?? binding?.UpdateSourceEventName;

			if (!IsNullOrEmpty(updateSourceEventName))
			{
				NativeBindingHelpers.SetBinding(view, propertyName, bindingBase, updateSourceEventName);
				return;
			}

			NativeViewPropertyListener nativePropertyListener = null;
			if (bindingBase.Mode == BindingMode.TwoWay) {
				nativePropertyListener = new NativeViewPropertyListener(propertyName);
				view.AddObserver(nativePropertyListener, propertyName, 0, IntPtr.Zero);
			}

			NativeBindingHelpers.SetBinding(view, propertyName, bindingBase, nativePropertyListener);
		}

		public static void SetBinding(this UIView self, BindableProperty targetProperty, BindingBase binding)
		{
			NativeBindingHelpers.SetBinding(self, targetProperty, binding);
		}

		public static void SetValue(this UIView target, BindableProperty targetProperty, object value)
		{
			NativeBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this UIView target, object bindingContext, Func<UIView, IEnumerable<UIView>> getChildren = null)
		{
			NativeBindingHelpers.SetBindingContext(target, bindingContext, getChildren);
		}

		internal static void TransferbindablePropertiesToWrapper(this UIView target, View wrapper)
		{
			NativeBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}

		internal static T FindDescendantView<T>(this UIView view) where T : UIView
		{
			var queue = new Queue<UIView>();
			queue.Enqueue(view);

			while (queue.Count > 0)
			{
				var descendantView = queue.Dequeue();

				var result = descendantView as T;
				if (result != null)
					return result;

				for (var i = 0; i < descendantView.Subviews.Length; i++)
					queue.Enqueue(descendantView.Subviews[i]);
			}

			return null;
		}

		internal static UIView FindFirstResponder(this UIView view)
		{
			if (view.IsFirstResponder)
				return view;

			foreach (var subView in view.Subviews)
			{
				var firstResponder = subView.FindFirstResponder();
				if (firstResponder != null)
					return firstResponder;
			}

			return null;
		}
	}
}