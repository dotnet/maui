using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	[ContentProperty("Page")]
	public partial class Window : VisualElement
	{
		ContentPage _page;
		ReadOnlyCollection<Element> _logicalChildren;
		ObservableCollection<Element> _internalChildren { get; } = new ObservableCollection<Element>();


		public static readonly BindableProperty PageProperty = BindableProperty.Create(nameof(Page), typeof(IPage), typeof(Window), null);

		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(Window), null);


		internal override ReadOnlyCollection<Element> LogicalChildrenInternal
		{
			get { return _logicalChildren ?? (_logicalChildren = new ReadOnlyCollection<Element>(_internalChildren)); }
		}

		public ContentPage Page
		{
			get { return _page; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				if (_page == value)
					return;

				OnPropertyChanging();

				if (_page != null)
					RemovePage(_page);
	
				_page = value;

				if (_page != null)
					AddPage(_page);
				
				OnPropertyChanged();
			}
		}

		void AddPage(Page page)
		{
			_internalChildren.Add(page);
			OnChildAdded(page);
		}
		void RemovePage(Page page)
		{
			_internalChildren.Remove(page);
			OnChildRemoved(page, 0);
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}
	}
}
