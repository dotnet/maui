#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using ElmSharp;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using EBox = ElmSharp.Box;
using TCollectionView = Tizen.UIExtensions.ElmSharp.CollectionView;
using TNavigationView = Tizen.UIExtensions.ElmSharp.NavigationView;
using ITCollectionViewController = Tizen.UIExtensions.ElmSharp.ICollectionViewController;
using TSelectedItemChangedEventArgs = Tizen.UIExtensions.ElmSharp.SelectedItemChangedEventArgs;


namespace Microsoft.Maui.Controls.Platform
{
	public class TVShellSectionHandler : IShellSectionHandler, IDisposable
	{
		EBox _mainLayout;
		EBox _contentArea;
		EvasObject? _currentContent = null;

		TNavigationView? _navigationView;
		TCollectionView? _itemsView;

		IList<ShellContent>? _cachedItems;
		Dictionary<ShellContent, EvasObject> _contentCache = new Dictionary<ShellContent, EvasObject>();

		bool _disposed = false;

		bool _drawerIsVisible => (ShellSection != null) ? (ShellSection.Items.Count > 1) : false;

		public TVShellSectionHandler(ShellSection section, IMauiContext context)
		{
			ShellSection = section;
			MauiContext = context;
			ShellSection.PropertyChanged += OnSectionPropertyChanged;
			if (ShellSection.Items is INotifyCollectionChanged collection)
			{
				collection.CollectionChanged += OnShellSectionCollectionChanged;
			}

			_mainLayout = new EBox(NativeParent);
			_mainLayout.SetLayoutCallback(OnLayout);

			_contentArea = new EBox(NativeParent);
			_contentArea.Show();
			_mainLayout.PackEnd(_contentArea);

			UpdateSectionItems();
			UpdateCurrentItem(ShellSection.CurrentItem);
		}

		public ShellSection ShellSection { get; }

		public EvasObject PlatformView
		{
			get
			{
				return _mainLayout;
			}
		}

		protected IMauiContext MauiContext { get; private set; }

		protected TCollectionView? ItemsView => _itemsView;

		protected EvasObject? NativeParent
		{
			get => MauiContext.GetNativeParent();
		}

		~TVShellSectionHandler()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (ShellSection != null)
				{
					ShellSection.PropertyChanged -= OnSectionPropertyChanged;
				}

				PlatformView.Unrealize();
			}
			_disposed = true;
		}

		protected virtual TCollectionView CreateItemsView()
		{
			_ = NativeParent ?? throw new InvalidOperationException($"{nameof(NativeParent)} should have been set by base class.");

			return new TCollectionView(NativeParent)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
				SelectionMode = CollectionViewSelectionMode.Single,
				HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible,
				VerticalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible,
				LayoutManager = new LinearLayoutManager(false)
			};
		}

		void OnNavigationViewSelectedItemChanged(object sender, ItemSelectedEventArgs e)
		{
			if (e.SelectedItem == null)
				return;

			var content = e.SelectedItem;
			if (ShellSection.CurrentItem != content)
			{
				ShellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, content);
			}
		}

		void OnSectionPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				UpdateCurrentItem(ShellSection.CurrentItem);
			}
		}

		void UpdateSectionItems()
		{
			if (!_drawerIsVisible)
			{
				return;
			}

			if (_navigationView == null && NativeParent != null)
			{
				_navigationView = new TVNavigationView(NativeParent);
				_navigationView.SetAlignment(-1, -1);
				_navigationView.SetWeight(1, 1);
				_navigationView.Show();
				_mainLayout.PackStart(_navigationView);

				_navigationView.LayoutUpdated += (s, e) =>
				{
					var drawerBound = e.Geometry;
					var drawerWidth = GetDrawerWidth();
				};

				_navigationView.Content = _itemsView = CreateItemsView();
			}

			BuildMenu();
		}

		bool IsItemChanged(IList<ShellContent> items)
		{
			if (_cachedItems == null)
				return true;

			if (_cachedItems.Count != items.Count)
				return true;

			for (int i = 0; i < items.Count; i++)
			{
				if (_cachedItems[i] != items[i])
					return true;
			}

			_cachedItems = items;
			return false;
		}

		void BuildMenu()
		{
			var items = ShellSection.Items;

			if (!IsItemChanged(items))
				return;

			_cachedItems = items;

			if (ItemsView != null)
			{
				ItemsView.Adaptor = new TVShellItemAdaptor(ShellSection, _navigationView, MauiContext, items, false);
				ItemsView.Adaptor.ItemSelected += OnItemSelected;
			}
		}

		void UpdateCurrentItem(ShellContent content)
		{
			if (_currentContent != null)
			{
				_currentContent.Hide();
				_contentArea.UnPack(_currentContent);
				_currentContent = null;
			}

			if (content == null)
			{
				return;
			}

			if (!_contentCache.ContainsKey(content))
			{
				var native = CreateShellContent(content);
				native.SetAlignment(-1, -1);
				native.SetWeight(1, 1);
				_contentCache[content] = native;
			}
			_currentContent = _contentCache[content];
			_currentContent.Show();
			_contentArea.PackEnd(_currentContent);
		}

		EvasObject CreateShellContent(ShellContent content)
		{
			Page xpage = ((IShellContentController)content).GetOrCreateContent();
			return xpage.ToPlatform(MauiContext);
		}

		void OnLayout()
		{
			if (PlatformView.Geometry.Width == 0 || PlatformView.Geometry.Height == 0)
				return;

			var bound = PlatformView.Geometry;
			var drawerWidth = 0;

			if (_drawerIsVisible && _navigationView != null)
			{
				var drawerBound = bound;
				drawerWidth = GetDrawerWidth();
				drawerBound.Width = drawerWidth;

				_navigationView.Geometry = drawerBound;
			}

			var contentBound = bound;

			contentBound.X += drawerWidth;
			contentBound.Width -= drawerWidth;
			_contentArea.Geometry = contentBound;
		}

		int GetDrawerWidth()
		{
			_ = NativeParent ?? throw new InvalidOperationException($"{nameof(NativeParent)} should have been set by base class.");

			int width = 0;
			if (ItemsView is ITCollectionViewController controller)
				width = controller.GetItemSize((NativeParent.Geometry.Width / 2), NativeParent.Geometry.Height).Width;

			return width;
		}

		void OnItemSelected(object? sender, TSelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem == null)
				return;

			var content = e.SelectedItem;
			if (ShellSection.CurrentItem != content)
			{
				ShellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, content);
			}
		}

		void OnShellSectionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateSectionItems();
		}
	}
}