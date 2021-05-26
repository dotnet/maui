using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class ShellRenderer : UIViewController, IShellContext, IVisualElementRenderer, IEffectControlProvider
	{
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public ShellRenderer()
		{

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

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint) => new SizeRequest(new Size(100, 100));

		public void RegisterEffect(Effect effect)
		{
			throw new NotImplementedException();
		}

		public void SetElement(VisualElement element)
		{
			if (Element != null)
				throw new NotSupportedException("Reuse of the Shell Renderer is not supported");
			Element = element;
			OnElementSet((Shell)Element);

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(null, Element));
		}

		public virtual void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
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
			// HACK
			if (UIApplication.SharedApplication?.Delegate?.GetType()?.FullName == "XamarinFormsPreviewer.iOS.AppDelegate")
			{
				return new DesignerFlyoutRenderer(this);
			}

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
				Controls.Internals.Log.Warning(nameof(Shell), $"Failed on changing current item: {exc}");
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

			bool update = _currentShellItemRenderer.ViewController.View.UpdateFlowDirection(Element);
			update = View.UpdateFlowDirection(Element) || update;

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
				Controls.Internals.Log.Warning(nameof(Shell), $"Failed to SetCurrentShellItemController: {exc}");
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
			var color = Shell.BackgroundColor;
			if (color == null)
				color = ColorExtensions.BackgroundColor.ToColor();

			FlyoutRenderer.View.BackgroundColor = color.ToUIColor();
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

		// this won't work on the previewer if it's private
		internal class DesignerFlyoutRenderer : IShellFlyoutRenderer
		{
			readonly UIViewController _parent;

			public DesignerFlyoutRenderer(UIViewController parent)
			{
				_parent = parent;
			}
			public UIViewController ViewController => _parent;

			public UIView View => _parent.View;

			public void AttachFlyout(IShellContext context, UIViewController content)
			{
			}

			public void Dispose()
			{
			}
		}
	}
}
