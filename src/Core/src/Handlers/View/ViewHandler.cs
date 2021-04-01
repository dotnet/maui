using System;
using System.Drawing;
using System.Runtime.CompilerServices;
#if __IOS__
using NativeView = UIKit.UIView;
#elif __MACOS__
using NativeView = AppKit.NSView;
#elif MONOANDROID
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler
	{
		public static PropertyMapper<IView> ViewMapper = new PropertyMapper<IView>
		{
			[nameof(IView.BackgroundColor)] = MapBackgroundColor,
			[nameof(IView.Frame)] = MapFrame,
			[nameof(IView.IsEnabled)] = MapIsEnabled,
			[nameof(IView.AutomationId)] = MapAutomationId,
			[nameof(IView.Semantics)] = MapSemantics
		};

		public static void MapFrame(IViewHandler handler, IView view)
		{
			handler.SetFrame(view.Frame);
		}

		public static void MapIsEnabled(IViewHandler handler, IView view)
		{
			(handler.NativeView as NativeView)?.UpdateIsEnabled(view);
		}

		public static void MapBackgroundColor(IViewHandler handler, IView view)
		{
			(handler.NativeView as NativeView)?.UpdateBackgroundColor(view);
		}

		public static void MapAutomationId(IViewHandler handler, IView view)
		{
			(handler.NativeView as NativeView)?.UpdateAutomationId(view);
		}

		private static void MapSemantics(IViewHandler handler, IView view)
		{
#if MONOANDROID
			// Check if view needs an Accessibility delegate
			// Maybe there's a way to register a delegate here against 
			// Disconnect per the life cycle code
			if (!string.IsNullOrEmpty(view.Semantics.Hint))
			{
				var accessibilityDelegate = 
					AndroidX.Core.View.ViewCompat.GetAccessibilityDelegate(handler.NativeView as NativeView);
				if (accessibilityDelegate is MauiAccessibilityDelegate mad)
					mad.View = view;
				else if (accessibilityDelegate == null)
					AndroidX.Core.View.ViewCompat.SetAccessibilityDelegate(handler.NativeView as NativeView, new MauiAccessibilityDelegate(view));
			}
#endif
			(handler.NativeView as NativeView)?.UpdateSemantics(view);
		}

		private protected void Disconnect(IViewHandler handler, IView view)
		{
#if MONOANDROID
			AndroidX.Core.View.ViewCompat.SetAccessibilityDelegate(handler.NativeView as NativeView, null);
#endif
		}

		private protected void Connect(IViewHandler handler, IView view)
		{
		}
	}
}