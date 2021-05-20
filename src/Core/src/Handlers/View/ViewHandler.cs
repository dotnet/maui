#nullable enable
using Microsoft.Maui.Graphics;
using System;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIView;
#elif MONOANDROID
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public abstract partial class FrameworkElementHandler : IFrameworkElementHandler
	{
		public static PropertyMapper<IFrameworkElement> FrameworkElementMapper = new PropertyMapper<IFrameworkElement>
		{
			[nameof(IFrameworkElement.AutomationId)] = MapAutomationId,
			[nameof(IFrameworkElement.Background)] = MapBackground,
			[nameof(IFrameworkElement.IsEnabled)] = MapIsEnabled,
			[nameof(IFrameworkElement.Semantics)] = MapSemantics,
			Actions = {
					[nameof(IFrameworkElement.InvalidateMeasure)] = MapInvalidateMeasure
				}
		};

		internal FrameworkElementHandler()
		{
		}

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

		public IServiceProvider? Services => MauiContext?.Services;

		public object? NativeView { get; private protected set; }

		public IFrameworkElement? VirtualView { get; private protected set; }

		public void SetMauiContext(IMauiContext mauiContext) => MauiContext = mauiContext;

		public abstract void SetVirtualView(IFrameworkElement view);

		public abstract void UpdateValue(string property);

		void IFrameworkElementHandler.DisconnectHandler() => DisconnectHandler(((NativeView?)NativeView));

		public abstract Size GetDesiredSize(double widthConstraint, double heightConstraint);

		public abstract void NativeArrange(Rectangle frame);

		private protected void ConnectHandler(NativeView? nativeView)
		{
		}

		partial void DisconnectingHandler(NativeView? nativeView);

		private protected void DisconnectHandler(NativeView? nativeView)
		{
			DisconnectingHandler(nativeView);

			if (VirtualView != null)
				VirtualView.Handler = null;

			VirtualView = null;
		}

		public static void MapIsEnabled(IFrameworkElementHandler handler, IFrameworkElement view)
		{
			((NativeView?)handler.NativeView)?.UpdateIsEnabled(view);
		}

		public static void MapBackground(IFrameworkElementHandler handler, IFrameworkElement view)
		{
			((NativeView?)handler.NativeView)?.UpdateBackground(view);
		}

		public static void MapAutomationId(IFrameworkElementHandler handler, IFrameworkElement view)
		{
			((NativeView?)handler.NativeView)?.UpdateAutomationId(view);
		}

		static partial void MappingSemantics(IFrameworkElementHandler handler, IFrameworkElement view);

		public static void MapSemantics(IFrameworkElementHandler handler, IFrameworkElement view)
		{
			MappingSemantics(handler, view);
			((NativeView?)handler.NativeView)?.UpdateSemantics(view);
		}

		public static void MapInvalidateMeasure(IFrameworkElementHandler handler, IFrameworkElement view)
		{
			((NativeView?)handler.NativeView)?.InvalidateMeasure(view);
		}
	}
}
