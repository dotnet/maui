using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellItemRenderer : UITabBarController, IShellItemRenderer, IAppearanceObserver
	{
		#region IShellItemRenderer

		public ShellItem ShellItem
		{
			get => _shellItem;
			set
			{
				if (_shellItem == value)
					return;
				_shellItem = value;
				OnShellItemSet(_shellItem);
				CreateTabRenderers();
			}
		}

		public IShellItemController ShellItemController => (IShellItemController)ShellItem;

		UIViewController IShellItemRenderer.ViewController => this;

		#endregion IShellItemRenderer

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			UpdateShellAppearance(appearance);
		}

		#endregion IAppearanceObserver

		readonly IShellContext _context;
		readonly Dictionary<UIViewController, IShellSectionRenderer> _sectionRenderers = new Dictionary<UIViewController, IShellSectionRenderer>();
		IShellTabBarAppearanceTracker _appearanceTracker;
		ShellSection _currentSection;
		Page _displayedPage;
		bool _disposed;
		ShellItem _shellItem;
		bool _switched = true;

		IShellSectionRenderer CurrentRenderer { get; set; }

		public ShellItemRenderer(IShellContext context)
		{
			_context = context;
		}

		public override UIViewController SelectedViewController
		{
			get { return base.SelectedViewController; }
			set
			{
				base.SelectedViewController = value;

				var renderer = RendererForViewController(value);
				if (renderer != null)
				{
					ShellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, renderer.ShellSection);
					CurrentRenderer = renderer;
				}
			}
		}		

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_appearanceTracker?.UpdateLayout(this);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			ShouldSelectViewController = (tabController, viewController) =>
			{
				bool accept = true;
				var r = RendererForViewController(viewController);
				if (r != null)
					accept = ((IShellItemController)ShellItem).ProposeSection(r.ShellSection, false);

				return accept;
			};
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_disposed = true;
				foreach (var kvp in _sectionRenderers.ToList())
				{
					var renderer = kvp.Value;
					RemoveRenderer(renderer);
					renderer.Dispose();
				}

				if (_displayedPage != null)
					_displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;

				if (_currentSection != null)
					((IShellSectionController)_currentSection).RemoveDisplayedPageObserver(this);


				_sectionRenderers.Clear();
				ShellItem.PropertyChanged -= OnElementPropertyChanged;
				((IShellController)_context.Shell).RemoveAppearanceObserver(this);
				ShellItemController.ItemsCollectionChanged -= OnItemsCollectionChanged;

				CurrentRenderer = null;
				_shellItem = null;
				_currentSection = null;
				_displayedPage = null;
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
			{
				GoTo(ShellItem.CurrentItem);
			}
		}

		protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (ShellSection shellSection in e.OldItems)
				{
					var renderer = RendererForShellContent(shellSection);
					if (renderer != null)
					{
						ViewControllers = ViewControllers.Remove(renderer.ViewController);
						RemoveRenderer(renderer);
					}
				}
			}

			if (e.NewItems != null && e.NewItems.Count > 0)
			{
				var items = ShellItemController.GetItems();
				var count = items.Count;
				UIViewController[] viewControllers = new UIViewController[count];

				int maxTabs = 5; // fetch this a better way
				bool willUseMore = count > maxTabs;

				int i = 0;
				bool goTo = false; // its possible we are in a transitionary state and should not nav
				var current = ShellItem.CurrentItem;
				for (int j = 0; j < items.Count; j++)
				{
					var shellContent = items[j];
					var renderer = RendererForShellContent(shellContent) ?? _context.CreateShellSectionRenderer(shellContent);

					if (willUseMore && j >= maxTabs - 1)
						renderer.IsInMoreTab = true;
					else
						renderer.IsInMoreTab = false;

					renderer.ShellSection = shellContent;

					AddRenderer(renderer);
					viewControllers[i++] = renderer.ViewController;
					if (shellContent == current)
						goTo = true;
				}

				ViewControllers = viewControllers;

				if (goTo)
					GoTo(ShellItem.CurrentItem);
			}

			SetTabBarHidden(ViewControllers.Length == 1);
		}

		protected virtual void OnShellItemSet(ShellItem shellItem)
		{
			_appearanceTracker = _context.CreateTabBarAppearanceTracker();
			shellItem.PropertyChanged += OnElementPropertyChanged;
			((IShellController)_context.Shell).AddAppearanceObserver(this, shellItem);
			ShellItemController.ItemsCollectionChanged += OnItemsCollectionChanged;
		}

		protected virtual void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BaseShellItem.IsEnabledProperty.PropertyName)
			{
				var shellSection = (ShellSection)sender;
				var renderer = RendererForShellContent(shellSection);
				var index = ViewControllers.ToList().IndexOf(renderer.ViewController);
				TabBar.Items[index].Enabled = shellSection.IsEnabled;
			}
		}

		protected virtual void UpdateShellAppearance(ShellAppearance appearance)
		{
			if (appearance == null)
			{
				_appearanceTracker.ResetAppearance(this);
				return;
			}
			_appearanceTracker.SetAppearance(this, appearance);
		}

		void AddRenderer(IShellSectionRenderer renderer)
		{
			if (_sectionRenderers.ContainsKey(renderer.ViewController))
				return;
			_sectionRenderers[renderer.ViewController] = renderer;
			renderer.ShellSection.PropertyChanged += OnShellSectionPropertyChanged;
		}

		void CreateTabRenderers()
		{
			var items = ShellItemController.GetItems();
			var count = items.Count;
			int maxTabs = 5; // fetch this a better way
			bool willUseMore = count > maxTabs;

			UIViewController[] viewControllers = new UIViewController[count];
			int i = 0;
			foreach (var shellContent in items)
			{
				var renderer = _context.CreateShellSectionRenderer(shellContent);

				renderer.IsInMoreTab = willUseMore && i >= maxTabs - 1;

				renderer.ShellSection = shellContent;
				AddRenderer(renderer);
				viewControllers[i++] = renderer.ViewController;
			}
			ViewControllers = viewControllers;

			// No sense showing a bar that has a single icon
			if (ViewControllers.Length == 1)
				SetTabBarHidden(true);

			// Make sure we are at the right item
			GoTo(ShellItem.CurrentItem);

			// now that they are applied we can set the enabled state of the TabBar items
			for (i = 0; i < ViewControllers.Length; i++)
			{
				var renderer = RendererForViewController(ViewControllers[i]);
				if (!renderer.ShellSection.IsEnabled)
				{
					TabBar.Items[i].Enabled = false;
				}
			}
		}
			   
		void GoTo(ShellSection shellSection)
		{
			if (shellSection == null || _currentSection == shellSection)
				return;
			var renderer = RendererForShellContent(shellSection);
			if (renderer?.ViewController != SelectedViewController)
				SelectedViewController = renderer.ViewController;
			CurrentRenderer = renderer;

			if (_currentSection != null)
			{
				((IShellSectionController)_currentSection).RemoveDisplayedPageObserver(this);
			}

			_currentSection = shellSection;

			if (_currentSection != null)
			{
				((IShellSectionController)_currentSection).AddDisplayedPageObserver(this, OnDisplayedPageChanged);
				_switched = true;
			}
		}

		void OnDisplayedPageChanged(Page page)
		{
			if (page == _displayedPage)
				return;

			if (_displayedPage != null)
				_displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;

			_displayedPage = page;

			if (_displayedPage != null)
			{
				_displayedPage.PropertyChanged += OnDisplayedPagePropertyChanged;

				if (!_currentSection.Stack.Contains(_displayedPage) || _switched)
				{
					_switched = false;
					UpdateTabBarHidden();
				}
			}
		}

		void OnDisplayedPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.TabBarIsVisibleProperty.PropertyName)
				UpdateTabBarHidden();
		}

		void RemoveRenderer(IShellSectionRenderer renderer)
		{
			if (_sectionRenderers.Remove(renderer.ViewController))
				renderer.ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;
		}

		IShellSectionRenderer RendererForShellContent(ShellSection shellSection)
		{
			// Not Efficient!
			foreach (var item in _sectionRenderers)
			{
				if (item.Value.ShellSection == shellSection)
					return item.Value;
			}
			return null;
		}

		IShellSectionRenderer RendererForViewController(UIViewController viewController)
		{
			// Efficient!
			if (_sectionRenderers.TryGetValue(viewController, out var value))
				return value;
			return null;
		}

		void SetTabBarHidden(bool hidden)
		{
			TabBar.Hidden = hidden;

			if (CurrentRenderer == null)
				return;

			// now we must do the uikit jiggly dance to make sure the safe area updates. Failure
			// to perform the jiggle may result in the page not insetting properly when unhiding
			// the TabBar

			// a devious 1 pixel inset vertically
			CurrentRenderer.ViewController.View.Frame = View.Bounds.Inset(0, 1);

			// and quick as a whip we return it back to what it was with its insets being all proper
			CurrentRenderer.ViewController.View.Frame = View.Bounds;
		}

		void UpdateTabBarHidden()
		{
			if (_displayedPage == null || ShellItem == null)
				return;

			var hidden = !Shell.GetTabBarIsVisible(_displayedPage);
			if (ShellItemController.GetItems().Count > 1)
			{
				SetTabBarHidden(hidden);
			}
		}
	}
}