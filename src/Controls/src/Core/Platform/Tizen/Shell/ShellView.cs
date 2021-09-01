using System;
using System.Collections.Generic;
using System.ComponentModel;
using ElmSharp;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using EColor = ElmSharp.Color;
using TINavigationView = Tizen.UIExtensions.ElmSharp.INavigationView;
using TNavigationView = Tizen.UIExtensions.ElmSharp.NavigationView;
using TThemeConstants = Tizen.UIExtensions.ElmSharp.ThemeConstants;
using TCollectionView = Tizen.UIExtensions.ElmSharp.CollectionView;
using TSelectedItemChangedEventArgs = Tizen.UIExtensions.ElmSharp.SelectedItemChangedEventArgs;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellView : NavigationDrawer, IFlyoutBehaviorObserver
	{
		TINavigationView _navigationView;
		FlyoutHeaderBehavior _headerBehavior;

		List<List<Element>> _cachedGroups;

		View _headerView;
		View _footerView;
		TCollectionView _itemsView;

		Element _lastSelected;
		ShellItemView _currentShellItem;

		public static readonly EColor DefaultBackgroundColor = TThemeConstants.Shell.ColorClass.DefaultBackgroundColor;
		public static readonly EColor DefaultForegroundColor = TThemeConstants.Shell.ColorClass.DefaultForegroundColor;
		public static readonly EColor DefaultTitleColor = TThemeConstants.Shell.ColorClass.DefaultTitleColor;

		// The source of icon resources is https://materialdesignicons.com/
		public const string MenuIcon = "";

		protected EvasObject NativeParent { get; private set; }

		protected Shell Element { get; private set; }

		public IMauiContext MauiContext { get; private set; }

		public NavigationDrawer NativeView => this;

		bool HeaderOnMenu => _headerBehavior == FlyoutHeaderBehavior.Scroll ||
							 _headerBehavior == FlyoutHeaderBehavior.CollapseOnScroll;

		public ShellView(EvasObject parent) : base(parent)
		{
			NativeParent = parent;
			_navigationView = CreateNavigationView();
			_navigationView.LayoutUpdated += OnNavigationViewLayoutUpdated;

			NavigationView = _navigationView.TargetView;
			Toggled += OnDrawerToggled;

			_navigationView.Content = CreateItemsView();
		}

		internal void SetElement(Shell shell, IMauiContext context)
		{
			Element = shell;
			Element.PropertyChanged += OnElementPropertyChanged;
			MauiContext = context;

			((IShellController)Element).StructureChanged += OnShellStructureChanged;
			_lastSelected = null;

			UpdateFlyoutIsPresented();
			UpdateCurrentItem();
			UpdateFlyoutHeader();
			UpdateFooter();
		}

		protected virtual ShellItemView CreateShellItemView(ShellItem item)
		{
			return new ShellItemView(item, MauiContext);
		}

		protected virtual TINavigationView CreateNavigationView()
		{
			return new TNavigationView(NativeParent);
		}

		protected virtual EvasObject CreateItemsView()
		{
			_itemsView = new TCollectionView(NativeParent)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
				SelectionMode = CollectionViewSelectionMode.Single,
				HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible,
				VerticalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible,
				LayoutManager = new LinearLayoutManager(false, Tizen.UIExtensions.ElmSharp.ItemSizingStrategy.MeasureFirstItem)
			};

			return _itemsView;
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.CurrentItemProperty.PropertyName)
			{
				UpdateCurrentItem();
			}
			else if (e.PropertyName == Shell.FlyoutIsPresentedProperty.PropertyName)
			{
				UpdateFlyoutIsPresented();
			}
			else if (e.PropertyName == Shell.FlyoutBackgroundColorProperty.PropertyName)
			{
				UpdateFlyoutBackgroundColor();
			}
			else if (e.PropertyName == Shell.FlyoutHeaderProperty.PropertyName)
			{
				UpdateFlyoutHeader();
			}
			else if (e.PropertyName == Shell.FlyoutHeaderTemplateProperty.PropertyName)
			{
				UpdateFlyoutHeader();
			}
			else if (e.PropertyName == Shell.FlyoutHeaderBehaviorProperty.PropertyName)
			{
				UpdateFlyoutHeader();
			}
			else if (e.PropertyName == Shell.FlyoutFooterProperty.PropertyName)
			{
				UpdateFooter();
			}
		}

		protected virtual void UpdateFlyoutIsPresented()
		{
			// It is workaround of Panel.IsOpen bug, Panel.IsOpen property is not working when layouting was triggered
			Device.BeginInvokeOnMainThread(() =>
			{
				IsOpen = Element.FlyoutIsPresented;
			});
		}

		protected void OnDrawerToggled(object sender, EventArgs e)
		{
			Element.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, IsOpen);
		}

		protected virtual void UpdateFlyoutBehavior()
		{
			if (Element.FlyoutBehavior == FlyoutBehavior.Locked)
			{
				IsSplit = true;
			}
			else
			{
				IsSplit = false;
			}
		}

		protected virtual void BuildMenu()
		{
			var groups = ((IShellController)Element).GenerateFlyoutGrouping();

			if (!IsItemChanged(groups) && !HeaderOnMenu)
				return;

			_cachedGroups = groups;

			var items = new List<Element>();

			foreach (var group in groups)
			{
				bool isFirst = true;
				foreach (var item in group)
				{
					items.Add(item);

					// TODO: implements separator
					if (isFirst)
						isFirst = false;
				}
			}

			_itemsView.Adaptor = new ShellFlyoutItemAdaptor(Element, MauiContext, items, HeaderOnMenu);
			_itemsView.Adaptor.ItemSelected += OnItemSelected;
		}

		protected virtual void UpdateFlyoutHeader()
		{
			if (_headerView != null)
			{
				_headerView.MeasureInvalidated -= OnHeaderSizeChanged;
				_headerView = null;
			}

			_headerView = (Element as IShellController).FlyoutHeader;
			_headerBehavior = Element.FlyoutHeaderBehavior;

			BuildMenu();

			if (_headerView != null)
			{
				if (HeaderOnMenu)
				{
					_navigationView.Header = null;
				}
				else
				{
					_navigationView.Header = _headerView?.ToNative(MauiContext);
					_headerView.MeasureInvalidated += OnHeaderSizeChanged;
				}
			}
			else
			{
				_navigationView.Header = null;
			}
		}

		protected virtual void UpdateFooter()
		{
			if (_footerView != null)
			{
				_footerView.MeasureInvalidated -= OnFooterSizeChanged;
				_footerView = null;
			}

			_footerView = (Element as IShellController).FlyoutFooter;

			if (_footerView != null)
			{
				_navigationView.Footer = _footerView?.ToNative(MauiContext);
				_footerView.MeasureInvalidated += OnFooterSizeChanged;
			}
			else
			{
				_navigationView.Footer = null;
			}
		}

		void OnShellStructureChanged(object sender, EventArgs e)
		{
			BuildMenu();
		}

		void OnItemSelected(object sender, TSelectedItemChangedEventArgs e)
		{
			_lastSelected = e.SelectedItem as Element;
			((IShellController)Element).OnFlyoutItemSelected(_lastSelected);
		}

		bool IsItemChanged(List<List<Element>> groups)
		{
			if (_cachedGroups == null)
				return true;

			if (_cachedGroups.Count != groups.Count)
				return true;

			for (int i = 0; i < groups.Count; i++)
			{
				if (_cachedGroups[i].Count != groups[i].Count)
					return true;

				for (int j = 0; j < groups[i].Count; j++)
				{
					if (_cachedGroups[i][j] != groups[i][j])
						return true;
				}
			}

			_cachedGroups = groups;
			return false;
		}

		void UpdateCurrentItem()
		{
			_currentShellItem?.Dispose();
			if (Element.CurrentItem != null)
			{
				_currentShellItem = CreateShellItemView(Element.CurrentItem);
				Main = _currentShellItem.NativeView;
			}
			else
			{
				Main = null;
			}
		}

		void UpdateFlyoutBackgroundColor()
		{
			_navigationView.BackgroundColor = Element.FlyoutBackgroundColor.ToNativeEFL();
		}

		void OnNavigationViewLayoutUpdated(object sender, LayoutEventArgs args)
		{
			UpdateHeaderLayout(args.Geometry.Width, args.Geometry.Height);
			UpdateFooterLayout(args.Geometry.Width, args.Geometry.Height);
		}

		void OnHeaderSizeChanged(object sender, EventArgs e)
		{
			var bound = (_navigationView as EvasObject).Geometry;
			Device.BeginInvokeOnMainThread(()=> {
				UpdateHeaderLayout(bound.Width, bound.Height);
			});
		}

		void OnFooterSizeChanged(object sender, EventArgs e)
		{
			var bound = (_navigationView as EvasObject).Geometry;
			Device.BeginInvokeOnMainThread(() => {
				UpdateFooterLayout(bound.Width, bound.Height);
			});
		}

		void UpdateHeaderLayout(double widthConstraint, double heightConstraint)
		{
			if ((!HeaderOnMenu) && (_headerView != null))
			{
				var requestSize = _headerView.Measure(widthConstraint, heightConstraint);
				_navigationView.Header.MinimumHeight = DPExtensions.ConvertToScaledPixel(requestSize.Request.Height);
			}
		}

		void UpdateFooterLayout(double widthConstraint, double heightConstraint)
		{
			if (_footerView != null)
			{
				var requestSize = _footerView.Measure(widthConstraint, heightConstraint);
				_navigationView.Footer.MinimumHeight = DPExtensions.ConvertToScaledPixel(requestSize.Request.Height);
			}
		}

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			UpdateFlyoutBehavior();
		}
	}
}
