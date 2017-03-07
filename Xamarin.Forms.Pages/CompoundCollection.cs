using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Pages
{
	public class CompoundCollection : Element, IList, INotifyCollectionChanged
	{
		public static readonly BindableProperty MainListProperty = BindableProperty.Create(nameof(MainList), typeof(IReadOnlyList<object>), typeof(CompoundCollection), default(IReadOnlyList<object>),
			propertyChanged: OnMainListPropertyChanged);

		readonly ObservableCollection<object> _appendList = new ObservableCollection<object>();

		readonly ObservableCollection<object> _prependList = new ObservableCollection<object>();

		public CompoundCollection()
		{
			_prependList.CollectionChanged += OnPrependCollectionChanged;
			_appendList.CollectionChanged += OnAppendCollectionChanged;
		}

		public IList AppendList => _appendList;

		public IReadOnlyList<object> MainList
		{
			get { return (IReadOnlyList<object>)GetValue(MainListProperty); }
			set { SetValue(MainListProperty, value); }
		}

		public IList PrependList => _prependList;

		public void CopyTo(Array array, int index)
		{
			throw new NotSupportedException();
		}

		public int Count => AppendList.Count + PrependList.Count + (MainList?.Count ?? 0);

		public bool IsSynchronized => false;

		public object SyncRoot => null;

		public IEnumerator GetEnumerator()
		{
			foreach (object item in PrependList)
				yield return item;
			foreach (object item in MainList)
				yield return item;
			foreach (object item in AppendList)
				yield return item;
		}

		public int Add(object value)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(object value)
		{
			IReadOnlyList<object> mainList = MainList;
			bool masterContains;
			var masterList = mainList as IList;
			if (masterList != null)
			{
				masterContains = masterList.Contains(value);
			}
			else
			{
				masterContains = mainList.Contains(value);
			}
			return masterContains || PrependList.Contains(value) || AppendList.Contains(value);
		}

		public int IndexOf(object value)
		{
			int result;
			result = PrependList.IndexOf(value);
			if (result >= 0)
				return result;
			result = MainList.IndexOf(value);
			if (result >= 0)
				return result + PrependList.Count;

			result = AppendList.IndexOf(value);
			if (result >= 0)
				return result + PrependList.Count + MainList.Count;
			return -1;
		}

		public void Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		public bool IsFixedSize => false;

		public bool IsReadOnly => true;

		public object this[int index]
		{
			get
			{
				IReadOnlyList<object> mainList = MainList;
				int prependSize = PrependList.Count;
				if (index < prependSize)
					return PrependList[index];
				index -= prependSize;

				if (mainList != null)
				{
					if (index < mainList.Count)
						return mainList[index];
					index -= mainList.Count;
				}

				if (index >= AppendList.Count)
					throw new IndexOutOfRangeException();
				return AppendList[index];
			}
			set { throw new NotSupportedException(); }
		}

		public void Remove(object value)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		void OnAppendCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			int offset = _prependList.Count + (MainList?.Count ?? 0);
			// here we just need to calculate the offset for the index, everything else is the same
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, args.NewItems, offset + args.NewStartingIndex));
					break;
				case NotifyCollectionChangedAction.Move:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, args.OldItems, offset + args.NewStartingIndex, offset + args.OldStartingIndex));
					break;
				case NotifyCollectionChangedAction.Remove:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, args.OldItems, offset + args.OldStartingIndex));
					break;
				case NotifyCollectionChangedAction.Replace:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, args.NewItems, args.OldItems, offset + args.OldStartingIndex));
					break;
				case NotifyCollectionChangedAction.Reset:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			CollectionChanged?.Invoke(this, args);
		}

		void OnMainCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			// much complexity to be had here
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, args.NewItems, PublicIndexFromMainIndex(args.NewStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Move:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, args.OldItems, PublicIndexFromMainIndex(args.NewStartingIndex),
						PublicIndexFromMainIndex(args.OldStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Remove:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, args.OldItems, PublicIndexFromMainIndex(args.OldStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Replace:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, args.NewItems, args.OldItems, PublicIndexFromMainIndex(args.OldStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Reset:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		static void OnMainListPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (CompoundCollection)bindable;
			var observable = oldValue as INotifyCollectionChanged;
			if (observable != null)
				observable.CollectionChanged -= self.OnMainCollectionChanged;
			observable = newValue as INotifyCollectionChanged;
			if (observable != null)
				observable.CollectionChanged += self.OnMainCollectionChanged;
			self.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		void OnPrependCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			// this can basically be a passthrough as prepend has no masking and identical indexing
			OnCollectionChanged(args);
		}

		int PublicIndexFromMainIndex(int index)
		{
			return PrependList.Count + index;
		}
	}
}