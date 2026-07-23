#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class ShellRenderer : UIViewController, IShellContext, IPlatformViewHandler
	{
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Static mapper is shared for the renderer type and does not capture renderer instances.")]
		public static IPropertyMapper<Shell, ShellRenderer> Mapper = new PropertyMapper<Shell, ShellRenderer>(ViewHandler.ViewMapper);
		public static CommandMapper<Shell, ShellRenderer> CommandMapper = new CommandMapper<Shell, ShellRenderer>(ViewHandler.ViewCommandMapper);

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public ShellRenderer()
		{

		}

		public override bool PrefersHomeIndicatorAutoHidden
			=> Shell?.CurrentPage?.OnThisPlatform()?.PrefersHomeIndicatorAutoHidden() ?? base.PrefersHomeIndicatorAutoHidden;


		public override bool PrefersStatusBarHidden()
			=> Shell?.CurrentPage?.OnThisPlatform()?.PrefersStatusBarHidden() == StatusBarHiddenMode.True;

		public override UIKit.UIStatusBarAnimation PreferredStatusBarUpdateAnimation
		{
			get
			{
				var mode = Shell?.CurrentPage?.OnThisPlatform()?.PreferredStatusBarUpdateAnimation();
				return mode switch
				{
					PlatformConfiguration.iOSSpecific.UIStatusBarAnimation.None => UIKit.UIStatusBarAnimation.None,
					PlatformConfiguration.iOSSpecific.UIStatusBarAnimation.Fade => UIKit.UIStatusBarAnimation.Fade,
					PlatformConfiguration.iOSSpecific.UIStatusBarAnimation.Slide => UIKit.UIStatusBarAnimation.Slide,
					_ => base.PreferredStatusBarUpdateAnimation,
				};
			}
		}


		#region IShellContext

		bool IShellContext.AllowFlyoutGesture
		{
			get
			{
				ShellSection shellSection = Shell?.CurrentItem?.CurrentItem;
				if (shellSection == null)
					return true;
				return shellSection.Stack.Count <= 1;
			}
		}

		IShellItemRenderer IShellContext.CurrentShellItemRenderer => _currentShellItemRenderer;

		IShellNavBarAppearanceTracker IShellContext.CreateNavBarAppearanceTracker()
		{
			return CreateNavBarAppearanceTracker();
		}

		IShellPageRendererTracker IShellContext.CreatePageRendererTracker()
		{
			return CreatePageRendererTracker();
		}

		IShellFlyoutContentRenderer IShellContext.CreateShellFlyoutContentRenderer()
		{
			return CreateShellFlyoutContentRenderer();
		}

		IShellSearchResultsRenderer IShellContext.CreateShellSearchResultsRenderer()
		{
			return CreateShellSearchResultsRenderer();
		}

		IShellSectionRenderer IShellContext.CreateShellSectionRenderer(ShellSection shellSection)
		{
			return CreateShellSectionRenderer(shellSection);
		}

		IShellTabBarAppearanceTracker IShellContext.CreateTabBarAppearanceTracker()
		{
			return CreateTabBarAppearanceTracker();
		}

		#endregion IShellContext

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Current item renderer is owned by ShellRenderer and disposed when replaced or when ShellRenderer is disposed.")]
		IShellItemRenderer _currentShellItemRenderer;
		bool _disposed;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Flyout renderer is owned by ShellRenderer and disposed in Dispose(bool).")]
		IShellFlyoutRenderer _flyoutRenderer;
		Task _activeTransition = Task.CompletedTask;
		TaskCompletionSource<bool> _activeTransitionCancellation;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Incoming item renderer is a transient transition reference cleared when ShellRenderer is disposed.")]
		IShellItemRenderer _incomingRenderer;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Outgoing item renderer is retained only while its transition is active and disposed when the transition completes or ShellRenderer disconnects.")]
		IShellItemRenderer _outgoingRenderer;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Pending item renderers are disposed when superseded or when ShellRenderer disconnects.")]
		readonly HashSet<IShellItemRenderer> _pendingRenderers = new(ReferenceEqualityComparer.Instance);
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "MauiContext is provided by the handler and cleared when ShellRenderer is disposed.")]
		IMauiContext _mauiContext;

		IShellFlyoutRenderer FlyoutRenderer
		{
			get
			{
				if (_flyoutRenderer == null)
				{
					FlyoutRenderer = CreateFlyoutRenderer();
					FlyoutRenderer.AttachFlyout(this, this);
				}
				return _flyoutRenderer;
			}
			set { _flyoutRenderer = value; }
		}

		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Event is cleared in Dispose(bool) when ShellRenderer is released.")]
		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public VisualElement Element { get; private set; }
		public UIView NativeView => FlyoutRenderer.View;
		public Shell Shell => (Shell)Element;
		public UIViewController ViewController => FlyoutRenderer.ViewController;

		public void SetElement(VisualElement element)
		{
			if (Element != null)
				throw new NotSupportedException("Reuse of the Shell Renderer is not supported");
			Element = element;
			OnElementSet((Shell)Element);

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(null, Element));
			Mapper.UpdateProperties(this, Element);
		}

		[Obsolete]
		public virtual void SetElementSize(Size size)
		{
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			if (_currentShellItemRenderer != null)
				_currentShellItemRenderer.ViewController.View.Frame = View.Bounds;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			SetupCurrentShellItem();
		}

		protected virtual IShellFlyoutRenderer CreateFlyoutRenderer()
		{
			return new ShellFlyoutRenderer()
			{
				FlyoutTransition = new SlideFlyoutTransition()
			};
		}

		protected virtual IShellNavBarAppearanceTracker CreateNavBarAppearanceTracker()
		{
			return new SafeShellNavBarAppearanceTracker();
		}

		protected virtual IShellPageRendererTracker CreatePageRendererTracker()
		{
			return new ShellPageRendererTracker(this);
		}

		protected virtual IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer()
		{
			return new ShellFlyoutContentRenderer(this);
		}

		protected virtual IShellItemRenderer CreateShellItemRenderer(ShellItem item)
		{
			return new ShellItemRenderer(this)
			{
				ShellItem = item
			};
		}

		protected virtual IShellItemTransition CreateShellItemTransition()
		{
			return new ShellItemTransition();
		}

		protected virtual IShellSearchResultsRenderer CreateShellSearchResultsRenderer()
		{
			return new ShellSearchResultsRenderer(this);
		}

		protected virtual IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection)
		{
			return new ShellSectionRenderer(this);
		}

		protected virtual IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker()
		{
			return new ShellTabBarAppearanceTracker();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				DisconnectHandler();

			base.Dispose(disposing);
		}

		void DisconnectHandler()
		{
			if (_disposed)
				return;

			_disposed = true;

			var element = Element;
			if (element != null)
				element.PropertyChanged -= OnElementPropertyChanged;

			ElementChanged = null;
			CancelActiveTransition();

			foreach (var pendingRenderer in _pendingRenderers)
			{
				if (!ReferenceEquals(pendingRenderer, _currentShellItemRenderer))
					DisconnectAndDispose(pendingRenderer);
			}

			_pendingRenderers.Clear();
			var outgoingRenderer = _outgoingRenderer;
			_outgoingRenderer = null;
			if (outgoingRenderer is not null &&
				!ReferenceEquals(outgoingRenderer, _currentShellItemRenderer))
			{
				DetachAndDispose(outgoingRenderer);
			}

			DisconnectAndDispose(_currentShellItemRenderer);
			_flyoutRenderer?.Dispose();

			_activeTransition = Task.CompletedTask;
			_activeTransitionCancellation = null;
			_incomingRenderer = null;
			_currentShellItemRenderer = null;
			_flyoutRenderer = null;
			_mauiContext = null;

			if (element is IElement shell && ReferenceEquals(shell.Handler, this))
				shell.Handler = null;

			Element = null;
		}

		static void DisconnectAndDispose(IShellItemRenderer renderer)
		{
			DetachRenderer(renderer);
			(renderer as IDisconnectable)?.Disconnect();
			renderer?.Dispose();
		}

		static void DetachAndDispose(IShellItemRenderer renderer)
		{
			DetachRenderer(renderer);
			renderer?.Dispose();
		}

		static void DetachRenderer(IShellItemRenderer renderer)
		{
			var viewController = renderer?.ViewController;
			viewController?.ViewIfLoaded?.RemoveFromSuperview();
			viewController?.RemoveFromParentViewController();
		}

		void CancelActiveTransition()
		{
			if (_activeTransition.IsCompleted)
				return;

			_currentShellItemRenderer?.ViewController?.ViewIfLoaded?.Layer.RemoveAllAnimations();
			_activeTransitionCancellation?.TrySetResult(true);
		}

		static async Task WaitForTransitionOrCancellationAsync(Task transition, Task cancellation)
		{
			var completedTask = await Task.WhenAny(transition, cancellation);
			await completedTask;
		}

		protected virtual async void OnCurrentItemChanged()
		{
			try
			{
				await OnCurrentItemChangedAsync();
			}
			catch (Exception exc)
			{
				_mauiContext?.CreateLogger<ShellRenderer>()?.LogWarning(exc, "Failed on changing current item");
			}
		}

		protected virtual async Task OnCurrentItemChangedAsync()
		{
			var shell = Element as Shell;
			if (_disposed || shell == null)
				return;

			var currentItem = shell.CurrentItem;

			var oldLayer = _currentShellItemRenderer
				?.ViewController
				?.View
				?.Layer;

			if (oldLayer?.AnimationKeys?.Length > 0)
				oldLayer.RemoveAllAnimations();

			await _activeTransition;
			if (_disposed)
				return;

			if (_currentShellItemRenderer?.ShellItem != currentItem)
			{
				var newController = CreateShellItemRenderer(currentItem);
				await SetCurrentShellItemControllerAsync(newController);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_disposed)
				return;

			if (e.PropertyName == Shell.CurrentItemProperty.PropertyName)
			{
				OnCurrentItemChanged();
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateFlowDirection(true);
			}
		}

		void UpdateFlowDirection(bool readdViews = false)
		{
			if (_disposed || _currentShellItemRenderer?.ViewController == null)
				return;

			var originalValue = _currentShellItemRenderer.ViewController.View.SemanticContentAttribute;
			var originalViewValue = View.SemanticContentAttribute;

			_currentShellItemRenderer.ViewController.View.UpdateFlowDirection(Element);
			View.UpdateFlowDirection(Element);

			bool update = originalValue == _currentShellItemRenderer.ViewController.View.SemanticContentAttribute ||
				originalViewValue == View.SemanticContentAttribute;

			if (update && readdViews)
			{
				_currentShellItemRenderer.ViewController.View.RemoveFromSuperview();
				View.AddSubview(_currentShellItemRenderer.ViewController.View);
				View.SendSubviewToBack(_currentShellItemRenderer.ViewController.View);
			}
		}

		[UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "Shell PropertyChanged subscription is removed in Dispose(bool).")]
		protected virtual void OnElementSet(Shell element)
		{
			if (element == null)
				return;

			element.PropertyChanged += OnElementPropertyChanged;
		}

		protected async void SetCurrentShellItemController(IShellItemRenderer value)
		{
			try
			{
				await SetCurrentShellItemControllerAsync(value);
			}
			catch (Exception exc)
			{
				_mauiContext?.CreateLogger<ShellRenderer>()?.LogWarning(exc, "Failed to SetCurrentShellItemController");
			}
		}

		protected async Task SetCurrentShellItemControllerAsync(IShellItemRenderer value)
		{
			if (_disposed)
			{
				DisconnectAndDispose(value);
				return;
			}

			_pendingRenderers.Add(value);
			_incomingRenderer = value;
			await _activeTransition;

			if (_disposed)
			{
				if (_pendingRenderers.Remove(value))
					DisconnectAndDispose(value);

				return;
			}

			// This means the selected item changed while the active transition
			// was finishing up
			var shell = Element as Shell;
			if (_incomingRenderer != value ||
				shell == null ||
				value.ShellItem != shell.CurrentItem)
			{
				if (ReferenceEquals(_incomingRenderer, value))
					_incomingRenderer = null;

				if (_pendingRenderers.Remove(value))
					DisconnectAndDispose(value);

				return;
			}

			var oldRenderer = _currentShellItemRenderer;
			(oldRenderer as IDisconnectable)?.Disconnect();
			var newRenderer = value;

			_pendingRenderers.Remove(value);
			_currentShellItemRenderer = value;
			_incomingRenderer = null;

			AddChildViewController(newRenderer.ViewController);
			View.AddSubview(newRenderer.ViewController.View);
			View.SendSubviewToBack(newRenderer.ViewController.View);

			newRenderer.ViewController.View.Frame = View.Bounds;

			if (oldRenderer != null)
			{
				var transition = CreateShellItemTransition();
				var transitionCancellation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

				_activeTransitionCancellation = transitionCancellation;
				_outgoingRenderer = oldRenderer;
				_activeTransition = WaitForTransitionOrCancellationAsync(
					transition.Transition(oldRenderer, newRenderer),
					transitionCancellation.Task);

				try
				{
					await _activeTransition;
				}
				finally
				{
					if (ReferenceEquals(_activeTransitionCancellation, transitionCancellation))
						_activeTransitionCancellation = null;

					if (ReferenceEquals(_outgoingRenderer, oldRenderer))
					{
						_outgoingRenderer = null;
						DetachAndDispose(oldRenderer);
					}
				}

				if (_disposed)
					return;
			}
			else
			{
				View.AddSubview(newRenderer.ViewController.View);
			}

			// current renderer is still valid
			if (!_disposed && _currentShellItemRenderer == value)
			{
				UpdateBackgroundColor();
				UpdateFlowDirection();
			}
		}

		protected virtual void UpdateBackgroundColor()
		{
			if (_disposed || Element == null)
				return;

			var color = Shell.BackgroundColor?.ToPlatform();
			if (color == null)
				color = Microsoft.Maui.Platform.ColorExtensions.BackgroundColor;

			FlyoutRenderer.View.BackgroundColor = color;
		}

		void SetupCurrentShellItem()
		{
			if (_disposed)
				return;

			if (Shell.CurrentItem == null)
			{
				throw new InvalidOperationException("Active Shell Item not set. Have you added any Shell Items to your Shell?");
			}
			else if (Shell.CurrentItem.CurrentItem == null)
			{
				throw new InvalidOperationException($"Content not found for active {Shell.CurrentItem}. Title: {Shell.CurrentItem.Title}. Route: {Shell.CurrentItem.Route}.");
			}
			else if (_currentShellItemRenderer == null)
			{
				OnCurrentItemChanged();
			}
		}

		bool IViewHandler.HasContainer { get => false; set { } }

		object IViewHandler.ContainerView => null;

		IView IViewHandler.VirtualView => Element;

		object IElementHandler.PlatformView => NativeView;

		Maui.IElement IElementHandler.VirtualView => Element;

		IMauiContext IElementHandler.MauiContext => _mauiContext;

		UIView IPlatformViewHandler.PlatformView => NativeView;

		UIView IPlatformViewHandler.ContainerView => null;

		Size IViewHandler.GetDesiredSize(double widthConstraint, double heightConstraint) => new Size(100, 100);

		void IViewHandler.PlatformArrange(Rect rect)
		{
			//TODO I don't think we need this
		}

		void IElementHandler.SetMauiContext(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		void IElementHandler.SetVirtualView(Maui.IElement view)
		{
			SetElement((VisualElement)view);
		}

		void IElementHandler.UpdateValue(string property)
		{
			Mapper.UpdateProperty(this, Element, property);
		}

		void IElementHandler.Invoke(string command, object args)
		{
			CommandMapper.Invoke(this, Element, command, args);
		}

		void IElementHandler.DisconnectHandler()
		{
			DisconnectHandler();
		}
	}
}
