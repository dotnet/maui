using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;
using NView = Gtk.Widget;

namespace Gtk.UIExtensions.NUI
{
    /// <summary>
    /// Base class for an Adapter
    /// Adapters provide a binding from an app-specific data set to views that are displayed within a CollectionView.
    /// </summary>
    public abstract class ItemAdaptor : INotifyCollectionChanged, IDisposable
    {
        bool disposedValue;
        IList _itemsSource;

        /// <summary>
        ///  Initializes a new instance of the <see cref="ItemAdaptor"/> class.
        /// </summary>
        /// <param name="items">Items soruce</param>
#pragma warning disable CS8618
        // dotnet compiler does not track a method that called on constructor to check non-nullable object
        // https://github.com/dotnet/roslyn/issues/32358
        protected ItemAdaptor(IEnumerable items)
#pragma warning restore CS8618
        {
            SetItemsSource(items);
        }

        /// <summary>
        /// A CollectionView associated with current Adaptor
        /// </summary>
        public ICollectionViewController? CollectionView { get; set; }

        /// <summary>
        /// Sets ItemsSource
        /// </summary>
        /// <param name="items">Items source</param>
        protected void SetItemsSource(IEnumerable items)
        {
            switch (items)
            {
                case IList list:
                    _itemsSource = list;
                    if (list is INotifyCollectionChanged observable)
                    {
                        _observableCollection = observable;
                        _observableCollection.CollectionChanged += OnCollectionChanged;
                    }
                    break;
                case IEnumerable<object> generic:
                    _itemsSource = new List<object>(generic);
                    break;
                case IEnumerable _:
                    _itemsSource = new List<object>();
                    foreach (var item in items)
                    {
                        _itemsSource.Add(item);
                    }
                    break;
            }
        }

        public object? this[int index]
        {
            get
            {
                return _itemsSource[index];
            }
        }

        /// <summary>
        /// the number of items
        /// </summary>
        public virtual int Count => _itemsSource.Count;

        INotifyCollectionChanged? _observableCollection;

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// Handle Selected item
        /// </summary>
        /// <param name="selected"></param>
        public virtual void SendItemSelected(IEnumerable<int> selected)
        {
        }

        /// <summary>
        /// Update View state
        /// </summary>
        /// <param name="view">A view to update</param>
        /// <param name="state">State of view</param>
        public virtual void UpdateViewState(NView view, ViewHolderState state)
        {
        }

        /// <summary>
        /// Find item index by item
        /// </summary>
        /// <param name="item">item to find</param>
        /// <returns>index of item</returns>
        public int GetItemIndex(object item)
        {
            return _itemsSource.IndexOf(item);
        }

        /// <summary>
        /// A view category that represent a item, it use to distinguish kinds of view
        /// </summary>
        /// <param name="index">item index</param>
        /// <returns>An identifier of category</returns>
        public virtual object GetViewCategory(int index)
        {
            return this;
        }

        /// <summary>
        /// Create a new view
        /// </summary>
        /// <returns>Created view</returns>
        public abstract NView CreateNativeView();

        /// <summary>
        /// Create a new view
        /// </summary>
        /// <param name="index">To used item when create a view</param>
        /// <returns>Created view</returns>
        public abstract NView CreateNativeView(int index);

        /// <summary>
        /// Create a header view, if header is not existed, null will be returned
        /// </summary>
        /// <returns>A created view</returns>
        public abstract NView? GetHeaderView();

        public abstract IView? GetTemplatedView(int index);
        
        public abstract IView? GetTemplatedView(NView view);
        
        /// <summary>
        /// Remove header view, a created view by Adaptor, should be removed by Adaptor
        /// </summary>
        /// <param name="header">A view to remove</param>
        public virtual void RemoveHeaderView(NView header)
        {
            header.Dispose();
        }

        /// <summary>
        /// Create a footer view, if footer is not existed, null will be returned
        /// </summary>
        /// <returns>A created view</returns>
        public abstract NView? GetFooterView();

        /// <summary>
        /// Remove footer view, a created view by Adaptor, should be removed by Adaptor
        /// </summary>
        /// <param name="footer">A view to remove</param>
        public virtual void RemoveFooterView(NView footer)
        {
            footer.Dispose();
        }

        /// <summary>
        /// Remove view, a created view by Adaptor, should be removed by Adaptor
        /// </summary>
        /// <param name="native">A view to remove</param>
        public abstract void RemoveNativeView(NView native);

        /// <summary>
        /// Set data binding between view and item
        /// </summary>
        /// <param name="view">A target view</param>
        /// <param name="index">A target item</param>
        public abstract void SetBinding(NView view, int index);

        /// <summary>
        /// Unset data binding on view
        /// </summary>
        /// <param name="view">A view to unbinding</param>
        public abstract void UnBinding(NView view);

        /// <summary>
        /// Measure item size
        /// </summary>
        /// <param name="widthConstraint">A width size that could be reached as maximum</param>
        /// <param name="heightConstraint">A height  size that could be reached as maximum</param>
        /// <returns>Item size</returns>
        public abstract Size MeasureItem(double widthConstraint, double heightConstraint);

        /// <summary>
        /// Measure item size
        /// </summary>
        /// <param name="index">A item index to measure</param>
        /// <param name="widthConstraint">A width size that could be reached as maximum</param>
        /// <param name="heightConstraint">A height  size that could be reached as maximum</param>
        /// <returns>Item size</returns>
        public abstract Size MeasureItem(int index, double widthConstraint, double heightConstraint);

        /// <summary>
        /// Measure header size
        /// </summary>
        /// <param name="widthConstraint">A width size that could be reached as maximum</param>
        /// <param name="heightConstraint">A height  size that could be reached as maximum</param>
        /// <returns>Header size</returns>
        public abstract Size MeasureHeader(double widthConstraint, double heightConstraint);

        /// <summary>
        /// Measure Footer size
        /// </summary>
        /// <param name="widthConstraint">A width size that could be reached as maximum</param>
        /// <param name="heightConstraint">A height  size that could be reached as maximum</param>
        /// <returns>Footer size</returns>
        public abstract Size MeasureFooter(double widthConstraint, double heightConstraint);

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_observableCollection != null)
                    {
                        _observableCollection.CollectionChanged -= OnCollectionChanged;
                    }
                }

                disposedValue = true;
            }
        }

        void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}
