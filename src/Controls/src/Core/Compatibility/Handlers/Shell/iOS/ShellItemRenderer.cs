#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
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

		public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
		{
			if (previousTraitCollection.VerticalSizeClass == TraitCollection.VerticalSizeClass)
				return;

			foreach (var item in TabBar.Items)
			{
				item.Image = TabbedViewExtensions.AutoResizeTabBarImage(TraitCollection, item.Image);
			}
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_appearanceTracker?.UpdateLayout(this);
			UpdateNavBarHidden();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			ShouldSelectViewController = (tabController, viewController) =>
			{
				bool accept = true;
				var renderer = RendererForViewController(viewController);
				if (renderer is not null)
				{
					// On iOS 26+, disabled tabs can still be selected by dragging.
					// Return false to prevent selecting disabled tabs.
					if (!renderer.ShellSection.IsEnabled && (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26)))
					{
						return false;
					}

					accept = ((IShellItemController)ShellItem).ProposeSection(renderer.ShellSection, false);
				}

				return accept;
			};
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			ApplyInitialDisabledState();
		}

		void ApplyInitialDisabledState()
		{
			if (TabBar.Items is null)
				return;

			var items = ShellItemController?.GetItems();
			if (items is null)
				return;

			for (int i = 0; i < items.Count && i < TabBar.Items.Length; i++)
			{
				if (!items[i].IsEnabled)
					UpdateTabBarItemEnabled(TabBar.Items[i], false);
			}
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

				// Apply initial IsEnabled state for each tab item
				SetTabItemsEnabledState();

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
				if (TabBar.Items is not null && index >= 0 && index < TabBar.Items.Length)
					UpdateTabBarItemEnabled(TabBar.Items[index], shellSection.IsEnabled);
			}
		}

		void UpdateTabBarItemEnabled(UITabBarItem tabBarItem, bool isEnabled)
		{
			tabBarItem.Enabled = isEnabled;

			var disabledColor = Shell.GetTabBarDisabledColor(_context.Shell)?.ToPlatform();
			if (disabledColor is null)
				return;

			// Set text color for disabled state - needed for both text-only and icon tabs
			// Global UITabBarAppearance.Disabled handles initial state, but per-item is needed for dynamic changes
			var textAttributes = isEnabled ? null : new UIStringAttributes { ForegroundColor = disabledColor };
			tabBarItem.SetTitleTextAttributes(textAttributes, UIControlState.Normal);
			tabBarItem.SetTitleTextAttributes(textAttributes, UIControlState.Disabled);

			// For tabs with icons, also tint the icon image
			// UITabBarAppearance.Disabled.IconColor doesn't work for actual image tinting
			if (tabBarItem.Image is not null)
			{
				tabBarItem.Image = isEnabled
					? tabBarItem.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate)
					: CreateTintedImage(tabBarItem.Image, disabledColor).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
			}
		}

		UIImage CreateTintedImage(UIImage image, UIColor color)
		{
			var renderer = new UIGraphicsImageRenderer(image.Size, new UIGraphicsImageRendererFormat { Opaque = false, Scale = image.CurrentScale });
			return renderer.CreateImage(ctx =>
			{
				image.Draw(new CGRect(CGPoint.Empty, image.Size));
				color.SetFill();
				ctx.FillRect(new CGRect(CGPoint.Empty, image.Size), CGBlendMode.SourceIn);
			});
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

		void SetTabItemsEnabledState()
		{
			if (TabBar?.Items is null)
			{
				return;
			}

			var items = ShellItemController.GetItems();
			if (items is null)
			{
				return;
			}

			if (TabBar.Items.Length >= items.Count)
			{
				for (int tabIndex = 0; tabIndex < items.Count; tabIndex++)
				{
					TabBar.Items[tabIndex].Enabled = items[tabIndex].IsEnabled;
				}
			}
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

			// Apply initial IsEnabled state for newly added tab items
			SetTabItemsEnabledState();

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
				UpdateLargeTitles();
				UpdateNavBarHidden();
			}
		}

		void OnDisplayedPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.TabBarIsVisibleProperty.PropertyName)
				UpdateTabBarHidden();
		}


		void UpdateLargeTitles()
		{
			var page = _displayedPage;
			if (page is null || !OperatingSystem.IsIOSVersionAtLeast(11))
				return;

			var largeTitleDisplayMode = page.OnThisPlatform().LargeTitleDisplay();

			if (SelectedViewController is UINavigationController navigationController)
			{
				navigationController.NavigationBar.PrefersLargeTitles = largeTitleDisplayMode == LargeTitleDisplayMode.Always;
				var top = navigationController.TopViewController;
				if (top is not null)
				{
					top.NavigationItem.LargeTitleDisplayMode = largeTitleDisplayMode switch
					{
						LargeTitleDisplayMode.Always => UINavigationItemLargeTitleDisplayMode.Always,
						LargeTitleDisplayMode.Automatic => UINavigationItemLargeTitleDisplayMode.Automatic,
						_ => UINavigationItemLargeTitleDisplayMode.Never
					};
				}
			}
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
			UpdateLargeTitles();
			base.ViewWillLayoutSubviews();
		}

		void UpdateNavBarHidden()
		{
			if (SelectedViewController is UINavigationController navigationController && _displayedPage is not null)
			{
				navigationController.SetNavigationBarHidden(!Shell.GetNavBarIsVisible(_displayedPage), Shell.GetNavBarVisibilityAnimationEnabled(_displayedPage));
			}
		}

		void UpdateTabBarHidden()
		{
			if (ShellItemController == null)
				return;

			if (OperatingSystem.IsMacCatalystVersionAtLeast(18) || OperatingSystem.IsIOSVersionAtLeast(18))
			{
#if MACCATALYST
				if (TabBar != null && TabBar.Hidden != !ShellItemController.ShowTabs)
				{
					// Root Cause: On MacCatalyst 18+, DisableiOS18ToolbarTabs() sets Mode = TabSidebar 
					// which causes iOS to set TabBar.Hidden = true and Alpha = 0 by the system.
					// This is a side effect of TabSidebar mode when there's no sidebar to show.

					// Explicitly set Alpha and Hidden to override this incorrect system behavior.
					TabBar.Alpha = 1.0f;
					TabBar.Hidden = !ShellItemController.ShowTabs;
				}
#endif

				if (TabBarHidden == !ShellItemController.ShowTabs)
				{
					return;
				}

				TabBarHidden = !ShellItemController.ShowTabs;
			}
			else
			{
				TabBar.Hidden = !ShellItemController.ShowTabs;
			}
		}
	}
}
