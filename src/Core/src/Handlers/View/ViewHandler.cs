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
	public abstract partial class ViewHandler : IViewHandler
	{
		public static PropertyMapper<IView> ViewMapper = new PropertyMapper<IView>
		{
			[nameof(IView.AutomationId)] = MapAutomationId,
			[nameof(IView.BackgroundColor)] = MapBackgroundColor,
			[nameof(IView.Width)] = MapWidth,
			[nameof(IView.Height)] = MapHeight,
			[nameof(IView.IsEnabled)] = MapIsEnabled,
			[nameof(IView.Semantics)] = MapSemantics,
			[nameof(IView.TranslationX)] = MapTranslationX,
			[nameof(IView.TranslationY)] = MapTranslationY,
			[nameof(IView.Scale)] = MapScale,
			[nameof(IView.ScaleX)] = MapScale,
			[nameof(IView.ScaleY)] = MapScale,
			[nameof(IView.Rotation)] = MapRotation,
			[nameof(IView.RotationX)] = MapRotationX,
			[nameof(IView.RotationY)] = MapRotationY,
			[nameof(IView.AnchorX)] = MapAnchorX,
			[nameof(IView.AnchorY)] = MapAnchorY,
			Actions =
			{
				[nameof(IFrameworkElement.InvalidateMeasure)] = MapInvalidateMeasure
			}
		};

		internal ViewHandler()
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

		public IView? VirtualView { get; private protected set; }

		public void SetMauiContext(IMauiContext mauiContext) => MauiContext = mauiContext;

		public abstract void SetVirtualView(IView view);

		public abstract void UpdateValue(string property);

		void IViewHandler.DisconnectHandler() => DisconnectHandler(((NativeView?)NativeView));

		public abstract Size GetDesiredSize(double widthConstraint, double heightConstraint);

		public abstract void NativeArrange(Rectangle frame);

		partial void ConnectingHandler(NativeView? nativeView);

		private protected void ConnectHandler(NativeView? nativeView)
		{
			ConnectingHandler(nativeView);
		}

		partial void DisconnectingHandler(NativeView? nativeView);

		private protected void DisconnectHandler(NativeView? nativeView)
		{
			DisconnectingHandler(nativeView);

			if (VirtualView != null)
				VirtualView.Handler = null;

			VirtualView = null;
		}

		public static void MapWidth(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateWidth(view);
		}

		public static void MapHeight(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateHeight(view);
		}

		public static void MapIsEnabled(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateIsEnabled(view);
		}

		public static void MapBackgroundColor(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateBackgroundColor(view);
		}

		public static void MapAutomationId(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateAutomationId(view);
		}

		static partial void MappingSemantics(IViewHandler handler, IView view);

		public static void MapSemantics(IViewHandler handler, IView view)
		{
			MappingSemantics(handler, view);
			((NativeView?)handler.NativeView)?.UpdateSemantics(view);
		}

		public static void MapInvalidateMeasure(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.InvalidateMeasure(view);
		}
	}
}
