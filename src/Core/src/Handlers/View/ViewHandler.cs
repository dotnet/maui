using Microsoft.Maui.Graphics;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
#elif __ANDROID__
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD
using PlatformView = System.Object;
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
				[nameof(IBorder.Border)] = MapBorderView,
#if ANDROID || WINDOWS
				[nameof(IToolbarElement.Toolbar)] = MapToolbar,
#endif
			};

		public static CommandMapper<IView, IViewHandler> ViewCommandMapper = new()
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
				if(VirtualView is IBorderView border)
					return border?.Shape != null || border?.Stroke != null;
				
				return false;
#else
				return VirtualView?.Clip != null || VirtualView?.Shadow != null || (VirtualView as IBorder)?.Border != null;
#endif
			}
		}

		public PlatformView? ContainerView { get; private protected set; }

		object? IViewHandler.ContainerView => ContainerView;

		public new PlatformView? PlatformView
		{
			get => (PlatformView?)base.PlatformView;
			private protected set => base.PlatformView = value;
		}

		public new IView? VirtualView
		{
			get => (IView?)base.VirtualView;
			private protected set => base.VirtualView = value;
		}

		public abstract Size GetDesiredSize(double widthConstraint, double heightConstraint);

		public abstract void PlatformArrange(Rectangle frame);

		private protected abstract PlatformView OnCreatePlatformView();

		private protected sealed override object OnCreatePlatformElement() =>
			OnCreatePlatformView();

#if ANDROID
		// This sets up AndroidBatchPropertyMapper
		public override void SetVirtualView(IElement element)
		{
			base.SetVirtualView(element);

			if (element is IView view)
			{
				((PlatformView?)PlatformView)?.Initialize(view);
			}
		}
#endif

#if !NETSTANDARD
		private protected abstract void OnConnectHandler(PlatformView nativeView);

		partial void ConnectingHandler(PlatformView? nativeView);

		private protected sealed override void OnConnectHandler(object nativeView)
		{
			ConnectingHandler((PlatformView)nativeView);
			OnConnectHandler((PlatformView)nativeView);
		}

		private protected abstract void OnDisconnectHandler(PlatformView nativeView);

		partial void DisconnectingHandler(PlatformView nativeView);

		private protected sealed override void OnDisconnectHandler(object nativeView)
		{
			DisconnectingHandler((PlatformView)nativeView);
			OnDisconnectHandler((PlatformView)nativeView);
		}
#endif

		public static void MapWidth(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateWidth(view);
		}

		public static void MapHeight(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateHeight(view);
		}

		public static void MapMinimumHeight(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateMinimumHeight(view);
		}

		public static void MapMaximumHeight(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateMaximumHeight(view);
		}

		public static void MapMinimumWidth(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateMinimumWidth(view);
		}

		public static void MapMaximumWidth(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateMaximumWidth(view);
		}

		public static void MapIsEnabled(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateIsEnabled(view);
		}

		public static void MapVisibility(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateVisibility(view);
		}

		public static void MapBackground(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateBackground(view);
		}

		public static void MapFlowDirection(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateFlowDirection(view);
		}

		public static void MapOpacity(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateOpacity(view);
		}

		public static void MapAutomationId(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateAutomationId(view);
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

			((PlatformView?)handler.ContainerView)?.UpdateClip(view);
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

 			((PlatformView?)handler.ContainerView)?.UpdateShadow(view);
		}

		static partial void MappingSemantics(IViewHandler handler, IView view);

		public static void MapSemantics(IViewHandler handler, IView view)
		{
			MappingSemantics(handler, view);
			((PlatformView?)handler.PlatformView)?.UpdateSemantics(view);
		}

		public static void MapInvalidateMeasure(IViewHandler handler, IView view, object? args)
		{
			(handler.PlatformView as PlatformView)?.InvalidateMeasure(view);
		}

		public static void MapContainerView(IViewHandler handler, IView view)
		{
			if (handler is ViewHandler viewHandler)
				handler.HasContainer = viewHandler.NeedsContainer;
		}

		public static void MapBorderView(IViewHandler handler, IView view)
		{
			var border = (view as IBorder)?.Border;

			if (border != null)
			{
				handler.HasContainer = true;
			}
			else
			{
				if (handler is ViewHandler viewHandler)
					handler.HasContainer = viewHandler.NeedsContainer;
			}

 			((PlatformView?)handler.ContainerView)?.UpdateBorder(view);
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
