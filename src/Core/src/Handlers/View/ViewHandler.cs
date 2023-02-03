using System.Collections.Generic;
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
	public abstract partial class ViewHandler : ElementHandler, IViewHandler, INeedsContainerViewHandler
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
				[nameof(INeedsContainerViewHandler.NeedsContainer)] = MapNeedsContainer,
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
			};

		public static CommandMapper<IView, IViewHandler> ViewCommandMapper = new()
		{
			[nameof(IView.InvalidateMeasure)] = MapInvalidateMeasure,
			[nameof(IView.Frame)] = MapFrame,
			[nameof(IView.ZIndex)] = MapZIndex,
			[nameof(IView.Focus)] = MapFocus,
			[nameof(IView.Unfocus)] = MapUnfocus,
			[nameof(INeedsContainerViewHandler.NeedsContainer)] = MapNeedsContainer,
		};

		bool _hasContainer;

		internal DataFlowDirection DataFlowDirection { get; set; }

		protected ViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper ?? ViewCommandMapper)
		{
		}

		public ICollection<string> ChangesContainer { get; } = new HashSet<string>();

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

				UpdateValue(nameof(IViewHandler.ContainerView));
			}
		}

		public virtual bool NeedsContainer
		{
			get
			{
#if !TIZEN
				if (VirtualView?.Clip != null || VirtualView?.Shadow != null)
					return true;
#endif
#if ANDROID
				if (VirtualView?.InputTransparent == true)
					return true;
#endif
#if ANDROID || IOS
				if (VirtualView is IBorder border)
					return border.Border != null;
#endif
#if WINDOWS || TIZEN
				if (VirtualView is IBorderView border)
					return border?.Shape != null || border?.Stroke != null;
#endif
				return false;
			}
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
			handler.Invoke(nameof(INeedsContainerViewHandler.NeedsContainer), nameof(IView.Visibility));

			if (handler.HasContainer)
			{
				((PlatformView?)handler.ContainerView)?.UpdateVisibility(view);
				((PlatformView?)handler.PlatformView)?.UpdateVisibility(Visibility.Visible);
			}
			else
			{
				((PlatformView?)handler.PlatformView)?.UpdateVisibility(view);
			}
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
			handler.Invoke(nameof(INeedsContainerViewHandler.NeedsContainer), nameof(IView.Opacity));
			handler.ToPlatform()?.UpdateOpacity(view);
		}

		public static void MapAutomationId(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateAutomationId(view);
		}

		public static void MapClip(IViewHandler handler, IView view)
		{
			handler.Invoke(nameof(INeedsContainerViewHandler.NeedsContainer), nameof(IView.Clip));

			((PlatformView?)handler.ContainerView)?.UpdateClip(view);
		}

		public static void MapShadow(IViewHandler handler, IView view)
		{
			handler.Invoke(nameof(INeedsContainerViewHandler.NeedsContainer), nameof(IView.Shadow));

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

		public static void MapNeedsContainer(IViewHandler handler, IView view, object? args)
		{
			if (handler is ViewHandler viewHandler && args is string mapper)
				viewHandler.ChangesContainer.Add(mapper);

			handler.UpdateValue(nameof(INeedsContainerViewHandler.NeedsContainer));
		}

		public static void MapNeedsContainer(IViewHandler handler, IView view)
		{
			if (handler is INeedsContainerViewHandler ncvh)
				handler.HasContainer = ncvh.NeedsContainer;
		}

		public static void MapContainerView(IViewHandler handler, IView view)
		{
			if (handler is INeedsContainerViewHandler ncvh)
			{
				foreach (var map in ncvh.ChangesContainer)
					handler.UpdateValue(map);
			}
		}

		public static void MapBorderView(IViewHandler handler, IView view)
		{
			handler.Invoke(nameof(INeedsContainerViewHandler.NeedsContainer), nameof(IBorder.Border));

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
			if (args is FocusRequest request)
			{
				if (handler.PlatformView == null)
				{
					return;
				}

				((PlatformView?)handler.PlatformView)?.Focus(request);
			}
		}

		public static void MapInputTransparent(IViewHandler handler, IView view)
		{
#if ANDROID
			handler.Invoke(nameof(INeedsContainerViewHandler.NeedsContainer), nameof(IView.InputTransparent));
			((PlatformView?)handler.ContainerView)?.UpdateInputTransparent(view);
#else
			((PlatformView?)handler.PlatformView)?.UpdateInputTransparent(view);
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
