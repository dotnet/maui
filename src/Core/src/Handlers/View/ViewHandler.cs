using Microsoft.Maui.Graphics;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIView;
#elif __ANDROID__
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler : ElementHandler, IViewHandler
	{
		public static IPropertyMapper<IView, IViewHandler> ViewMapper =
#if ANDROID
			// Use a custom mapper for Android which knows how to batch the initial property sets
			new AndroidBatchPropertyMapper<IView, IViewHandler>(ElementMapper)
#else
			new PropertyMapper<IView, IViewHandler>(ElementHandler.ElementMapper)
#endif
			{
				[nameof(IView.AutomationId)] = MapAutomationId,
				[nameof(IView.Clip)] = MapClip,
				[nameof(IView.Shadow)] = MapShadow,
				[nameof(IView.Visibility)] = MapVisibility,
				[nameof(IView.Background)] = MapBackground,
				[nameof(IView.FlowDirection)] = MapFlowDirection,
				[nameof(IView.Width)] = MapWidth,
				[nameof(IView.Height)] = MapHeight,
				[nameof(IView.MinimumHeight)] = MapMinimumHeight,
				[nameof(IView.MaximumHeight)] = MapMaximumHeight,
				[nameof(IView.MinimumWidth)] = MapMinimumWidth,
				[nameof(IView.MaximumWidth)] = MapMaximumWidth,
				[nameof(IView.IsEnabled)] = MapIsEnabled,
				[nameof(IView.Opacity)] = MapOpacity,
				[nameof(IView.Semantics)] = MapSemantics,
				[nameof(IView.TranslationX)] = MapTranslationX,
				[nameof(IView.TranslationY)] = MapTranslationY,
				[nameof(IView.Scale)] = MapScale,
				[nameof(IView.ScaleX)] = MapScaleX,
				[nameof(IView.ScaleY)] = MapScaleY,
				[nameof(IView.Rotation)] = MapRotation,
				[nameof(IView.RotationX)] = MapRotationX,
				[nameof(IView.RotationY)] = MapRotationY,
				[nameof(IView.AnchorX)] = MapAnchorX,
				[nameof(IView.AnchorY)] = MapAnchorY,
				[nameof(IViewHandler.ContainerView)] = MapContainerView,
#if ANDROID || WINDOWS
			[nameof(IToolbarElement.Toolbar)] = MapToolbar,
#endif
			};

		public static CommandMapper<IView, ViewHandler> ViewCommandMapper = new()
		{
			[nameof(IView.InvalidateMeasure)] = MapInvalidateMeasure,
			[nameof(IView.Frame)] = MapFrame,
			[nameof(IView.ZIndex)] = MapZIndex,
		};

		bool _hasContainer;

		protected ViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper ?? ViewCommandMapper)
		{
		}

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

		public virtual bool NeedsContainer
		{
			get
			{
#if WINDOWS
				if(VirtualView is IBorder border)
					return border?.Shape != null || border?.Stroke != null;
				
				return false;
#else
				return VirtualView?.Clip != null || VirtualView?.Shadow != null;
#endif
			}
		}

		public NativeView? ContainerView { get; private protected set; }

		object? IViewHandler.ContainerView => ContainerView;

		public new NativeView? NativeView
		{
			get => (NativeView?)base.NativeView;
			private protected set => base.NativeView = value;
		}

		public new IView? VirtualView
		{
			get => (IView?)base.VirtualView;
			private protected set => base.VirtualView = value;
		}

		public abstract Size GetDesiredSize(double widthConstraint, double heightConstraint);

		public abstract void NativeArrange(Rectangle frame);

		private protected abstract NativeView OnCreateNativeView();

		private protected sealed override object OnCreateNativeElement() =>
			OnCreateNativeView();

#if ANDROID
		// This sets up AndroidBatchPropertyMapper
		public override void SetVirtualView(IElement element)
		{
			base.SetVirtualView(element);

			if (element is IView view)
			{
				((NativeView?)NativeView)?.Initialize(view);
			}
		}
#endif

#if !NETSTANDARD
		private protected abstract void OnConnectHandler(NativeView nativeView);

		partial void ConnectingHandler(NativeView? nativeView);

		private protected sealed override void OnConnectHandler(object nativeView)
		{
			ConnectingHandler((NativeView)nativeView);
			OnConnectHandler((NativeView)nativeView);
		}

		private protected abstract void OnDisconnectHandler(NativeView nativeView);

		partial void DisconnectingHandler(NativeView nativeView);

		private protected sealed override void OnDisconnectHandler(object nativeView)
		{
			DisconnectingHandler((NativeView)nativeView);
			OnDisconnectHandler((NativeView)nativeView);
		}
#endif

		public static void MapWidth(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateWidth(view);
		}

		public static void MapHeight(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateHeight(view);
		}

		public static void MapMinimumHeight(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateMinimumHeight(view);
		}

		public static void MapMaximumHeight(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateMaximumHeight(view);
		}

		public static void MapMinimumWidth(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateMinimumWidth(view);
		}

		public static void MapMaximumWidth(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateMaximumWidth(view);
		}

		public static void MapIsEnabled(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateIsEnabled(view);
		}

		public static void MapVisibility(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateVisibility(view);
		}

		public static void MapBackground(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateBackground(view);
		}

		public static void MapFlowDirection(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateFlowDirection(view);
		}

		public static void MapOpacity(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateOpacity(view);
		}

		public static void MapAutomationId(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateAutomationId(view);
		}

		public static void MapClip(IViewHandler handler, IView view)
		{
			var clipShape = view.Clip;

			if (clipShape != null)
			{
				handler.HasContainer = true;
			}
			else
			{
				if (handler is ViewHandler viewHandler)
					handler.HasContainer = viewHandler.NeedsContainer;
			}

			((NativeView?)handler.ContainerView)?.UpdateClip(view);
		}

		public static void MapShadow(IViewHandler handler, IView view)
		{
			var shadow = view.Shadow;

			if (shadow != null)
			{
				handler.HasContainer = true;
			}
			else
			{
				if (handler is ViewHandler viewHandler)
					handler.HasContainer = viewHandler.NeedsContainer;
			}

 			((NativeView?)handler.ContainerView)?.UpdateShadow(view);
		}

		static partial void MappingSemantics(IViewHandler handler, IView view);

		public static void MapSemantics(IViewHandler handler, IView view)
		{
			MappingSemantics(handler, view);
			((NativeView?)handler.NativeView)?.UpdateSemantics(view);
		}

		public static void MapInvalidateMeasure(IViewHandler handler, IView view, object? args)
		{
			(handler.NativeView as NativeView)?.InvalidateMeasure(view);
		}

		public static void MapContainerView(IViewHandler handler, IView view)
		{
			if (handler is ViewHandler viewHandler)
				handler.HasContainer = viewHandler.NeedsContainer;
		}

		static partial void MappingFrame(IViewHandler handler, IView view);

		public static void MapFrame(IViewHandler handler, IView view, object? args)
		{
			MappingFrame(handler, view);
		}

		public static void MapZIndex(IViewHandler handler, IView view, object? args)
		{
			if (view.Parent is ILayout layout)
			{
				layout.Handler?.Invoke(nameof(ILayoutHandler.UpdateZIndex), view);
			}
		}
	}
}
