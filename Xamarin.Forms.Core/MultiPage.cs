using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty("Children")]
	public abstract class MultiPage<T> : Page, IViewContainer<T>, IPageContainer<T>, IItemsView<T>, IMultiPageController<T> where T : Page
	{
		public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(MultiPage<>), null);

		public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(MultiPage<>), null);

		public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create("SelectedItem", typeof(object), typeof(MultiPage<>), null, BindingMode.TwoWay);

		internal static readonly BindableProperty IndexProperty = BindableProperty.Create("Index", typeof(int), typeof(Page), -1);

		readonly ElementCollection<T> _children;
		readonly TemplatedItemsList<MultiPage<T>, T> _templatedItems;

		T _current;

		protected MultiPage()
		{
			_templatedItems = new TemplatedItemsList<MultiPage<T>, T>(this, ItemsSourceProperty, ItemTemplateProperty);
			_templatedItems.CollectionChanged += OnTemplatedItemsChanged;

			_children = new ElementCollection<T>(InternalChildren);
			InternalChildren.CollectionChanged += OnChildrenChanged;
		}

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		T IItemsView<T>.CreateDefault(object item)
		{
			return CreateDefault(item);
		}

		void IItemsView<T>.SetupContent(T content, int index)
		{
			SetupContent(content, index);
		}

		void IItemsView<T>.UnhookContent(T content)
		{
			UnhookContent(content);
		}

		public T CurrentPage
		{
			get { return _current; }
			set
			{
				if (_current == value)
					return;

				OnPropertyChanging();
				_current = value;
				OnPropertyChanged();
				OnCurrentPageChanged();
			}
		}

		public IList<T> Children
		{
			get { return _children; }
		}

		public event EventHandler CurrentPageChanged;

		public event NotifyCollectionChangedEventHandler PagesChanged;

		protected abstract T CreateDefault(object item);

		protected override bool OnBackButtonPressed()
		{
			if (CurrentPage != null)
			{
				bool handled = CurrentPage.SendBackButtonPressed();
				if (handled)
					return true;
			}

			return base.OnBackButtonPressed();
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);

			ForceLayout();
		}

		protected virtual void OnCurrentPageChanged()
		{
			EventHandler changed = CurrentPageChanged;
			if (changed != null)
				changed(this, EventArgs.Empty);
		}

		protected virtual void OnPagesChanged(NotifyCollectionChangedEventArgs e)
			=> PagesChanged?.Invoke(this, e);

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (propertyName == ItemsSourceProperty.PropertyName)
				_children.IsReadOnly = ItemsSource != null;
			else if (propertyName == SelectedItemProperty.PropertyName)
			{
				UpdateCurrentPage();
			}
			else if (propertyName == "CurrentPage" && ItemsSource != null)
			{
				if (CurrentPage == null)
				{
					SelectedItem = null;
				}
				else
				{
					int index = _templatedItems.IndexOf(CurrentPage);
					SelectedItem = index != -1 ? _templatedItems.ListProxy[index] : null;
				}
			}

			base.OnPropertyChanged(propertyName);
		}

		protected virtual void SetupContent(T content, int index)
		{
		}

		protected virtual void UnhookContent(T content)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static int GetIndex(T page)
		{
			if (page == null)
				throw new ArgumentNullException("page");

			return (int)page.GetValue(IndexProperty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public T GetPageByIndex(int index)
		{
			foreach (T page in InternalChildren)
			{
				if (index == GetIndex(page))
					return page;
			}
			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetIndex(Page page, int index)
		{
			if (page == null)
				throw new ArgumentNullException("page");

			page.SetValue(IndexProperty, index);
		}

		void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (Children.IsReadOnly)
				return;

			var i = 0;
			foreach (T page in Children)
				SetIndex(page, i++);

			OnPagesChanged(e);

			if (CurrentPage == null || Children.IndexOf(CurrentPage) == -1)
				CurrentPage = Children.FirstOrDefault();
		}

		void OnTemplatedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewStartingIndex < 0)
						goto case NotifyCollectionChangedAction.Reset;

					for (int i = e.NewStartingIndex; i < Children.Count; i++)
						SetIndex((T)InternalChildren[i], i + e.NewItems.Count);

					for (var i = 0; i < e.NewItems.Count; i++)
					{
						var page = (T)e.NewItems[i];
						page.Owned = true;
						int index = i + e.NewStartingIndex;
						SetIndex(page, index);
						InternalChildren.Insert(index, (T)e.NewItems[i]);
					}

					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex < 0)
						goto case NotifyCollectionChangedAction.Reset;

					int removeIndex = e.OldStartingIndex;
					for (int i = removeIndex + e.OldItems.Count; i < Children.Count; i++)
						SetIndex((T)InternalChildren[i], removeIndex++);

					for (var i = 0; i < e.OldItems.Count; i++)
					{
						Element element = InternalChildren[e.OldStartingIndex];
						InternalChildren.RemoveAt(e.OldStartingIndex);
						element.Owned = false;
					}

					break;

				case NotifyCollectionChangedAction.Move:
					if (e.NewStartingIndex < 0 || e.OldStartingIndex < 0)
						goto case NotifyCollectionChangedAction.Reset;

					if (e.NewStartingIndex == e.OldStartingIndex)
						return;

					bool movingForward = e.OldStartingIndex < e.NewStartingIndex;

					if (movingForward)
					{
						int moveIndex = e.OldStartingIndex;
						for (int i = moveIndex + e.OldItems.Count; i <= e.NewStartingIndex; i++)
							SetIndex((T)InternalChildren[i], moveIndex++);
					}
					else
					{
						for (var i = 0; i < e.OldStartingIndex - e.NewStartingIndex; i++)
						{
							var page = (T)InternalChildren[i + e.NewStartingIndex];
							SetIndex(page, GetIndex(page) + e.OldItems.Count);
						}
					}

					for (var i = 0; i < e.OldItems.Count; i++)
						InternalChildren.RemoveAt(e.OldStartingIndex);

					int insertIndex = e.NewStartingIndex;
					if (movingForward)
						insertIndex -= e.OldItems.Count - 1;

					for (var i = 0; i < e.OldItems.Count; i++)
					{
						var page = (T)e.OldItems[i];
						SetIndex(page, insertIndex + i);
						InternalChildren.Insert(insertIndex + i, page);
					}

					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.OldStartingIndex < 0)
						goto case NotifyCollectionChangedAction.Reset;

					for (int i = e.OldStartingIndex; i - e.OldStartingIndex < e.OldItems.Count; i++)
					{
						Element element = InternalChildren[i];
						InternalChildren.RemoveAt(i);
						element.Owned = false;

						T page = _templatedItems.GetOrCreateContent(i, e.NewItems[i - e.OldStartingIndex]);
						page.Owned = true;
						SetIndex(page, i);
						InternalChildren.Insert(i, page);
					}

					break;

				case NotifyCollectionChangedAction.Reset:
					Reset();
					return;
			}

			OnPagesChanged(e);
			UpdateCurrentPage();
		}

		void Reset()
		{
			List<Element> snapshot = InternalChildren.ToList();

			InternalChildren.Clear();

			foreach (Element element in snapshot)
				element.Owned = false;

			for (var i = 0; i < _templatedItems.Count; i++)
			{
				T page = _templatedItems.GetOrCreateContent(i, _templatedItems.ListProxy[i]);
				page.Owned = true;
				SetIndex(page, i);
				InternalChildren.Add(page);
			}

			var currentNeedsUpdate = true;

			BatchBegin();

			if (ItemsSource != null)
			{
				object selected = SelectedItem;
				if (selected == null || !ItemsSource.Cast<object>().Contains(selected))
				{
					SelectedItem = ItemsSource.Cast<object>().FirstOrDefault();
					currentNeedsUpdate = false;
				}
			}

			if (currentNeedsUpdate)
				UpdateCurrentPage();

			OnPagesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

			BatchCommit();
		}

		void UpdateCurrentPage()
		{
			if (ItemsSource != null)
			{
				int index = _templatedItems.ListProxy.IndexOf(SelectedItem);
				if (index == -1)
					CurrentPage = (T)InternalChildren.FirstOrDefault();
				else
					CurrentPage = _templatedItems.GetOrCreateContent(index, SelectedItem);
			}
			else if (SelectedItem is T)
				CurrentPage = (T)SelectedItem;
		}
	}
}