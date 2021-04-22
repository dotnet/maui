using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Maui;

namespace Microsoft.Maui.Controls
{
	public class Window : VisualElement, IWindow
	{
		ReadOnlyCollection<Element> _logicalChildren;
		ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();
		internal override ReadOnlyCollection<Element> LogicalChildrenInternal =>
			_logicalChildren ?? (_logicalChildren = new ReadOnlyCollection<Element>(InternalChildren));

		Page _page;

		public Window(Page page)
		{
			InternalChildren.CollectionChanged += OnCollectionChanged;
			Page = page;
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				for (var i = 0; i < e.OldItems.Count; i++)
				{
					var item = (Element)e.OldItems[i];
					OnChildRemoved(item, e.OldStartingIndex + i);
				}
			}

			if (e.NewItems != null)
			{
				foreach (Element item in e.NewItems)
				{
					OnChildAdded(item);
				}
			}
		}

		public Page Page
		{
			get => (Page)(this as IWindow).View;
			set => (this as IWindow).View = (IView)value;
		}

		IView IWindow.View
		{
			get => _page;
			set
			{
				var page = (Page)value;

				if (Page != null)
					InternalChildren.Remove(Page);

				_page = value;

				if (value != null)
					InternalChildren.Add(page);

				if (value is NavigableElement ne)
					ne.NavigationProxy.Inner = NavigationProxy;

				_page = value;
			}
		}
	}
}