#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;
using GColor = Microsoft.Maui.Graphics.Color;
using NCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using NColor = Tizen.NUI.Color;
using NView = Tizen.NUI.BaseComponents.View;
using TDrawerBehavior = Tizen.UIExtensions.Common.DrawerBehavior;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellView : ViewGroup, IAppearanceObserver, IFlyoutBehaviorObserver, IToolbarContainer
	{
		INavigationDrawer _navigationDrawer;

		INavigationView _navigationView;
		INavigationContentView _navigationContentView;

		View? _headerView;
		FlyoutHeaderBehavior _headerBehavior;

		IView? _flyoutView;

		MauiToolbar? _toolbar;

		NCollectionView? _itemsView;
		ItemTemplateAdaptor? _adaptor;

		ShellItemHandler? _currentItemHandler;

		bool _isOpen;

		protected Shell? Element { get; set; }

		protected IShellController ShellController => (Element as IShellController)!;

		protected IMauiContext? MauiContext { get; private set; }

		protected bool HeaderOnMenu => _headerBehavior == FlyoutHeaderBehavior.Scroll || _headerBehavior == FlyoutHeaderBehavior.CollapseOnScroll;

		protected NColor DefaultBackgroundCorlor = NColor.White;

		public event EventHandler? Toggled;

		public ShellView() : base()
		{
			_navigationDrawer = CreateNavigationDrawer();

			_navigationView = CreateNavigationView();
			_navigationDrawer.Drawer = _navigationView.TargetView;

			_navigationContentView = CreateNavigationContentView();
			_navigationDrawer.Content = _navigationContentView.TargetView;

			_navigationDrawer.Toggled += (s, e) =>
			{
				_isOpen = _navigationDrawer.IsOpened;
				Toggled?.Invoke(this, EventArgs.Empty);
			};

			Children.Add((NView)_navigationDrawer);
		}

		public bool IsOpened
		{
			get => _isOpen;
			set
			{
				if (_isOpen != value)
					_isOpen = value;

				if (value)
					_ = _navigationDrawer.OpenAsync(true);
				else
					_ = _navigationDrawer.CloseAsync(true);
			}
		}

		public void SetElement(Shell shell, IMauiContext context)
		{
			_ = shell ?? throw new ArgumentNullException($"{nameof(shell)} cannot be null here.");
			_ = context ?? throw new ArgumentNullException($"{nameof(context)} cannot be null here.");

			Element = shell;
			MauiContext = context;

			Element.Toolbar.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == "BackButtonVisible")
				{
					UpdetDrawerToggleVisible();
				}
			};
		}

		public void UpdateFlyoutBehavior(FlyoutBehavior flyoutBehavior)
		{
			_navigationDrawer.DrawerBehavior = flyoutBehavior.ToPlatform();
			UpdetDrawerToggleVisible();

			if (_navigationDrawer.DrawerBehavior == TDrawerBehavior.Drawer)
				_ = _navigationDrawer.CloseAsync(false);
		}

		public void UpdateDrawerWidth(double drawerwidth)
		{
			_navigationDrawer.DrawerWidth = drawerwidth.ToScaledPixel();
		}

		public void UpdateFlyout(IView? flyout)
		{
			_flyoutView = flyout;

			if (_flyoutView != null)
				_navigationView.Content = _flyoutView.ToPlatform(MauiContext!);
		}

		public void UpdateBackgroundColor(GColor? color)
		{
			_navigationView.BackgroundColor = color?.ToNUIColor() ?? DefaultBackgroundCorlor;
		}

		public void UpdateCurrentItem(ShellItem newItem, bool animate = true)
		{
			if (_currentItemHandler != null)
				_currentItemHandler.Dispose();

			if (newItem != null)
			{
				_currentItemHandler = (ShellItemHandler)newItem.ToHandler(MauiContext!);
				_navigationContentView.Content = newItem.ToPlatform(MauiContext!);
			}
		}

		public void UpdateFlyoutFooter(Shell shell)
		{
			_navigationView.Footer = ShellController.FlyoutFooter?.ToPlatform(MauiContext!);
		}

		public void UpdateFlyoutHeader(Shell shell)
		{
			_headerBehavior = shell.FlyoutHeaderBehavior;

			if (_flyoutView != null)
				return;

			// Once _headerView is attached to CollectionView, it will be disposed when the adaptor of CollectionView is changed.
			// This code is to reset handler after NUI view of header is disposed.
			if (_headerView != null && _headerView.Handler is IPlatformViewHandler nativeHandler)
			{
				nativeHandler.Dispose();
				_headerView.Handler = null;
			}

			_headerView = ShellController.FlyoutHeader;

			if (HeaderOnMenu)
			{
				_navigationView.Header = null;
			}
			else
			{
				_navigationView.Header = _headerView?.ToPlatform(MauiContext!);
			}

			UpdateItems();
		}

		public void UpdateItems()
		{
			if (_flyoutView != null)
				return;

			if (_itemsView == null)
			{
				_itemsView = new NCollectionView
				{
					SizeHeight = 60d.ToScaledPixel(),
					WidthSpecification = LayoutParamPolicies.MatchParent,
					LayoutManager = new LinearLayoutManager(false),
					SelectionMode = CollectionViewSelectionMode.SingleAlways,
				};
				_itemsView.ScrollView.HideScrollbar = true;
			}

			if (_adaptor != null)
				_adaptor.SelectionChanged -= OnTabItemSelected;

			_itemsView.Adaptor = _adaptor = CreateItemAdaptor();
			_adaptor.SelectionChanged += OnTabItemSelected;

			_navigationView.Content = _itemsView;
		}

		public void UpdateFlyoutBackDrop(Brush backdrop)
		{
		}

		public void SetToolbar(MauiToolbar toolbar)
		{
			if (_toolbar != null)
			{
				_toolbar.IconPressed -= OnIconPressed;
				_navigationContentView.TitleView = null;
			}

			_toolbar = toolbar;

			if (_toolbar != null)
			{
				_toolbar.IconPressed += OnIconPressed;
				_navigationContentView.TitleView = _toolbar;
			}
		}

		protected virtual INavigationDrawer CreateNavigationDrawer()
		{
			return new NavigationDrawer();
		}

		protected virtual INavigationView CreateNavigationView()
		{
			return new NavigationView();
		}

		protected virtual INavigationContentView CreateNavigationContentView()
		{
			return new NavigationContentView();
		}

		protected virtual ItemTemplateAdaptor CreateItemAdaptor()
		{
			return new ShellFlyoutItemTemplateAdaptor(Element!, Element!.Items, HeaderOnMenu);
		}

		void OnIconPressed(object? sender, EventArgs e)
		{
			if (!Element!.Toolbar.BackButtonVisible && ((Element!.Toolbar.IsVisible)))
				IsOpened = true;
		}

		void UpdetDrawerToggleVisible()
		{
			Element!.Toolbar.DrawerToggleVisible = ((Element!.Toolbar.DrawerToggleVisible) && (Element.FlyoutBehavior == FlyoutBehavior.Flyout));
			_toolbar?.UpdateBackButton(Element!.Toolbar);
		}

		void OnTabItemSelected(object? sender, CollectionViewSelectionChangedEventArgs e)
		{
			if (e.SelectedItems == null || e.SelectedItems.Count == 0 || e.SelectedItems[0] is not ShellItem selectedItem)
				return;

			Element!.CurrentItem = selectedItem;

			if (IsOpened)
				IsOpened = false;
		}

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
		}

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
		}
	}
}
