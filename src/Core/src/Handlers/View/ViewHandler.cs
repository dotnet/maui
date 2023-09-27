using Microsoft.Maui.Graphics;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
#elif __ANDROID__
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM)
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
#if ANDROID || WINDOWS || TIZEN
				[nameof(IToolbarElement.Toolbar)] = MapToolbar,
#endif
				[nameof(IView.InputTransparent)] = MapInputTransparent,
				[nameof(IToolTipElement.ToolTip)] = MapToolTip,
#if WINDOWS || MACCATALYST
				[nameof(IContextFlyoutElement.ContextFlyout)] = MapContextFlyout,
#endif

#if ANDROID
				["_InitializeBatchedProperties"] = MapInitializeBatchedProperties
#endif
			};

		public static CommandMapper<IView, IViewHandler> ViewCommandMapper = new()
		{
			[nameof(IView.InvalidateMeasure)] = MapInvalidateMeasure,
			[nameof(IView.Frame)] = MapFrame,
			[nameof(IView.ZIndex)] = MapZIndex,
			[nameof(IView.Focus)] = MapFocus,
			[nameof(IView.Unfocus)] = MapUnfocus,
		};

		bool _hasContainer;

		internal DataFlowDirection DataFlowDirection { get; set; }

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

		public virtual bool NeedsContainer
		{
			get => VirtualView.NeedsContainer();
		}

		protected abstract void SetupContainer();

		protected abstract void RemoveContainer();

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

		public abstract void PlatformArrange(Rect frame);

		private protected abstract PlatformView OnCreatePlatformView();

		private protected sealed override object OnCreatePlatformElement() =>
			OnCreatePlatformView();

#if ANDROID

		static void MapInitializeBatchedProperties(IViewHandler handler, IView view)
		{
			if (handler.PlatformView is PlatformView pv)
				pv.Initialize(view);
		}
#endif


#if !(NETSTANDARD || !PLATFORM)
		private protected abstract void OnConnectHandler(PlatformView platformView);

		partial void ConnectingHandler(PlatformView? platformView);

		private protected sealed override void OnConnectHandler(object platformView)
		{
			ConnectingHandler((PlatformView)platformView);
			OnConnectHandler((PlatformView)platformView);
		}

		private protected abstract void OnDisconnectHandler(PlatformView platformView);

		partial void DisconnectingHandler(PlatformView platformView);

		private protected sealed override void OnDisconnectHandler(object platformView)
		{
			DisconnectingHandler((PlatformView)platformView);
			OnDisconnectHandler((PlatformView)platformView);
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
			if (handler.HasContainer)
				((PlatformView?)handler.ContainerView)?.UpdateVisibility(view);

			((PlatformView?)handler.PlatformView)?.UpdateVisibility(view);
		}

		public static void MapBackground(IViewHandler handler, IView view)
		{
			if (handler.PlatformView is not PlatformView platformView)
				return;

			if (view.Background is ImageSourcePaint image)
			{
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

				platformView.UpdateBackgroundImageSourceAsync(image.ImageSource, provider)
					.FireAndForget(handler);
			}
			else
			{
				platformView.UpdateBackground(view);
			}
		}

		public static void MapFlowDirection(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateFlowDirection(view);
		}

		public static void MapOpacity(IViewHandler handler, IView view)
		{
			if (handler.HasContainer)
			{
				((PlatformView?)handler.ContainerView)?.UpdateOpacity(view);
				//We don't want the control opacity to be reduced by the container one, so we always set 100% to the control if it has a container
				((PlatformView?)handler.PlatformView)?.UpdateOpacity(1);
			}
			else
				((PlatformView?)handler.PlatformView)?.UpdateOpacity(view);
		}

		public static void MapAutomationId(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateAutomationId(view);
		}

		public static void MapClip(IViewHandler handler, IView view)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			((PlatformView?)handler.ContainerView)?.UpdateClip(view);
		}

		public static void MapShadow(IViewHandler handler, IView view)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

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
			else
				handler.HasContainer = view.NeedsContainer();
		}

		public static void MapBorderView(IViewHandler handler, IView view)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

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

		public static void MapFocus(IViewHandler handler, IView view, object? args)
		{
			if (args is not FocusRequest request)
				return;

			((PlatformView?)handler.PlatformView)?.Focus(request);
		}

		public static void MapInputTransparent(IViewHandler handler, IView view)
		{
#if ANDROID
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			if (handler.ContainerView is WrapperView wrapper)
				wrapper.InputTransparent = view.InputTransparent;
#else

#if IOS || MACCATALYST
			// Containers on iOS/Mac Catalyst may be hit testable, so we need to
			// propagate the the view's values to its container view.
			if (handler.ContainerView is WrapperView wrapper)
				wrapper.UpdateInputTransparent(handler, view);
#endif

			((PlatformView?)handler.PlatformView)?.UpdateInputTransparent(handler, view);
#endif
		}

		public static void MapUnfocus(IViewHandler handler, IView view, object? args)
		{
			((PlatformView?)handler.PlatformView)?.Unfocus(view);
		}

		public static void MapToolTip(IViewHandler handler, IView view)
		{
#if PLATFORM
			if (view is IToolTipElement tooltipContainer)
				handler.ToPlatform().UpdateToolTip(tooltipContainer.ToolTip);
#endif
		}
	}
}
