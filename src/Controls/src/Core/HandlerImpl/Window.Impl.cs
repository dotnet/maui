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

		public Window(IPage page)
		{
			InternalChildren.CollectionChanged += OnCollectionChanged;
			Page = (Page)page;
		}

		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
			get => _page;
			set
			{
				if (_page != null)
					InternalChildren.Remove(_page);

				_page = value;

				if (value != null)
					InternalChildren.Add(_page);
			}
		}

		IView IWindow.View
		{
			get => (IView)Page;
			set => Page = (Page)value;
		}
	}
}