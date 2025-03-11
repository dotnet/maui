using System;
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
	/// <summary>
	/// Base class for handlers that manage views which implement <see cref="IView"/>.
	/// </summary>
	/// <remarks>Handlers map virtual views (.NET MAUI layer) to controls on each platform (iOS, Android, Windows, macOS, etc.), which are known as platform views.
	/// Handlers are also responsible for instantiating the underlying platform view, and mapping the cross-platform control API to the platform view API. </remarks>
	public abstract partial class ViewHandler : ElementHandler, IViewHandler
	{
		class ViewHandlerPropertyMapper : IPropertyMapper<IView, IViewHandler>
		{
			static IPropertyMapper Base => ElementHandler.ElementMapper;

			public void Add(string key, Action<IViewHandler, IView> action)
			{
				throw new NotImplementedException();
			}

			public IEnumerable<string> GetKeys()
			{
				// This property is a special one and needs to be set before other properties.
				yield return nameof(IViewHandler.ContainerView);
				yield return nameof(IView.AutomationId);
				yield return nameof(IView.Clip);
				yield return nameof(IView.Shadow);
				yield return nameof(IView.Visibility);
				yield return nameof(IView.Background);
				yield return nameof(IView.FlowDirection);
				yield return nameof(IView.Width);
				yield return nameof(IView.Height);
				yield return nameof(IView.MinimumHeight);
				yield return nameof(IView.MaximumHeight);
				yield return nameof(IView.MinimumWidth);
				yield return nameof(IView.MaximumWidth);
				yield return nameof(IView.IsEnabled);
				yield return nameof(IView.Opacity);
				yield return nameof(IView.Semantics);
				yield return nameof(IView.TranslationX);
				yield return nameof(IView.TranslationY);
				yield return nameof(IView.Scale);
				yield return nameof(IView.ScaleX);
				yield return nameof(IView.ScaleY);
				yield return nameof(IView.Rotation);
				yield return nameof(IView.RotationX);
				yield return nameof(IView.RotationY);
				yield return nameof(IView.AnchorX);
				yield return nameof(IView.AnchorY);
#pragma warning disable CS0618 // Type or member is obsolete
				yield return nameof(IBorder.Border);
#pragma warning restore CS0618 // Type or member is obsolete
#if ANDROID || WINDOWS || TIZEN
				yield return nameof(IToolbarElement.Toolbar);
#endif
				yield return nameof(IView.InputTransparent);
				yield return nameof(IToolTipElement.ToolTip);
#if WINDOWS || MACCATALYST
				yield return nameof(IContextFlyoutElement.ContextFlyout);
#endif
				
				foreach(var key in Base.GetKeys())
				{
					yield return key;
				}
			}

			public Action<IElementHandler, IElement>? GetProperty(string key)
			{
				throw new NotImplementedException();
			}

			public void UpdateProperties(IElementHandler elementHandler, IElement virtualView)
			{
				if (elementHandler is not IViewHandler viewHandler || virtualView is not IView view)
					return;

				// This property is a special one and needs to be set before other properties.
				MapContainerView(viewHandler, view);
				MapAutomationId(viewHandler, view);
				MapClip(viewHandler, view);
				MapShadow(viewHandler, view);
				MapVisibility(viewHandler, view);
				MapBackground(viewHandler, view);
				MapFlowDirection(viewHandler, view);
				MapWidth(viewHandler, view);
				MapHeight(viewHandler, view);
				MapMinimumHeight(viewHandler, view);
				MapMaximumHeight(viewHandler, view);
				MapMinimumWidth(viewHandler, view);
				MapMaximumWidth(viewHandler, view);
				MapIsEnabled(viewHandler, view);
				MapOpacity(viewHandler, view);
				MapSemantics(viewHandler, view);
				MapTranslationX(viewHandler, view);
				MapTranslationY(viewHandler, view);
				MapScale(viewHandler, view);
				MapScaleX(viewHandler, view);
				MapScaleY(viewHandler, view);
				MapRotation(viewHandler, view);
				MapRotationX(viewHandler, view);
				MapRotationY(viewHandler, view);
				MapAnchorX(viewHandler, view);
				MapAnchorY(viewHandler, view);
#pragma warning disable CS0618 // Type or member is obsolete
				MapBorderView(viewHandler, view);
#pragma warning restore CS0618 // Type or member is obsolete
#if ANDROID || WINDOWS || TIZEN
				MapToolbar(viewHandler, view);
#endif
				MapInputTransparent(viewHandler, view);
				MapToolTip(viewHandler, view);
#if WINDOWS || MACCATALYST
				MapContextFlyout(viewHandler, view);
#endif

				Base.UpdateProperties(elementHandler, virtualView);
			}

			public void UpdateProperty(IElementHandler elementHandler, IElement virtualView, string property)
			{
				if (elementHandler is not IViewHandler viewHandler || virtualView is not IView view)
					return;

				switch (property)
				{
					// This property is a special one and needs to be set before other properties.
					case nameof(IViewHandler.ContainerView): MapContainerView(viewHandler, view); break;
					case nameof(IView.AutomationId): MapAutomationId(viewHandler, view); break;
					case nameof(IView.Clip): MapClip(viewHandler, view); break;
					case nameof(IView.Shadow): MapShadow(viewHandler, view); break;
					case nameof(IView.Visibility): MapVisibility(viewHandler, view); break;
					case nameof(IView.Background): MapBackground(viewHandler, view); break;
					case nameof(IView.FlowDirection): MapFlowDirection(viewHandler, view); break;
					case nameof(IView.Width): MapWidth(viewHandler, view); break;
					case nameof(IView.Height): MapHeight(viewHandler, view); break;
					case nameof(IView.MinimumHeight): MapMinimumHeight(viewHandler, view); break;
					case nameof(IView.MaximumHeight): MapMaximumHeight(viewHandler, view); break;
					case nameof(IView.MinimumWidth): MapMinimumWidth(viewHandler, view); break;
					case nameof(IView.MaximumWidth): MapMaximumWidth(viewHandler, view); break;
					case nameof(IView.IsEnabled): MapIsEnabled(viewHandler, view); break;
					case nameof(IView.Opacity): MapOpacity(viewHandler, view); break;
					case nameof(IView.Semantics): MapSemantics(viewHandler, view); break;
					case nameof(IView.TranslationX): MapTranslationX(viewHandler, view); break;
					case nameof(IView.TranslationY): MapTranslationY(viewHandler, view); break;
					case nameof(IView.Scale): MapScale(viewHandler, view); break;
					case nameof(IView.ScaleX): MapScaleX(viewHandler, view); break;
					case nameof(IView.ScaleY): MapScaleY(viewHandler, view); break;
					case nameof(IView.Rotation): MapRotation(viewHandler, view); break;
					case nameof(IView.RotationX): MapRotationX(viewHandler, view); break;
					case nameof(IView.RotationY): MapRotationY(viewHandler, view); break;
					case nameof(IView.AnchorX): MapAnchorX(viewHandler, view); break;
					case nameof(IView.AnchorY): MapAnchorY(viewHandler, view); break;
	#pragma warning disable CS0618 // Type or member is obsolete
					case nameof(IBorder.Border): MapBorderView(viewHandler, view); break;
	#pragma warning restore CS0618 // Type or member is obsolete
	#if ANDROID || WINDOWS || TIZEN
					case nameof(IToolbarElement.Toolbar): MapToolbar(viewHandler, view); break;
	#endif
					case nameof(IView.InputTransparent): MapInputTransparent(viewHandler, view); break;
					case nameof(IToolTipElement.ToolTip): MapToolTip(viewHandler, view); break;
	#if WINDOWS || MACCATALYST
					case nameof(IContextFlyoutElement.ContextFlyout): MapContextFlyout(viewHandler, view); break;
	#endif
					default: Base.UpdateProperty(elementHandler, virtualView, property); break;
				}
			}
		}

		/// <summary>
		/// A dictionary that maps the virtual view properties to their platform view counterparts.
		/// </summary>
		public static IPropertyMapper<IView, IViewHandler> ViewMapper = 
			// new ViewHandlerPropertyMapper();
//*
#if ANDROID
			// Use a custom mapper for Android which knows how to batch the initial property sets
			new AndroidBatchPropertyMapper<IView, IViewHandler>(ElementHandler.ElementMapper)
#else
			new PropertyMapper<IView, IViewHandler>(ElementHandler.ElementMapper)
#endif
			{
				// This property is a special one and needs to be set before other properties.
				[nameof(IViewHandler.ContainerView)] = MapContainerView,
#if ANDROID
				[AndroidBatchPropertyMapper.InitializeBatchedPropertiesKey] = MapInitializeBatchedProperties,
#endif

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
#pragma warning disable CS0618 // Type or member is obsolete
				[nameof(IBorder.Border)] = MapBorderView,
#pragma warning restore CS0618 // Type or member is obsolete
#if ANDROID || WINDOWS || TIZEN
				[nameof(IToolbarElement.Toolbar)] = MapToolbar,
#endif
				[nameof(IView.InputTransparent)] = MapInputTransparent,
				[nameof(IToolTipElement.ToolTip)] = MapToolTip,
#if WINDOWS || MACCATALYST
				[nameof(IContextFlyoutElement.ContextFlyout)] = MapContextFlyout,
#endif
			};
// */

		/// <summary>
		/// A dictionary that maps the virtual view commands to their platform view counterparts.
		/// </summary>
		/// <remarks>The concept or a command mapper is very similar to the property mapper with
		/// the addition that you can provide extra data in the form of arguments with the command mapper.</remarks>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="ViewHandler"/> class.
		/// </summary>
		/// <param name="mapper">The default mapper to use for this handler.</param>
		/// <param name="commandMapper">The command mapper to use for this handler.</param>
		protected ViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper ?? ViewCommandMapper)
		{
		}

		/// <summary>
		/// Gets or sets a value that indicates whether the <see cref="PlatformView"/> is contained within a view.
		/// </summary>
		/// <remarks>When set to <see langword="true"/>, <see cref="SetupContainer"/> is called to setup the container view.
		/// When set to <see langword="false"/>, <see cref="RemoveContainer"/> is called to remove the current container view.</remarks>
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

		/// <summary>
		/// Gets a value that indicates whether or not the <see cref="VirtualView"/> needs a container view.
		/// </summary>
		public virtual bool NeedsContainer
		{
			get => VirtualView.NeedsContainer();
		}

		/// <summary>
		/// Constructs the <see cref="ContainerView"/> and adds <see cref="PlatformView"/> to a container.
		/// </summary>
		/// <remarks>This method is called when <see cref="HasContainer"/> is set to <see langword="true"/>.</remarks>
		protected abstract void SetupContainer();

		/// <summary>
		/// Deconstructs the <see cref="ContainerView"/> and removes <see cref="PlatformView"/> from its container. 
		/// </summary>
		/// <remarks>This method is called when <see cref="HasContainer"/> is set to <see langword="false"/>.</remarks>
		protected abstract void RemoveContainer();

		/// <summary>
		/// Gets the view that acts as a container for the <see cref="PlatformView"/>.
		/// </summary>
		/// <remarks>Note that this can be <see langword="null"/>. Especially when <see cref="HasContainer"/> is set to <see langword="false"/> this value might not be set.</remarks>
		public PlatformView? ContainerView { get; private protected set; }

		object? IViewHandler.ContainerView => ContainerView;

		/// <summary>
		/// Gets or sets the platform representation of the view associated to this handler.
		/// </summary>
		/// <remarks>This property holds the reference to platform layer view, e.g. the iOS/macOS, Android or Windows view.
		/// The abstract (.NET MAUI) view is found in <see cref="VirtualView"/>.</remarks>
		public new PlatformView? PlatformView
		{
			get => (PlatformView?)base.PlatformView;
			private protected set => base.PlatformView = value;
		}

		/// <summary>
		/// Gets or sets the .NET MAUI repesentation of the view associated to this handler.
		/// </summary>
		/// <remarks>This property holds the reference to the abstract (.NET MAUI) view.
		/// The platform view is found in <see cref="PlatformView"/>.</remarks>
		public new IView? VirtualView
		{
			get => (IView?)base.VirtualView;
			private protected set => base.VirtualView = value;
		}

		/// <inheritdoc/>
		public abstract Size GetDesiredSize(double widthConstraint, double heightConstraint);

		/// <inheritdoc/>
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

		/// <summary>
		/// Maps the abstract <see cref="IView.Width"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapWidth(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateWidth(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Height"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapHeight(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateHeight(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.MinimumHeight"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapMinimumHeight(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateMinimumHeight(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.MaximumHeight"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapMaximumHeight(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateMaximumHeight(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.MinimumWidth"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapMinimumWidth(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateMinimumWidth(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.MaximumWidth"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapMaximumWidth(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateMaximumWidth(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.IsEnabled"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapIsEnabled(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateIsEnabled(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Visibility"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapVisibility(IViewHandler handler, IView view)
		{
			if (handler.HasContainer)
				((PlatformView?)handler.ContainerView)?.UpdateVisibility(view);

			((PlatformView?)handler.PlatformView)?.UpdateVisibility(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Background"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
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

		/// <summary>
		/// Maps the abstract <see cref="IView.FlowDirection"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapFlowDirection(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateFlowDirection(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Opacity"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
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

		/// <summary>
		/// Maps the abstract <see cref="IView.AutomationId"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapAutomationId(IViewHandler handler, IView view)
		{
			((PlatformView?)handler.PlatformView)?.UpdateAutomationId(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Clip"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapClip(IViewHandler handler, IView view)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			((PlatformView?)handler.ContainerView)?.UpdateClip(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Shadow"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapShadow(IViewHandler handler, IView view)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			((PlatformView?)handler.ContainerView)?.UpdateShadow(view);
		}

		static partial void MappingSemantics(IViewHandler handler, IView view);

		/// <summary>
		/// Maps the abstract <see cref="IView.Semantics"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapSemantics(IViewHandler handler, IView view)
		{
			MappingSemantics(handler, view);
			((PlatformView?)handler.PlatformView)?.UpdateSemantics(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.InvalidateMeasure"/> method to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		/// <param name="args">The arguments passed associated to this event.</param>
		public static void MapInvalidateMeasure(IViewHandler handler, IView view, object? args)
		{
			(handler.PlatformView as PlatformView)?.InvalidateMeasure(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IViewHandler.ContainerView"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapContainerView(IViewHandler handler, IView view)
		{
			bool hasContainerOldValue = handler.HasContainer;

			if (handler is ViewHandler viewHandler)
				handler.HasContainer = viewHandler.NeedsContainer;
			else
				handler.HasContainer = view.NeedsContainer();

			if (hasContainerOldValue != handler.HasContainer)
			{
				handler.UpdateValue(nameof(IView.Visibility));

#if WINDOWS
				handler.UpdateValue(nameof(IView.Opacity));
#endif
			}
		}

#pragma warning disable CS0618 // Type or member is obsolete
		/// <summary>
		/// Maps the abstract <see cref="IBorder.Border"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapBorderView(IViewHandler handler, IView view)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			((PlatformView?)handler.ContainerView)?.UpdateBorder(view);
		}
#pragma warning restore CS0618 // Type or member is obsolete

		static partial void MappingFrame(IViewHandler handler, IView view);

		/// <summary>
		/// Maps the abstract <see cref="IView.Frame"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		/// <param name="args">The arguments passed associated to this event.</param>
		public static void MapFrame(IViewHandler handler, IView view, object? args)
		{
			MappingFrame(handler, view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.ZIndex"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		/// <param name="args">The arguments passed associated to this event.</param>
		public static void MapZIndex(IViewHandler handler, IView view, object? args)
		{
			if (view.Parent is ILayout layout)
			{
				layout.Handler?.Invoke(nameof(ILayoutHandler.UpdateZIndex), view);
			}
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Focus"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		/// <param name="args">The arguments passed associated to this event.</param>
		public static void MapFocus(IViewHandler handler, IView view, object? args)
		{
			if (args is not FocusRequest request)
				return;

			((PlatformView?)handler.PlatformView)?.Focus(request);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.InputTransparent"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
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

		/// <summary>
		/// Maps the abstract <see cref="IView.Unfocus"/> method to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		/// <param name="args">The arguments passed associated to this event.</param>
		public static void MapUnfocus(IViewHandler handler, IView view, object? args)
		{
			((PlatformView?)handler.PlatformView)?.Unfocus(view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IToolTipElement.ToolTip"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapToolTip(IViewHandler handler, IView view)
		{
#if PLATFORM
			if (view is IToolTipElement tooltipContainer)
				handler.ToPlatform().UpdateToolTip(tooltipContainer.ToolTip);
#endif
		}
	}
}
