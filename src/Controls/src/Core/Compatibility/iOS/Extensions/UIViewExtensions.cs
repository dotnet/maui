#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using static System.String;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using System.Diagnostics.CodeAnalysis;

#if __MOBILE__
using ObjCRuntime;
using UIKit;
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using UIView = AppKit.NSView;
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public static class UIViewExtensions
	{
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

		[Obsolete("Use IView.Measure or just call SizeThatFits directly on the UIView")]
		public static SizeRequest GetSizeRequest(this UIView self, double widthConstraint, double heightConstraint,
			double minimumWidth = -1, double minimumHeight = -1)
		{
			var s = self.SizeThatFits(new SizeF((float)widthConstraint, (float)heightConstraint));
			var request = new Size(s.Width == float.PositiveInfinity ? double.PositiveInfinity : s.Width,
				s.Height == float.PositiveInfinity ? double.PositiveInfinity : s.Height);
			var minimum = new Size(minimumWidth < 0 ? request.Width : minimumWidth,
				minimumHeight < 0 ? request.Height : minimumHeight);
			return new SizeRequest(request, minimum);
		}

		[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
		public static void SetBinding(this UIView view, string propertyName, BindingBase bindingBase,
			string updateSourceEventName = null)
		{
			var binding = bindingBase as Binding;
			//This will allow setting bindings from Xaml by reusing the MarkupExtension
			updateSourceEventName = updateSourceEventName ?? binding?.UpdateSourceEventName;

			if (!IsNullOrEmpty(updateSourceEventName))
			{
				PlatformBindingHelpers.SetBinding(view, propertyName, bindingBase, updateSourceEventName);
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
					var key = new Foundation.NSString(propertyName.ToLowerInvariant());
					var valueKey = view.ValueForKey(key);
					if (valueKey != null)
					{
						view.AddObserver(nativePropertyListener, key, Foundation.NSKeyValueObservingOptions.New, IntPtr.Zero);
					}
				}
				catch (ObjCRuntime.ObjCException ex) when (ex.Name == "NSUnknownKeyException")
				{
					nativePropertyListener = null;
					System.Diagnostics.Debug.WriteLine("KVO not supported, try specify a UpdateSourceEventName instead.");
					return;
				}
			}

			PlatformBindingHelpers.SetBinding(view, propertyName, bindingBase, nativePropertyListener);
		}

		public static void SetBinding(this UIView self, BindableProperty targetProperty, BindingBase binding)
		{
			PlatformBindingHelpers.SetBinding(self, targetProperty, binding);
		}

		public static void SetValue(this UIView target, BindableProperty targetProperty, object value)
		{
			PlatformBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this UIView target, object bindingContext,
			Func<UIView, IEnumerable<UIView>> getChildren = null)
		{
			PlatformBindingHelpers.SetBindingContext(target, bindingContext, getChildren);
		}

		internal static void TransferbindablePropertiesToWrapper(this UIView target, View wrapper)
		{
			PlatformBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
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