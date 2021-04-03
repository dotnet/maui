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
	public abstract partial class ViewHandler : IViewHandler
	{
		public static PropertyMapper<IView> ViewMapper = new PropertyMapper<IView>
		{
			[nameof(IView.BackgroundColor)] = MapBackgroundColor,
			[nameof(IView.Frame)] = MapFrame,
			[nameof(IView.IsEnabled)] = MapIsEnabled,
			[nameof(IView.AutomationId)] = MapAutomationId,
			[nameof(IView.Semantics)] = MapSemantics
		};

		bool _hasContainer;

		public bool HasContainer
		{
			get => _hasContainer;
			set
			{
				if (_hasContainer == value)
					return;

				_hasContainer = value;

				if (value)
					SetupContainer();
				else
					RemoveContainer();
			}
		}

		protected abstract void SetupContainer();

		protected abstract void RemoveContainer();

		public IMauiContext? MauiContext { get; private set; }

		public NativeView? NativeView
		{
			get;
			private protected set;
		}

		public IView? VirtualView 
		{ 
			get; 
			private protected set; 
		}

		public void SetMauiContext(IMauiContext mauiContext) => MauiContext = mauiContext;

		public abstract void SetVirtualView(IView view);

		public abstract void UpdateValue(string property);

		void IViewHandler.DisconnectHandler()
		{
			if (NativeView != null && VirtualView != null)
				DisconnectHandlerCore(NativeView);

			if (VirtualView != null)
				VirtualView.Handler = null;

			VirtualView = null;
		}

		public abstract Size GetDesiredSize(double widthConstraint, double heightConstraint);

		public abstract void SetFrame(Rectangle frame);

		private protected virtual void ConnectHandlerCore(NativeView nativeView)
		{
		}

#if !MONOANDROID
		private protected virtual void DisconnectHandlerCore(NativeView nativeView)
		{
		}
#endif

		public static void MapFrame(IViewHandler handler, IView view)
		{
			handler.SetFrame(view.Frame);
		}

		public static void MapIsEnabled(IViewHandler handler, IView view)
		{
			handler.NativeView?.UpdateIsEnabled(view);
		}

		public static void MapBackgroundColor(IViewHandler handler, IView view)
		{
			handler.NativeView?.UpdateBackgroundColor(view);
		}

		public static void MapAutomationId(IViewHandler handler, IView view)
		{
			handler.NativeView?.UpdateAutomationId(view);
		}

#if !MONOANDROID
		public static void MapSemantics(IViewHandler handler, IView view)
		{
			handler.NativeView?.UpdateSemantics(view);
		}
#endif
	}
}