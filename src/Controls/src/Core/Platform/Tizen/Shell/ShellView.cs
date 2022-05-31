#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using ElmSharp;
using Microsoft.Extensions.DependencyInjection;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using EBox = ElmSharp.Box;
using EColor = ElmSharp.Color;
using ITNavigationView = Tizen.UIExtensions.ElmSharp.INavigationView;
using TCollectionView = Tizen.UIExtensions.ElmSharp.CollectionView;
using TImage = Tizen.UIExtensions.ElmSharp.Image;
using TNavigationView = Tizen.UIExtensions.ElmSharp.NavigationView;
using TSelectedItemChangedEventArgs = Tizen.UIExtensions.ElmSharp.SelectedItemChangedEventArgs;
using TThemeConstants = Tizen.UIExtensions.ElmSharp.ThemeConstants;
using TDPExtensions = Tizen.UIExtensions.ElmSharp.DPExtensions;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellView : EBox, IFlyoutBehaviorObserver
	{
		// TODO: NET7 remove this for net7.0
		public static readonly EColor DefaultBackgroundColor = TThemeConstants.Shell.ColorClass.DefaultBackgroundColor;
		public static readonly EColor DefaultForegroundColor = TThemeConstants.Shell.ColorClass.DefaultForegroundColor;
		public static readonly EColor DefaultTitleColor = TThemeConstants.Shell.ColorClass.DefaultTitleColor;

		INavigationDrawer _navigationDrawer;
		ITNavigationView _navigationView;
		FlyoutHeaderBehavior _headerBehavior;

		List<List<Element>>? _cachedGroups;

		View? _headerView;
		View? _footerView;
		TCollectionView _itemsView;

		Element? _lastSelected;
		ShellItemView? _currentShellItem;

		public ShellView(EvasObject parent) : base(parent)
		{
			PlatformParent = parent;
			_navigationDrawer = CreateNavigationDrawer();
			_navigationView = CreateNavigationView();
			_navigationView.LayoutUpdated += OnNavigationViewLayoutUpdated;
			_navigationView.Content = _itemsView = CreateItemsView();
			_navigationDrawer.DrawerWidth = ThemeConstants.Shell.Resources.DefaultFlyoutItemWidth;

			_navigationDrawer.NavigationView = _navigationView.TargetView;
			_navigationDrawer.Toggled += OnDrawerToggled;

			_navigationDrawer.TargetView.SetAlignment(-1.0, -1.0);
			_navigationDrawer.TargetView.SetWeight(1.0, 1.0);
			_navigationDrawer.TargetView.Show();

			Show();
			PackEnd(_navigationDrawer.TargetView);
		}

		public IMauiContext? MauiContext { get; private set; }

		protected EvasObject PlatformParent { get; private set; }

		protected Shell? Element { get; private set; }

		protected TCollectionView ItemsView => _itemsView;

		protected ITNavigationView NavigationView => _navigationView;

		protected bool HeaderOnMenu => _headerBehavior == FlyoutHeaderBehavior.Scroll || _headerBehavior == FlyoutHeaderBehavior.CollapseOnScroll;

		public virtual void SetElement(Shell shell, IMauiContext context)
		{
			Element = shell;
			Element.PropertyChanged += OnElementPropertyChanged;
			MauiContext = context;

			((IShellController)Element).StructureChanged += OnShellStructureChanged;
			((IShellController)Element).AddFlyoutBehaviorObserver(this);
			_lastSelected = null;

			UpdateFlyoutIsPresented();
			UpdateFlyoutBackgroundColor();
			UpdateFlyoutBackgroundImage();
			UpdateCurrentItem();
			UpdateFlyoutHeader();
			UpdateFooter();
		}

		protected virtual ShellItemView CreateShellItemView(ShellItem item)
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			return new ShellItemView(item, MauiContext);
		}

		protected virtual INavigationDrawer CreateNavigationDrawer()
		{
			return new NavigationDrawer(PlatformParent);
		}

		protected virtual ITNavigationView CreateNavigationView()
		{
			return new TNavigationView(PlatformParent);
		}

		protected virtual TCollectionView CreateItemsView()
		{
			return new TCollectionView(PlatformParent)
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
		}

		protected virtual ItemAdaptor GetItemAdaptor(IEnumerable items)
		{
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			return new ShellFlyoutItemAdaptor(Element, MauiContext, items, HeaderOnMenu);
		}

		protected virtual void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
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
			else if (e.PropertyName == Shell.FlyoutBackgroundImageProperty.PropertyName)
			{
				UpdateFlyoutBackgroundImage();
			}
			else if (e.PropertyName == Shell.FlyoutBackgroundImageProperty.PropertyName)
			{
				UpdateFlyoutBackgroundImageAspect();
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
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");

			// It is workaround of Panel.IsOpen bug, Panel.IsOpen property is not working when layouting was triggered
			Application.Current?.Dispatcher.Dispatch(() =>
			{
				_navigationDrawer.IsOpen = Element.FlyoutIsPresented;
			});
		}

		// TODO: NET7 make this public for net7.0
		internal virtual void UpdateFlyoutWidth()
		{
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");

			_navigationDrawer.DrawerWidth = Element.FlyoutWidth;
		}

		protected void OnDrawerToggled(object? sender, EventArgs e)
		{
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");

			Element.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, _navigationDrawer.IsOpen);
		}

		protected virtual void UpdateFlyoutBehavior()
		{
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");

			_navigationDrawer.DrawerBehavior = Element.FlyoutBehavior.ToPlatform();
		}

		protected virtual void BuildMenu()
		{
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");

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

			ItemsView.Adaptor = GetItemAdaptor(items);
			ItemsView.Adaptor.ItemSelected += OnItemSelected;
		}

		protected virtual void UpdateFlyoutHeader()
		{
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

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
					_navigationView.Header = _headerView.ToPlatform(MauiContext);
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
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (_footerView != null)
			{
				_footerView.MeasureInvalidated -= OnFooterSizeChanged;
				_footerView = null;
			}

			_footerView = (Element as IShellController).FlyoutFooter;

			if (_footerView != null)
			{
				_navigationView.Footer = _footerView.ToPlatform(MauiContext);
				_footerView.MeasureInvalidated += OnFooterSizeChanged;
			}
			else
			{
				_navigationView.Footer = null;
			}
		}

		void OnShellStructureChanged(object? sender, EventArgs e)
		{
			BuildMenu();
		}

		void OnItemSelected(object? sender, TSelectedItemChangedEventArgs e)
		{
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");

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
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");

			_currentShellItem?.Dispose();
			if (Element.CurrentItem != null)
			{
				_currentShellItem = CreateShellItemView(Element.CurrentItem);
				_navigationDrawer.Main = _currentShellItem.PlatformView;
			}
			else
			{
				_navigationDrawer.Main = null;
			}
		}

		void UpdateFlyoutBackgroundColor()
		{
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");

			_navigationView.BackgroundColor = Element.FlyoutBackgroundColor.ToPlatformEFL();
		}

		async void UpdateFlyoutBackgroundImage()
		{
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (Element.FlyoutBackgroundImage != null)
			{
				var image = new TImage(PlatformParent);
				var imageSource = Element.FlyoutBackgroundImage;
				var provider = MauiContext.Services.GetRequiredService<IImageSourceServiceProvider>();
				var service = provider.GetRequiredImageSourceService(imageSource);
				image.Aspect = Element.FlyoutBackgroundImageAspect.ToPlatform();
				_navigationView.BackgroundImage = image;

				await service.GetImageAsync(imageSource, image);
			}
			else
			{
				_navigationView.BackgroundImage = null;
			}
		}

		void UpdateFlyoutBackgroundImageAspect()
		{
			_ = Element ?? throw new InvalidOperationException($"{nameof(Element)} should have been set by base class.");

			if (_navigationView.BackgroundImage is TImage image)
			{
				image.Aspect = Element.FlyoutBackgroundImageAspect.ToPlatform();
			}
		}

		void OnNavigationViewLayoutUpdated(object? sender, LayoutEventArgs args)
		{
			UpdateHeaderLayout(args.Geometry.Width, args.Geometry.Height);
			UpdateFooterLayout(args.Geometry.Width, args.Geometry.Height);
		}

		void OnHeaderSizeChanged(object? sender, EventArgs e)
		{
			var bound = (_navigationView as EvasObject)?.Geometry;
			Application.Current?.Dispatcher.Dispatch(() =>
			{
				UpdateHeaderLayout((bound?.Width).GetValueOrDefault(), (bound?.Height).GetValueOrDefault());
			});
		}

		void OnFooterSizeChanged(object? sender, EventArgs e)
		{
			var bound = (_navigationView as EvasObject)?.Geometry;
			Application.Current?.Dispatcher.Dispatch(() =>
			{
				UpdateFooterLayout((bound?.Width).GetValueOrDefault(), (bound?.Height).GetValueOrDefault());
			});
		}

		void UpdateHeaderLayout(double widthConstraint, double heightConstraint)
		{
			if ((!HeaderOnMenu) && (_headerView != null))
			{
				var requestSize = _headerView.Measure(widthConstraint, heightConstraint);
				if (_navigationView.Header != null)
					_navigationView.Header.MinimumHeight = TDPExtensions.ConvertToScaledPixel(requestSize.Request.Height);
			}
		}

		void UpdateFooterLayout(double widthConstraint, double heightConstraint)
		{
			if (_footerView != null)
			{
				var requestSize = _footerView.Measure(widthConstraint, heightConstraint);
				if (_navigationView.Footer != null)
					_navigationView.Footer.MinimumHeight = TDPExtensions.ConvertToScaledPixel(requestSize.Request.Height);
			}
		}

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			UpdateFlyoutBehavior();
		}
	}
}
