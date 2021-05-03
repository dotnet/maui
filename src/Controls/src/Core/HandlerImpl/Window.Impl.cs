#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls
{
	public class Window : VisualElement, IWindow
	{
		ReadOnlyCollection<Element>? _logicalChildren;
		Page? _page;

		ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCollection<Element>(InternalChildren);

		public Window()
		{
			InternalChildren.CollectionChanged += OnCollectionChanged;
		}

		public Window(Page page)
			: this()
		{
			Page = page;
		}


		void SendWindowAppearing()
		{
			Page?.SendAppearing();
		}

		void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				for (var i = 0; i < e.OldItems.Count; i++)
				{
					var item = (Element?)e.OldItems[i];
					OnChildRemoved(item, e.OldStartingIndex + i);
				}
			}

			if (e.NewItems != null)
			{
				foreach (Element item in e.NewItems)
				{
					OnChildAdded(item);

					// TODO once we have better life cycle events on pages 
					if (item is Page)
					{
						SendWindowAppearing();
					}
				}
			}
		}

		public Page? Page
		{
			get => _page;
			set
			{
				if (_page != null)
					InternalChildren.Remove(_page);

				_page = value;

				if (_page != null)
					InternalChildren.Add(_page);

				if (value is NavigableElement ne)
					ne.NavigationProxy.Inner = NavigationProxy;
			}
		}

		IView IWindow.View
		{
			get => Page ?? throw new InvalidOperationException("No page was set on the window.");
			set => Page = (Page)value;
		}
	}
}
