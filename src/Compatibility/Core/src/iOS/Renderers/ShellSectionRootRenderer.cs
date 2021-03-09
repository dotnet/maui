using CoreAnimation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class ShellSectionRootRenderer : UIViewController, IShellSectionRootRenderer, IDisconnectable
	{
		#region IShellSectionRootRenderer

		bool IShellSectionRootRenderer.ShowNavBar => Shell.GetNavBarIsVisible(((IShellContentController)ShellSection.CurrentItem).GetOrCreateContent());

		UIViewController IShellSectionRootRenderer.ViewController => this;

		#endregion IShellSectionRootRenderer

		const int HeaderHeight = 35;
		IShellContext _shellContext;
		UIView _blurView;
		UIView _containerArea;
		ShellContent _currentContent;
		int _currentIndex = 0;
		IShellSectionRootHeader _header;
		IVisualElementRenderer _isAnimatingOut;
		Dictionary<ShellContent, IVisualElementRenderer> _renderers = new Dictionary<ShellContent, IVisualElementRenderer>();
		IShellPageRendererTracker _tracker;
		bool _didLayoutSubviews;
		int _lastTabThickness = Int32.MinValue;
		Thickness _lastInset;
		bool _isDisposed;
		UIViewPropertyAnimator _pageAnimation;

		ShellSection ShellSection
		{
			get;
			set;
		}

		IShellSectionController ShellSectionController => ShellSection;

		public ShellSectionRootRenderer(ShellSection shellSection, IShellContext shellContext)
		{
			ShellSection = shellSection ?? throw new ArgumentNullException(nameof(shellSection));
			_shellContext = shellContext;
			_shellContext.Shell.PropertyChanged += HandleShellPropertyChanged;
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
			if (_isDisposed)
				return;

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

			if(ShellSection.CurrentItem != null)
				tracker.Page = ((IShellContentController)ShellSection.CurrentItem).GetOrCreateContent();
			_tracker = tracker;
			UpdateFlowDirection();
		}

		public override void ViewWillAppear(bool animated)
		{
			if (_isDisposed)
				return;

				UpdateFlowDirection();
			base.ViewWillAppear(animated);
		}

		public override void ViewSafeAreaInsetsDidChange()
		{
			if (_isDisposed)
				return;

			base.ViewSafeAreaInsetsDidChange();

			LayoutHeader();
		}



		void IDisconnectable.Disconnect()
		{
			_pageAnimation?.StopAnimation(true);
			_pageAnimation = null;
			if (ShellSection != null)
				ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;

			if (ShellSectionController != null)
				ShellSectionController.ItemsCollectionChanged -= OnShellSectionItemsChanged;

			if (_shellContext?.Shell != null)
				_shellContext.Shell.PropertyChanged -= HandleShellPropertyChanged;

			if (_renderers != null)
			{
				foreach (var renderer in _renderers)
				{
					var oldRenderer = renderer.Value;
					var element = oldRenderer.Element;
					element?.ClearValue(Platform.RendererProperty);
					(renderer.Value as IDisconnectable)?.Disconnect();
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing && ShellSection != null)
			{
				(this as IDisconnectable).Disconnect();

				this.RemoveFromParentViewController();

				_header?.Dispose();
				_tracker?.Dispose();

				foreach (var renderer in _renderers)
				{
					var oldRenderer = renderer.Value;

					if(oldRenderer.NativeView != null)
						oldRenderer.NativeView.RemoveFromSuperview();

					if (oldRenderer.ViewController != null)
						oldRenderer.ViewController.RemoveFromParentViewController();

					var element = oldRenderer.Element;
					element?.ClearValue(Platform.RendererProperty);
					oldRenderer?.Dispose();
				}

				_renderers.Clear();
			}

			if(disposing)
			{
				_shellContext.Shell.PropertyChanged -= HandleShellPropertyChanged;
			}

			_shellContext = null;
			ShellSection = null;
			_header = null;
			_tracker = null;
			_currentContent = null;
			_isDisposed = true;
		}

		protected virtual void LayoutRenderers()
		{
			if (_isAnimatingOut != null)
				return;

			var items = ShellSectionController.GetItems();
			for (int i = 0; i < items.Count; i++)
			{
				var shellContent = items[i];
				if (_renderers.TryGetValue(shellContent, out var renderer))
				{
					var view = renderer.NativeView;
					if(view != null)
						view.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
				}
			}
		}

		protected virtual void LoadRenderers()
		{
			Dictionary<ShellContent, Page> createdPages = new Dictionary<ShellContent, Page>();
			var contentItems = ShellSectionController.GetItems();

			// pre create all the pages in case the visibility of a page
			// removes the page from shell
			for (int i = 0; i < contentItems.Count; i++)
			{
				ShellContent item = contentItems[i];
				var page = ((IShellContentController)item).GetOrCreateContent();
				createdPages.Add(item, page);
			}

			var currentItem = ShellSection.CurrentItem;
			contentItems = ShellSectionController.GetItems();

			for (int i = 0; i < contentItems.Count; i++)
			{
				ShellContent item = contentItems[i];

				if (_renderers.ContainsKey(item))
					continue;

				Page page = null;
				if(!createdPages.TryGetValue(item, out page))
				{
					page = ((IShellContentController)item).GetOrCreateContent();
					contentItems = ShellSectionController.GetItems();
				}

				var renderer = SetPageRenderer(page, item);

				AddChildViewController(renderer.ViewController);

				if (item == currentItem)
				{
					_containerArea.AddSubview(renderer.NativeView);
					_currentContent = currentItem;
					_currentIndex = i;
				}
			}
		}

		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is(VisualElement.FlowDirectionProperty))
				UpdateFlowDirection();
		}

		protected virtual void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_isDisposed)
				return;

			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				var newContent = ShellSection.CurrentItem;
				var oldContent = _currentContent;

				if (newContent == null)
					return;

				if (_currentContent == null)
				{
					_currentContent = newContent;
					_currentIndex = ShellSectionController.GetItems().IndexOf(_currentContent);
					_tracker.Page = ((IShellContentController)newContent).Page;
					return;
				}

				var items = ShellSectionController.GetItems();
				if (items.Count == 0)
					return;

				var oldIndex = _currentIndex;
				var newIndex = items.IndexOf(newContent);
				var oldRenderer = _renderers[oldContent];

				// this means the currently visible item has been removed
				if (oldIndex == -1 && _currentIndex <= newIndex)
				{
					newIndex++;
				}

				_currentContent = newContent;
				_currentIndex = newIndex;

				if (!_renderers.ContainsKey(newContent))
					return;

				var currentRenderer = _renderers[newContent];
				_isAnimatingOut = oldRenderer;
				_pageAnimation?.StopAnimation(true);
				_pageAnimation = null;
				_pageAnimation = CreateContentAnimator(oldRenderer, currentRenderer, oldIndex, newIndex, _containerArea);

				if (_pageAnimation != null)
				{
					_pageAnimation.AddCompletion((p) =>
					{
						if (_isDisposed)
							return;

						if (p == UIViewAnimatingPosition.End)
						{
							RemoveNonVisibleRenderers();
						}
					});

					_pageAnimation.StartAnimation();
				}
				else
				{
					RemoveNonVisibleRenderers();
				}
			}
		}

		UIViewPropertyAnimator CreateContentAnimator(
			IVisualElementRenderer oldRenderer,
			IVisualElementRenderer newRenderer,
			int oldIndex,
			int newIndex,
			UIView containerView)
		{
			containerView.AddSubview(newRenderer.NativeView);
			// -1 == slide left, 1 ==  slide right
			int motionDirection = newIndex > oldIndex ? -1 : 1;

			newRenderer.NativeView.Frame = new CGRect(-motionDirection * View.Bounds.Width, 0, View.Bounds.Width, View.Bounds.Height);

			if (oldRenderer.NativeView != null)
				oldRenderer.NativeView.Frame = containerView.Bounds;

			return new UIViewPropertyAnimator(0.25, UIViewAnimationCurve.EaseOut, () =>
			{
				newRenderer.NativeView.Frame = containerView.Bounds;

				if (oldRenderer.NativeView != null)
					oldRenderer.NativeView.Frame = new CGRect(motionDirection * View.Bounds.Width, 0, View.Bounds.Width, View.Bounds.Height);

			});
		}

		void RemoveNonVisibleRenderers()
		{
			IVisualElementRenderer activeRenderer = null;
			var activeItem = ShellSection?.CurrentItem;

			if (activeItem is IShellContentController scc &&
				_renderers.TryGetValue(activeItem, out activeRenderer))
			{
				var sectionItems = ShellSectionController.GetItems();
				List<ShellContent> removeMe = null;
				foreach (var r in _renderers)
				{
					if (r.Value == activeRenderer)
						continue;

					var oldContent = r.Key;
					var oldRenderer = r.Value;

					r.Value.NativeView.RemoveFromSuperview();

					if (!sectionItems.Contains(oldContent) && _renderers.ContainsKey(oldContent))
					{
						removeMe = removeMe ?? new List<ShellContent>();
						removeMe.Add(oldContent);

						if (oldRenderer.NativeView != null)
						{
							oldRenderer.ViewController.RemoveFromParentViewController();
							oldRenderer.Dispose();
						}
					}
				}

				if(removeMe != null)
				{
					foreach (var remove in removeMe)
						_renderers.Remove(remove);
				}

				_tracker.Page = scc.Page;
			}

			_isAnimatingOut = null;
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

		void UpdateFlowDirection()
		{
			if(_shellContext?.Shell?.CurrentItem?.CurrentItem == ShellSection)
				this.View.UpdateFlowDirection(_shellContext.Shell);
		}

		void OnShellSectionItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_isDisposed)
				return;

			// Make sure we do this after the header has a chance to react
			Device.BeginInvokeOnMainThread(UpdateHeaderVisibility);

			if (e.OldItems != null)
			{
				foreach (ShellContent oldItem in e.OldItems)
				{
					// if current item is removed will be handled by the currentitem property changed event
					// That way the render is swapped out cleanly once the new current item is set
					if (_currentContent == oldItem)
						continue;

					var oldRenderer = _renderers[oldItem];

					if (oldRenderer == _isAnimatingOut)
						continue;

					if (e.OldStartingIndex < _currentIndex)
						_currentIndex--;
					
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
					if (_renderers.ContainsKey(newItem))
						continue;

					var page = ((IShellContentController)newItem).GetOrCreateContent();
					var renderer = SetPageRenderer(page, newItem);

					AddChildViewController(renderer.ViewController);
				}
			}
		}

		IVisualElementRenderer SetPageRenderer(Page page, ShellContent shellContent)
		{
			var oldRenderer = Platform.GetRenderer(page);
			if(oldRenderer != null)
				oldRenderer?.Dispose();

			var renderer = Platform.CreateRenderer(page);
			Platform.SetRenderer(page, renderer);
			_renderers[shellContent] = renderer;

			return renderer;
		}

		void LayoutHeader()
		{
			if (ShellSection == null)
				return;

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
