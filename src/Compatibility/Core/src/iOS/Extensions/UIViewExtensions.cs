using System;
using System.Collections.Generic;
using System.Linq;
using static System.String;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
#if __MOBILE__
using UIKit;
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using UIView = AppKit.NSView;
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public static class UIViewExtensions
	{
#if __MOBILE__
		public static UIImage ConvertToImage(this UIView view)
		{
			if (!Forms.IsiOS10OrNewer)
			{
				UIGraphics.BeginImageContext(view.Frame.Size);
				view.Layer.RenderInContext(UIGraphics.GetCurrentContext());
				var image = UIGraphics.GetImageFromCurrentImageContext();
				UIGraphics.EndImageContext();
				return new UIImage(image.CGImage);
			}

			var imageRenderer = new UIGraphicsImageRenderer(view.Bounds.Size);

			return imageRenderer.CreateImage((a) =>
			{
				view.Layer.RenderInContext(a.CGContext);
			});
		}
#endif

		internal static T GetParentOfType<T>(this UIView view)
			where T : class
		{
			if (view is T t)
				return t;

			while (view != null)
			{
				T parent = view.Superview as T;
				if (parent != null)
					return parent;

				view = view.Superview;
			}

			return default(T);
		}

		public static IEnumerable<UIView> Descendants(this UIView self)
		{
			if (self.Subviews == null)
				return Enumerable.Empty<UIView>();
			return self.Subviews.Concat(self.Subviews.SelectMany(s => s.Descendants()));
		}

		internal static IEnumerable<UIView> DescendantsTree(this UIView self)
		{
			var children = self.Subviews;
			for (var i = 0; i < children.Length; i++)
			{
				UIView child = children[i];
				yield return child;
				foreach (var grandChild in child.DescendantsTree())
				{
					yield return grandChild;
				}
			}
		}

		public static SizeRequest GetSizeRequest(this UIView self, double widthConstraint, double heightConstraint,
			double minimumWidth = -1, double minimumHeight = -1)
		{
			CoreGraphics.CGSize s;
#if __MOBILE__
			s = self.SizeThatFits(new SizeF((float)widthConstraint, (float)heightConstraint));
#else
			var control = self as AppKit.NSControl;
			if (control != null)
				s = control.SizeThatFits(new CoreGraphics.CGSize(widthConstraint, heightConstraint));
			else
				s = self.FittingSize;
#endif
			var request = new Size(s.Width == float.PositiveInfinity ? double.PositiveInfinity : s.Width,
				s.Height == float.PositiveInfinity ? double.PositiveInfinity : s.Height);
			var minimum = new Size(minimumWidth < 0 ? request.Width : minimumWidth,
				minimumHeight < 0 ? request.Height : minimumHeight);
			return new SizeRequest(request, minimum);
		}

		public static void SetBinding(this UIView view, string propertyName, BindingBase bindingBase,
			string updateSourceEventName = null)
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
			if (bindingBase.Mode == BindingMode.TwoWay)
			{
				nativePropertyListener = new NativeViewPropertyListener(propertyName);
				try
				{
					//TODO: We need to figure a way to map the value back to the real objectiveC property.
					//the X.IOS camelcase property name won't work
					var key = new Foundation.NSString(propertyName.ToLower());
					var valueKey = view.ValueForKey(key);
					if (valueKey != null)
					{
						view.AddObserver(nativePropertyListener, key, Foundation.NSKeyValueObservingOptions.New, IntPtr.Zero);
					}
				}
#if __MOBILE__
				catch (Foundation.MonoTouchException ex)
				{
					nativePropertyListener = null;
					if (ex.Name == "NSUnknownKeyException")
					{
						System.Diagnostics.Debug.WriteLine("KVO not supported, try specify a UpdateSourceEventName instead.");
						return;
					}
					throw ex;
				}
#else
				catch (Exception ex)
				{
					throw ex;
				}
#endif
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

		public static void SetBindingContext(this UIView target, object bindingContext,
			Func<UIView, IEnumerable<UIView>> getChildren = null)
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

				for (var i = 0; i < descendantView.Subviews?.Length; i++)
					queue.Enqueue(descendantView.Subviews[i]);
			}

			return null;
		}

#if __MOBILE__
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
#endif
	}
}