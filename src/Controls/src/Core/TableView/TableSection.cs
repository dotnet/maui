#nullable disable
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Generic base class for table sections that contain a collection of items of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of items contained in the section.</typeparam>
	public abstract class TableSectionBase<T> : TableSectionBase, IList<T>, IVisualTreeElement, INotifyCollectionChanged where T : BindableObject
	{
		readonly ObservableCollection<T> _children = new ObservableCollection<T>();

		/// <summary>
		///     Constructs a Section without an empty header.
		/// </summary>
		protected TableSectionBase()
		{
			_children.CollectionChanged += OnChildrenChanged;
		}

		/// <summary>
		///     Constructs a Section with the specified header.
		/// </summary>
		protected TableSectionBase(string title) : base(title)
		{
			_children.CollectionChanged += OnChildrenChanged;
		}

		public void Add(T item)
		{
			_children.Add(item);
			if (item is IVisualTreeElement element)
			{
				VisualDiagnostics.OnChildAdded(this, element);
			}
		}

		public void Clear()
		{
			foreach (T item in _children)
			{
				if (item is IVisualTreeElement element)
				{
					VisualDiagnostics.OnChildRemoved(this, element, _children.IndexOf(item));
				}
			}

			_children.Clear();
		}

		public bool Contains(T item)
		{
			return _children.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_children.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _children.Count; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(T item)
		{
			if (item is IVisualTreeElement element)
			{
				VisualDiagnostics.OnChildRemoved(this, element, _children.IndexOf(item));
			}

			return _children.Remove(item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _children.GetEnumerator();
		}

		public int IndexOf(T item)
		{
			return _children.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			if (item is IVisualTreeElement element)
			{
				VisualDiagnostics.OnChildAdded(this, element, index);
			}

			_children.Insert(index, item);
		}

		public T this[int index]
		{
			get { return _children[index]; }
			set { _children[index] = value; }
		}

		public void RemoveAt(int index)
		{
			T item = _children[index];
			if (item is IVisualTreeElement element)
			{
				VisualDiagnostics.OnChildRemoved(this, element, index);
			}

			_children.RemoveAt(index);
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add { _children.CollectionChanged += value; }
			remove { _children.CollectionChanged -= value; }
		}

		public void Add(IEnumerable<T> items)
		{
			items.ForEach(_children.Add);
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			object bc = BindingContext;
			foreach (T child in _children)
				SetInheritedBindingContext(child, bc);
		}

		void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			// We need to hook up the binding context for new items.
			if (notifyCollectionChangedEventArgs.NewItems == null)
				return;
			object bc = BindingContext;
			foreach (BindableObject item in notifyCollectionChangedEventArgs.NewItems)
			{
				SetInheritedBindingContext(item, bc);
			}
		}

		IReadOnlyList<Maui.IVisualTreeElement> IVisualTreeElement.GetVisualChildren() => this._children.Cast<IVisualTreeElement>().ToList().AsReadOnly();

		IVisualTreeElement IVisualTreeElement.GetVisualParent() => null;
	}

	/// <summary>
	/// A logical grouping of cells in a <see cref="TableView"/>.
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	public sealed class TableSection : TableSectionBase<Cell>
#pragma warning restore CS0618 // Type or member is obsolete
	{
		/// <summary>
		/// Creates a new <see cref="TableSection"/> with default values.
		/// </summary>
		public TableSection()
		{
		}

		/// <summary>
		/// Creates a new <see cref="TableSection"/> with the specified title.
		/// </summary>
		public TableSection(string title) : base(title)
		{
		}
	}
}
