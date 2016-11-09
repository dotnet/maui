using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_PickerRenderer))]
	public class Picker : View, IElementConfiguration<Picker>
	{
		public static readonly BindableProperty TextColorProperty =
			BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(Picker), Color.Default);

		public static readonly BindableProperty TitleProperty =
			BindableProperty.Create(nameof(Title), typeof(string), typeof(Picker), default(string));

		public static readonly BindableProperty SelectedIndexProperty =
			BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(Picker), -1, BindingMode.TwoWay,
									propertyChanged: OnSelectedIndexChanged, coerceValue: CoerceSelectedIndex);

		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(Picker), default(IList),
									propertyChanged: OnItemsSourceChanged);

		public static readonly BindableProperty SelectedItemProperty =
			BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(Picker), null, BindingMode.TwoWay,
									propertyChanged: OnSelectedItemChanged);

		readonly Lazy<PlatformConfigurationRegistry<Picker>> _platformConfigurationRegistry;

		public Picker()
		{
			((INotifyCollectionChanged)Items).CollectionChanged += OnItemsCollectionChanged;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Picker>>(() => new PlatformConfigurationRegistry<Picker>(this));
		}

		public IList<string> Items { get; } = new LockableObservableListWrapper();

		public IList ItemsSource
		{
			get { return (IList)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public int SelectedIndex
		{
			get { return (int)GetValue(SelectedIndexProperty); }
			set { SetValue(SelectedIndexProperty, value); }
		}

		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public Color TextColor {
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}

		public string Title {
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		BindingBase _itemDisplayBinding;
		public BindingBase ItemDisplayBinding {
			get { return _itemDisplayBinding; }
			set {
				if (_itemDisplayBinding == value)
					return;

				OnPropertyChanging();
				var oldValue = value;
				_itemDisplayBinding = value;
				OnItemDisplayBindingChanged(oldValue, _itemDisplayBinding);
				OnPropertyChanged();
			}
		}

		public event EventHandler SelectedIndexChanged;

		static readonly BindableProperty s_displayProperty =
			BindableProperty.Create("Display", typeof(string), typeof(Picker), default(string));

		string GetDisplayMember(object item)
		{
			if (ItemDisplayBinding == null)
				return item.ToString();

			ItemDisplayBinding.Apply(item, this, s_displayProperty);
			ItemDisplayBinding.Unapply();
			return (string)GetValue(s_displayProperty);
		}

		static object CoerceSelectedIndex(BindableObject bindable, object value)
		{
			var picker = (Picker)bindable;
			return picker.Items == null ? -1 : ((int)value).Clamp(-1, picker.Items.Count - 1);
		}

		void OnItemDisplayBindingChanged(BindingBase oldValue, BindingBase newValue)
		{
			ResetItems();
		}

		void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SelectedIndex = SelectedIndex.Clamp(-1, Items.Count - 1);
			UpdateSelectedItem();
		}

		static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((Picker)bindable).OnItemsSourceChanged((IList)oldValue, (IList)newValue);
		}

		void OnItemsSourceChanged(IList oldValue, IList newValue)
		{ 
			var oldObservable = oldValue as INotifyCollectionChanged;
			if (oldObservable != null)
				oldObservable.CollectionChanged -= CollectionChanged;

			var newObservable = newValue as INotifyCollectionChanged;
			if (newObservable != null) {
				newObservable.CollectionChanged += CollectionChanged;
			}

			if (newValue != null) {
				((LockableObservableListWrapper)Items).IsLocked = true;
				ResetItems();
			} else {
				((LockableObservableListWrapper)Items).InternalClear();
				((LockableObservableListWrapper)Items).IsLocked = false;
			}
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				AddItems(e);
				break;
			case NotifyCollectionChangedAction.Remove:
				RemoveItems(e);
				break;
			default: //Move, Replace, Reset
				ResetItems();
				break;
			}
		}
		void AddItems(NotifyCollectionChangedEventArgs e)
		{
			int index = e.NewStartingIndex < 0 ? Items.Count : e.NewStartingIndex;
			foreach (object newItem in e.NewItems)
				((LockableObservableListWrapper)Items).InternalInsert(index++, GetDisplayMember(newItem));
		}

		void RemoveItems(NotifyCollectionChangedEventArgs e)
		{
			int index = e.OldStartingIndex < Items.Count ? e.OldStartingIndex : Items.Count;
			foreach (object _ in e.OldItems)
				((LockableObservableListWrapper)Items).InternalRemoveAt(index--);
		}

		void ResetItems()
		{
			if (ItemsSource == null)
				return;
			((LockableObservableListWrapper)Items).InternalClear();
			foreach (object item in ItemsSource)
				((LockableObservableListWrapper)Items).InternalAdd(GetDisplayMember(item));
			UpdateSelectedItem();
		}

		static void OnSelectedIndexChanged(object bindable, object oldValue, object newValue)
		{
			var picker = (Picker)bindable;
			picker.SelectedIndexChanged?.Invoke(bindable, EventArgs.Empty);
			picker.UpdateSelectedItem();
		}

		static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var picker = (Picker)bindable;
			picker.UpdateSelectedIndex(newValue);
		}

		void UpdateSelectedIndex(object selectedItem)
		{
			if (ItemsSource != null) {
				SelectedIndex = ItemsSource.IndexOf(selectedItem);
				return;
			}
			SelectedIndex = Items.IndexOf(selectedItem);
		}

		void UpdateSelectedItem()
		{
			if (SelectedIndex == -1) {
				SelectedItem = null;
				return;
			}

			if (ItemsSource != null) {
				SelectedItem = ItemsSource [SelectedIndex];
				return;
			}

			SelectedItem = Items [SelectedIndex];
		}

		public IPlatformElementConfiguration<T, Picker> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		class LockableObservableListWrapper : INotifyCollectionChanged, IList<string>
		{
			readonly ObservableList<string> _list = new ObservableList<string>();

			public bool IsLocked { get; set; }

			event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged {
				add { _list.CollectionChanged += value; }
				remove { _list.CollectionChanged -= value; }
			}

			void ThrowOnLocked()
			{
				if (IsLocked)
					throw new InvalidOperationException("The Items list can not be manipulated if the ItemsSource property is set");
			
			}
			public string this [int index] {
				get { return _list [index]; }
				set {
					ThrowOnLocked();
					_list [index] = value; }
			}

			public int Count {
				get { return _list.Count; }
			}

			public bool IsReadOnly {
				get { return ((IList<string>)_list).IsReadOnly; }
			}

			public void InternalAdd(string item)
			{
				_list.Add(item);
			}

			public void Add(string item)
			{
				ThrowOnLocked();
				InternalAdd(item);
			}

			public void InternalClear()
			{ 
				_list.Clear();
			}

			public void Clear()
			{
				ThrowOnLocked();
				InternalClear();
			}

			public bool Contains(string item)
			{
				return _list.Contains(item);
			}

			public void CopyTo(string [] array, int arrayIndex)
			{
				_list.CopyTo(array, arrayIndex);
			}

			public IEnumerator<string> GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			public int IndexOf(string item)
			{
				return _list.IndexOf(item);
			}

			public void InternalInsert(int index, string item)
			{
				_list.Insert(index, item);
			}

			public void Insert(int index, string item)
			{
				ThrowOnLocked();
				InternalInsert(index, item);
			}

			public bool InternalRemove(string item)
			{
				return _list.Remove(item);
			}

			public bool Remove(string item)
			{
				ThrowOnLocked();
				return InternalRemove(item);
			}

			public void InternalRemoveAt(int index)
			{
				_list.RemoveAt(index);
			}

			public void RemoveAt(int index)
			{
				ThrowOnLocked();
				InternalRemoveAt(index);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable)_list).GetEnumerator();
			}
		}
	}
}