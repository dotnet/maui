#nullable disable
using System;
using System.ComponentModel;
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

		IShellItemRenderer _currentShellItemRenderer;
		bool _disposed;
		IShellFlyoutRenderer _flyoutRenderer;
		Task _activeTransition = Task.CompletedTask;
		IShellItemRenderer _incomingRenderer;
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

		public virtual void SetElementSize(Size size)
		{
			Element.Layout(new Rect(Element.X, Element.Y, size.Width, size.Height));
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			if (_currentShellItemRenderer != null)
				_currentShellItemRenderer.ViewController.View.Frame = View.Bounds;

			SetElementSize(new Size(View.Bounds.Width, View.Bounds.Height));
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
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_disposed = true;
				FlyoutRenderer?.Dispose();
			}

			FlyoutRenderer = null;
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
			var currentItem = Shell.CurrentItem;

			var oldLayer = _currentShellItemRenderer
				?.ViewController
				?.View
				?.Layer;

			if (oldLayer?.AnimationKeys?.Length > 0)
				oldLayer.RemoveAllAnimations();

			await _activeTransition;
			if (_currentShellItemRenderer?.ShellItem != currentItem)
			{
				var newController = CreateShellItemRenderer(currentItem);
				await SetCurrentShellItemControllerAsync(newController);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
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
			if (_currentShellItemRenderer?.ViewController == null)
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
			_incomingRenderer = value;
			await _activeTransition;

			// This means the selected item changed while the active transition
			// was finishing up
			if (_incomingRenderer != value ||
				value.ShellItem != this.Shell.CurrentItem)
			{
				(value as IDisconnectable)?.Disconnect();
				value?.Dispose();
				return;
			}

			var oldRenderer = _currentShellItemRenderer;
			(oldRenderer as IDisconnectable)?.Disconnect();
			var newRenderer = value;

			_currentShellItemRenderer = value;

			AddChildViewController(newRenderer.ViewController);
			View.AddSubview(newRenderer.ViewController.View);
			View.SendSubviewToBack(newRenderer.ViewController.View);

			newRenderer.ViewController.View.Frame = View.Bounds;

			if (oldRenderer != null)
			{
				var transition = CreateShellItemTransition();

				_activeTransition = transition.Transition(oldRenderer, newRenderer);
				await _activeTransition;

				oldRenderer.ViewController.RemoveFromParentViewController();
				oldRenderer.ViewController.View.RemoveFromSuperview();
				oldRenderer.Dispose();
			}
			else
			{
				View.AddSubview(newRenderer.ViewController.View);
			}

			// current renderer is still valid
			if (_currentShellItemRenderer == value)
			{
				UpdateBackgroundColor();
				UpdateFlowDirection();
			}
		}

		protected virtual void UpdateBackgroundColor()
		{
			var color = Shell.BackgroundColor?.ToPlatform();
			if (color == null)
				color = Microsoft.Maui.Platform.ColorExtensions.BackgroundColor;

			FlyoutRenderer.View.BackgroundColor = color;
		}

		void SetupCurrentShellItem()
		{
			if (Shell.CurrentItem == null)
			{
				return;
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
		}
	}
}
