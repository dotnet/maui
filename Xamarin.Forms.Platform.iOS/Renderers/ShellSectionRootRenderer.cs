using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellSectionRootRenderer : UIViewController, IShellSectionRootRenderer
	{
		#region IShellSectionRootRenderer

		bool IShellSectionRootRenderer.ShowNavBar => Shell.GetNavBarIsVisible(((IShellContentController)ShellSection.CurrentItem).GetOrCreateContent());

		UIViewController IShellSectionRootRenderer.ViewController => this;

		#endregion IShellSectionRootRenderer

		const int HeaderHeight = 35;
		readonly IShellContext _shellContext;
		UIView _blurView;
		UIView _containerArea;
		int _currentIndex;
		IShellSectionRootHeader _header;
		bool _isAnimating;
		Dictionary<ShellContent, IVisualElementRenderer> _renderers = new Dictionary<ShellContent, IVisualElementRenderer>();
		IShellPageRendererTracker _tracker;
		bool _didLayoutSubviews;
		int _lastTabThickness = Int32.MinValue;
		Thickness _lastInset;

		ShellSection ShellSection { get; set; }
		IShellSectionController ShellSectionController => ShellSection;

		public ShellSectionRootRenderer(ShellSection shellSection, IShellContext shellContext)
		{
			ShellSection = shellSection ?? throw new ArgumentNullException(nameof(shellSection));
			_shellContext = shellContext;
		}

		public override void ViewDidLayoutSubviews()
		{
			_didLayoutSubviews = true;
			base.ViewDidLayoutSubviews();

			_containerArea.Frame = View.Bounds;

			LayoutRenderers();

			LayoutHeader();
		}

		public override void ViewDidLoad()
		{
			if (ShellSection.CurrentItem == null)
				throw new InvalidOperationException($"Content not found for active {ShellSection}. Title: {ShellSection.Title}. Route: {ShellSection.Route}.");

			base.ViewDidLoad();

			_containerArea = new UIView();
			if (Forms.IsiOS11OrNewer)
				_containerArea.InsetsLayoutMarginsFromSafeArea = false;

			View.AddSubview(_containerArea);

			LoadRenderers();

			ShellSection.PropertyChanged += OnShellSectionPropertyChanged;
			ShellSectionController.ItemsCollectionChanged += OnShellSectionItemsChanged;

			_blurView = new UIView();
			UIVisualEffect blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.ExtraLight);
			_blurView = new UIVisualEffectView(blurEffect);

			View.AddSubview(_blurView);

			UpdateHeaderVisibility();

			var tracker = _shellContext.CreatePageRendererTracker();
			tracker.IsRootPage = true;
			tracker.ViewController = this;
			tracker.Page = ((IShellContentController)ShellSection.CurrentItem).GetOrCreateContent();
			_tracker = tracker;
		}

		public override void ViewSafeAreaInsetsDidChange()
		{
			base.ViewSafeAreaInsetsDidChange();

			LayoutHeader();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && ShellSection != null)
			{
				ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;
				ShellSectionController.ItemsCollectionChanged -= OnShellSectionItemsChanged;

				_header?.Dispose();
				_tracker?.Dispose();

				foreach (var shellContent in ShellSectionController.GetItems())
				{
					if (_renderers.TryGetValue(shellContent, out var oldRenderer))
					{
						_renderers.Remove(shellContent);
						oldRenderer.NativeView.RemoveFromSuperview();
						oldRenderer.ViewController.RemoveFromParentViewController();
						var element = oldRenderer.Element;
						oldRenderer.Dispose();
						element?.ClearValue(Platform.RendererProperty);

					}
				}
			}

			ShellSection = null;
			_header = null;
			_tracker = null;
		}

		protected virtual void LayoutRenderers()
		{
			if (_isAnimating)
				return;

			var items = ShellSectionController.GetItems();
			for (int i = 0; i < items.Count; i++)
			{
				var shellContent = items[i];
				if (_renderers.TryGetValue(shellContent, out var renderer))
				{
					var view = renderer.NativeView;
					view.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
				}
			}
		}

		protected virtual void LoadRenderers()
		{
			var currentItem = ShellSection.CurrentItem;
			for (int i = 0; i < ShellSectionController.GetItems().Count; i++)
			{
				ShellContent item = ShellSectionController.GetItems()[i];
				var page = ((IShellContentController)item).GetOrCreateContent();
				var renderer = Platform.CreateRenderer(page);
				Platform.SetRenderer(page, renderer);
				AddChildViewController(renderer.ViewController);

				if (item == currentItem)
				{
					_containerArea.AddSubview(renderer.NativeView);
					_currentIndex = i;
				}

				_renderers[item] = renderer;
			}
		}

		protected virtual void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				var items = ShellSectionController.GetItems();
				var currentItem = ShellSection.CurrentItem;

				var oldIndex = _currentIndex;
				var oldItem = items[oldIndex];

				_currentIndex = ShellSectionController.GetItems().IndexOf(currentItem);

				var oldRenderer = _renderers[oldItem];
				var currentRenderer = _renderers[currentItem];

				// -1 == slide left, 1 ==  slide right
				int motionDirection = _currentIndex > oldIndex ? -1 : 1;

				_containerArea.AddSubview(currentRenderer.NativeView);

				_isAnimating = true;

				currentRenderer.NativeView.Frame = new CGRect(-motionDirection * View.Bounds.Width, 0, View.Bounds.Width, View.Bounds.Height);
				oldRenderer.NativeView.Frame = _containerArea.Bounds;

				UIView.Animate(.25, 0, UIViewAnimationOptions.CurveEaseOut, () =>
				{
					currentRenderer.NativeView.Frame = _containerArea.Bounds;
					oldRenderer.NativeView.Frame = new CGRect(motionDirection * View.Bounds.Width, 0, View.Bounds.Width, View.Bounds.Height);
				},
				() =>
				{
					oldRenderer.NativeView.RemoveFromSuperview();
					_isAnimating = false;

					_tracker.Page = ((IShellContentController)currentItem).Page;
				});
			}
		}

		protected virtual IShellSectionRootHeader CreateShellSectionRootHeader(IShellContext shellContext)
		{
			return new ShellSectionRootHeader(shellContext);
		}

		protected virtual void UpdateHeaderVisibility()
		{
			bool visible = ShellSectionController.GetItems().Count > 1;

			if (visible)
			{
				if (_header == null)
				{
					_header = CreateShellSectionRootHeader(_shellContext);
					_header.ShellSection = ShellSection;

					AddChildViewController(_header.ViewController);
					View.AddSubview(_header.ViewController.View);
				}
				_blurView.Hidden = false;
				LayoutHeader();
			}
			else
			{
				if (_header != null)
				{
					_header.ViewController.View.RemoveFromSuperview();
					_header.ViewController.RemoveFromParentViewController();
					_header.Dispose();
					_header = null;
				}
				_blurView.Hidden = true;
			}
		}

		void OnShellSectionItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// Make sure we do this after the header has a chance to react
			Device.BeginInvokeOnMainThread(UpdateHeaderVisibility);

			if (e.OldItems != null)
			{
				foreach (ShellContent oldItem in e.OldItems)
				{
					var oldRenderer = _renderers[oldItem];
					_renderers.Remove(oldItem);
					oldRenderer.NativeView.RemoveFromSuperview();
					oldRenderer.ViewController.RemoveFromParentViewController();
					oldRenderer.Dispose();
				}
			}

			if (e.NewItems != null)
			{
				foreach (ShellContent newItem in e.NewItems)
				{
					var page = ((IShellContentController)newItem).GetOrCreateContent();
					var renderer = Platform.CreateRenderer(page);
					Platform.SetRenderer(page, renderer);

					AddChildViewController(renderer.ViewController);
					_renderers[newItem] = renderer;
				}
			}
		}

		void LayoutHeader()
		{
			int tabThickness = 0;
			if (_header != null)
			{
				tabThickness = HeaderHeight;
				var headerTop = Forms.IsiOS11OrNewer ? View.SafeAreaInsets.Top : TopLayoutGuide.Length;
				CGRect frame = new CGRect(View.Bounds.X, headerTop, View.Bounds.Width, HeaderHeight);
				_blurView.Frame = frame;
				_header.ViewController.View.Frame = frame;
			}

			nfloat left;
			nfloat top;
			nfloat right;
			nfloat bottom;
			if (Forms.IsiOS11OrNewer)
			{
				left = View.SafeAreaInsets.Left;
				top = View.SafeAreaInsets.Top;
				right = View.SafeAreaInsets.Right;
				bottom = View.SafeAreaInsets.Bottom;
			}
			else
			{
				left = 0;
				top = TopLayoutGuide.Length;
				right = 0;
				bottom = BottomLayoutGuide.Length;
			}


			if (_didLayoutSubviews)
			{
				var newInset = new Thickness(left, top, right, bottom);
				if (newInset != _lastTabThickness || tabThickness != _lastTabThickness)
				{
					_lastTabThickness = tabThickness;
					_lastInset = new Thickness(left, top, right, bottom);
					((IShellSectionController)ShellSection).SendInsetChanged(_lastInset, _lastTabThickness);
				}
			}
		}
	}
}
