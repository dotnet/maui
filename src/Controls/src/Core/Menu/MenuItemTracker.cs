#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	internal abstract class MenuItemTracker<TMenuItem>
		where TMenuItem : BaseMenuItem
	{
		int _flyoutDetails;
		WeakReference<Page> _target;
		List<WeakReference<Page>> _additionalTargets = new();
		public MenuItemTracker()
		{
		}

		protected abstract IList<TMenuItem> GetMenuItems(Page page);

		protected abstract IComparer<TMenuItem> CreateComparer();

		public IEnumerable<Page> AdditionalTargets
		{
			get
			{
				foreach (var target in _additionalTargets)
				{
					if (target.TryGetTarget(out var page))
						yield return page;
				}
			}
			set
			{
				_additionalTargets.Clear();
				if (value is not null)
				{
					foreach (var page in value)
					{
						_additionalTargets.Add(new(page));
					}
				}
			}
		}

		public bool HaveFlyoutPage
		{
			get { return _flyoutDetails > 0; }
		}

		public bool SeparateFlyoutPage { get; set; }

		public Page Target
		{
			get => _target is not null && _target.TryGetTarget(out var target) ? target : null;
			set
			{
				var target = Target;
				if (target == value)
					return;

				UntrackTarget(target);
				_target = value is null ? null : new(value);

				if (value != null)
					TrackTarget(value);
				EmitCollectionChanged();
			}
		}

		public IList<TMenuItem> ToolbarItems
		{
			get
			{
				if (Target == null)
					return Array.Empty<TMenuItem>();

				// I realize this is sorting on every single get but we don't have 
				// a mechanism in place currently to invalidate a stored version of this

				List<TMenuItem> returnValue = GetCurrentToolbarItems(Target);

				if (AdditionalTargets != null)
					foreach (var item in AdditionalTargets)
						foreach (var menuItem in GetMenuItems(item))
							if (!returnValue.Contains(menuItem))
								returnValue.Add(menuItem);

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
			else if (page is Shell shell)
			{
				if (shell.GetCurrentShellPage() is Page shellPage && shellPage != shell)
					result.AddRange(GetCurrentToolbarItems(shellPage));
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
			if (propertyChangedEventArgs.PropertyName == NavigationPage.CurrentPageProperty.PropertyName ||
				propertyChangedEventArgs.PropertyName == FlyoutPage.IsPresentedProperty.PropertyName ||
				propertyChangedEventArgs.PropertyName == "Detail" ||
				propertyChangedEventArgs.PropertyName == "Flyout")
			{
				EmitCollectionChanged();
			}

			PagePropertyChanged?.Invoke(sender, propertyChangedEventArgs);
		}

		public event EventHandler<PropertyChangedEventArgs> PagePropertyChanged;
		public event EventHandler<EventArgs> PageAppearing;

		void RegisterChildPage(Page page)
		{
			if (page is FlyoutPage)
				_flyoutDetails++;

			((ObservableCollection<TMenuItem>)GetMenuItems(page)).CollectionChanged += OnCollectionChanged;
			page.PropertyChanged += OnPropertyChanged;
			page.Appearing += OnPageAppearing;
		}

		void OnPageAppearing(object sender, EventArgs e)
		{
			PageAppearing?.Invoke(sender, e);
		}

		void TrackTarget(Page page)
		{
			if (page == null)
				return;

			if (page is FlyoutPage)
				_flyoutDetails++;

			((ObservableCollection<TMenuItem>)GetMenuItems(page)).CollectionChanged += OnCollectionChanged;

			if (page is Shell shell)
			{
				shell.Navigated += OnShellNavigated;
				shell.Navigating += OnShellNavigating;

				if (shell.GetCurrentShellPage() is Page currentShellPage)
					RegisterChildPage(currentShellPage);

				return;
			}

			page.Descendants<Page>().ForEach(RegisterChildPage);

			page.DescendantAdded += OnChildAdded;
			page.DescendantRemoved += OnChildRemoved;
			page.PropertyChanged += OnPropertyChanged;
			page.Appearing += OnPageAppearing;
		}

		void OnShellNavigating(object sender, ShellNavigatingEventArgs e)
		{
			if (((Shell)sender).GetCurrentShellPage() is Page page)
				UnregisterChildPage(page);
		}

		void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
		{
			if (((Shell)sender).GetCurrentShellPage() is Page page)
			{
				UnregisterChildPage(page);
				RegisterChildPage(page);
			}

			EmitCollectionChanged();
		}

		void UnregisterChildPage(Page page)
		{
			if (page is FlyoutPage)
				_flyoutDetails--;

			((ObservableCollection<TMenuItem>)GetMenuItems(page)).CollectionChanged -= OnCollectionChanged;
			page.PropertyChanged -= OnPropertyChanged;
			page.Appearing -= OnPageAppearing;
		}

		void UntrackTarget(Page page)
		{
			if (page == null)
				return;

			if (page is FlyoutPage)
				_flyoutDetails--;

			if (page is Shell shell)
			{
				shell.Navigated -= OnShellNavigated;
				shell.Navigating -= OnShellNavigating;
				return;
			}

			((ObservableCollection<TMenuItem>)GetMenuItems(page)).CollectionChanged -= OnCollectionChanged;
			page.Descendants().OfType<Page>().ForEach(UnregisterChildPage);

			page.DescendantAdded -= OnChildAdded;
			page.DescendantRemoved -= OnChildRemoved;
			page.PropertyChanged -= OnPropertyChanged;
			page.Appearing -= OnPageAppearing;
		}
	}
}