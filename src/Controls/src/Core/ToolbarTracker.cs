using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	internal class ToolbarTracker : MenuItemTracker<ToolbarItem>
	{
		ToolBarItemComparer _toolBarItemComparer;
		protected override IList<ToolbarItem> GetMenuItems(Page page) =>
			page.ToolbarItems;

		protected override IComparer<ToolbarItem> CreateComparer() =>
			_toolBarItemComparer ??= new ToolBarItemComparer();

		class ToolBarItemComparer : IComparer<ToolbarItem>
		{
			public int Compare(ToolbarItem x, ToolbarItem y) => x.Priority.CompareTo(y.Priority);
		}
	}

	internal class MenuBarTracker : MenuItemTracker<MenuBarItem>
	{
		MenuBarItemComparer _Comparer;
		MenuBar _menuBar;
		Element _parent;
		string _handlerProperty;

		public MenuBarTracker(Element parent, string handlerProperty)
		{
			_parent = parent;
			_handlerProperty = handlerProperty;
			CollectionChanged += OnMenuBarItemCollectionChanged;
		}

		void OnMenuBarItemCollectionChanged(object sender, EventArgs e)
		{
			_parent?.Handler?.UpdateValue(_handlerProperty);
		}

		public MenuBar MenuBar
		{
			get
			{
				var menuBarItems = ToolbarItems;
				if (menuBarItems.Count == 0)
					return null;

				_menuBar ??= new MenuBar();
				_menuBar.ReplaceWith(ToolbarItems);

				if (_menuBar.Parent != _parent)
					_menuBar.Parent = _parent;

				return _menuBar;
			}
		}


		protected override IList<MenuBarItem> GetMenuItems(Page page) =>
			page.MenuBarItems;

		protected override IComparer<MenuBarItem> CreateComparer() =>
			_Comparer ??= new MenuBarItemComparer();

		class MenuBarItemComparer : IComparer<MenuBarItem>
		{
			public int Compare(MenuBarItem x, MenuBarItem y) => x.Priority.CompareTo(y.Priority);
		}
	}

	internal abstract class MenuItemTracker<TMenuItem>
		where TMenuItem : BaseMenuItem
	{
		int _flyoutDetails;
		Page _target;
		public MenuItemTracker()
		{
		}

		protected abstract IList<TMenuItem> GetMenuItems(Page page);

		protected abstract IComparer<TMenuItem> CreateComparer();

		public IEnumerable<Page> AdditionalTargets { get; set; }

		public bool HaveFlyoutPage
		{
			get { return _flyoutDetails > 0; }
		}

		public bool SeparateFlyoutPage { get; set; }

		public Page Target
		{
			get { return _target; }
			set
			{
				if (_target == value)
					return;

				UntrackTarget(_target);
				_target = value;

				if (_target != null)
					TrackTarget(_target);
				EmitCollectionChanged();
			}
		}

		public IReadOnlyList<TMenuItem> ToolbarItems
		{
			get
			{
				if (Target == null)
					return new TMenuItem[0];

				// I realize this is sorting on every single get but we don't have 
				// a mechanism in place currently to invalidate a stored version of this

				List<TMenuItem> returnValue = GetCurrentToolbarItems(Target);

				if (AdditionalTargets != null)
					foreach (var item in AdditionalTargets)
						foreach (var menuITem in GetMenuItems(item))
							returnValue.Add(menuITem);

				returnValue.Sort(CreateComparer());
				return returnValue;
			}
		}

		public event EventHandler CollectionChanged;

		void EmitCollectionChanged()
			=> CollectionChanged?.Invoke(this, EventArgs.Empty);

		List<TMenuItem> GetCurrentToolbarItems(Page page)
		{
			var result = new List<TMenuItem>();
			result.AddRange(GetMenuItems(page));

			if (page is FlyoutPage)
			{
				var flyoutDetail = (FlyoutPage)page;
				if (SeparateFlyoutPage)
				{
					if (flyoutDetail.IsPresented)
					{
						if (flyoutDetail.Flyout != null)
							result.AddRange(GetCurrentToolbarItems(flyoutDetail.Flyout));
					}
					else
					{
						if (flyoutDetail.Detail != null)
							result.AddRange(GetCurrentToolbarItems(flyoutDetail.Detail));
					}
				}
				else
				{
					if (flyoutDetail.Flyout != null)
						result.AddRange(GetCurrentToolbarItems(flyoutDetail.Flyout));
					if (flyoutDetail.Detail != null)
						result.AddRange(GetCurrentToolbarItems(flyoutDetail.Detail));
				}
			}
			else if (page is IPageContainer<Page>)
			{
				var container = (IPageContainer<Page>)page;
				if (container.CurrentPage != null && container.CurrentPage != container)
					result.AddRange(GetCurrentToolbarItems(container.CurrentPage));
			}

			return result;
		}

		void OnChildAdded(object sender, ElementEventArgs eventArgs)
		{
			var page = eventArgs.Element as Page;
			if (page == null)
				return;

			RegisterChildPage(page);
		}

		void OnChildRemoved(object sender, ElementEventArgs eventArgs)
		{
			var page = eventArgs.Element as Page;
			if (page == null)
				return;

			UnregisterChildPage(page);
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			EmitCollectionChanged();
		}

		void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == NavigationPage.CurrentPageProperty.PropertyName || propertyChangedEventArgs.PropertyName == FlyoutPage.IsPresentedProperty.PropertyName ||
				propertyChangedEventArgs.PropertyName == "Detail" || propertyChangedEventArgs.PropertyName == "Flyout")
			{
				EmitCollectionChanged();
			}
		}

		void RegisterChildPage(Page page)
		{
			if (page is FlyoutPage)
				_flyoutDetails++;

			((ObservableCollection<ToolbarItem>)page.ToolbarItems).CollectionChanged += OnCollectionChanged;
			page.PropertyChanged += OnPropertyChanged;
		}

		void TrackTarget(Page page)
		{
			if (page == null)
				return;

			if (page is FlyoutPage)
				_flyoutDetails++;

			((ObservableCollection<ToolbarItem>)page.ToolbarItems).CollectionChanged += OnCollectionChanged;
			page.Descendants().OfType<Page>().ForEach(RegisterChildPage);

			page.DescendantAdded += OnChildAdded;
			page.DescendantRemoved += OnChildRemoved;
			page.PropertyChanged += OnPropertyChanged;
		}

		void UnregisterChildPage(Page page)
		{
			if (page is FlyoutPage)
				_flyoutDetails--;

			((ObservableCollection<ToolbarItem>)page.ToolbarItems).CollectionChanged -= OnCollectionChanged;
			page.PropertyChanged -= OnPropertyChanged;
		}

		void UntrackTarget(Page page)
		{
			if (page == null)
				return;

			if (page is FlyoutPage)
				_flyoutDetails--;

			((ObservableCollection<ToolbarItem>)page.ToolbarItems).CollectionChanged -= OnCollectionChanged;
			page.Descendants().OfType<Page>().ForEach(UnregisterChildPage);

			page.DescendantAdded -= OnChildAdded;
			page.DescendantRemoved -= OnChildRemoved;
			page.PropertyChanged -= OnPropertyChanged;
		}
	}
}