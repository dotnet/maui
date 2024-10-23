#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellItemRenderer : UITabBarController, IShellItemRenderer, IAppearanceObserver, IUINavigationControllerDelegate, IDisconnectable
	{
		readonly static UITableViewCell[] EmptyUITableViewCellArray = Array.Empty<UITableViewCell>();

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
		static UIColor _defaultMoreTextLabelTextColor;

		internal IShellSectionRenderer CurrentRenderer { get; private set; }

		public ShellItemRenderer(IShellContext context)
		{
			this.DisableiOS18ToolbarTabs();
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
					MoreNavigationController?.PopToRootViewController(false);
				}

				if (ReferenceEquals(value, MoreNavigationController))
				{
					MoreNavigationController.WeakDelegate = this;
				}

				UpdateMoreCellsEnabled();
			}
		}

		[Export("navigationController:didShowViewController:animated:")]
		[Preserve(AllMembers = true)]
		public virtual void DidShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
		{
			var renderer = RendererForViewController(this.SelectedViewController);
			if (renderer != null)
			{
				ShellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, renderer.ShellSection);
				CurrentRenderer = renderer;
			}
			UpdateMoreCellsEnabled();
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

		void IDisconnectable.Disconnect()
		{
			if (_sectionRenderers != null)
			{
				foreach (var kvp in _sectionRenderers.ToList())
				{
					var renderer = kvp.Value as IDisconnectable;
					renderer?.Disconnect();
					kvp.Value.ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;
				}
			}

			if (_displayedPage != null)
				_displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;

			if (_currentSection != null)
				((IShellSectionController)_currentSection).RemoveDisplayedPageObserver(this);


			if (ShellItem != null)
				ShellItem.PropertyChanged -= OnElementPropertyChanged;

			if (_context?.Shell is IShellController shellController)
				shellController.RemoveAppearanceObserver(this);

			if (ShellItemController != null)
				ShellItemController.ItemsCollectionChanged -= OnItemsCollectionChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				(this as IDisconnectable).Disconnect();

				foreach (var kvp in _sectionRenderers.ToList())
				{
					var renderer = kvp.Value;
					RemoveRenderer(renderer);
				}

				_sectionRenderers.Clear();
				CurrentRenderer = null;
				_shellItem = null;
				_currentSection = null;
				_displayedPage = null;
			}

			base.Dispose(disposing);
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
						CustomizableViewControllers = Array.Empty<UIViewController>();
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
				CustomizableViewControllers = Array.Empty<UIViewController>();

				if (goTo)
					GoTo(ShellItem.CurrentItem);
			}

			UpdateTabBarHidden();
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
			if (ShellItem.CurrentItem == null)
				throw new InvalidOperationException($"Content not found for active {ShellItem}. Title: {ShellItem.Title}. Route: {ShellItem.Route}.");

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
			CustomizableViewControllers = Array.Empty<UIViewController>();

			UpdateTabBarHidden();

			// Make sure we are at the right item
			GoTo(ShellItem.CurrentItem);
			UpdateMoreCellsEnabled();
		}

		void UpdateMoreCellsEnabled()
		{
			var moreNavigationCells = GetMoreNavigationCells();
			var viewControllersLength = ViewControllers.Length;
			// now that they are applied we can set the enabled state of the TabBar items
			for (int i = 4; i < viewControllersLength; i++)
			{
				if ((i - 4) >= (moreNavigationCells.Length))
				{
					break;
				}

				var renderer = RendererForViewController(ViewControllers[i]);
				var cell = moreNavigationCells[i - 4];

#pragma warning disable CA1416, CA1422 // TODO: 'UITableViewCell.TextLabel' is unsupported on: 'ios' 14.0 and later
				if (!renderer.ShellSection.IsEnabled)
				{
					cell.UserInteractionEnabled = false;

					if (_defaultMoreTextLabelTextColor == null)
						_defaultMoreTextLabelTextColor = cell.TextLabel.TextColor;

					cell.TextLabel.TextColor = Color.FromRgb(213, 213, 213).ToPlatform();
				}
				else if (!cell.UserInteractionEnabled)
				{
					cell.UserInteractionEnabled = true;
					cell.TextLabel.TextColor = _defaultMoreTextLabelTextColor;
				}
#pragma warning restore CA1416, CA1422
			}

			UITableViewCell[] GetMoreNavigationCells()
			{
				if (MoreNavigationController.TopViewController.View is UITableView uITableView && uITableView.Window is not null)
					return uITableView.VisibleCells;

				return EmptyUITableViewCellArray;
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
				UpdateTabBarHidden();
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

			renderer?.Dispose();

			if (CurrentRenderer == renderer)
				CurrentRenderer = null;
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

		public override void ViewWillLayoutSubviews()
		{
			UpdateTabBarHidden();
			base.ViewWillLayoutSubviews();
		}

		void UpdateTabBarHidden()
		{
			if (ShellItemController == null)
				return;

			TabBar.Hidden = !ShellItemController.ShowTabs;
		}
	}
}
