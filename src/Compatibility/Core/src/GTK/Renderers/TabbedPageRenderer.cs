using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Gtk;
using Microsoft.Maui.Controls.Compatibility.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Extensions;
using Microsoft.Maui.Controls.Compatibility.PlatformConfiguration.GTKSpecific;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Renderers
{
	public class TabbedPageRenderer : AbstractPageRenderer<NotebookWrapper, TabbedPage>
	{
		const int DefaultIconWidth = 24;
		const int DefaultIconHeight = 24;

		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				Page.ChildAdded -= OnPageAdded;
				Page.ChildRemoved -= OnPageRemoved;
				Page.PagesChanged -= OnPagesChanged;
			}

			if (e.NewElement != null)
			{
				var newPage = e.NewElement as TabbedPage;

				if (newPage == null)
					throw new ArgumentException("Element must be a TabbedPage");

				if (Widget == null)
				{
					// Custom control using a tabbed notebook container.
					Widget = new NotebookWrapper();
					Control.Content = Widget;
				}

				Init();
			}
		}

		void Init()
		{
			OnPagesChanged(Page.Children,
				  new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

			Page.ChildAdded += OnPageAdded;
			Page.ChildRemoved += OnPageRemoved;
			Page.PagesChanged += OnPagesChanged;

			UpdateCurrentPage();
			UpdateBarBackgroundColor();
			UpdateBarTextColor();
			UpdateTabPos();
			UpdateBackgroundImage();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == nameof(TabbedPage.CurrentPage))
			{
				UpdateCurrentPage();
				UpdateBarTextColor();
				UpdateBarBackgroundColor();
			}
			else if (e.PropertyName == TabbedPage.BarTextColorProperty.PropertyName)
				UpdateBarTextColor();
			else if (e.PropertyName == TabbedPage.BarBackgroundColorProperty.PropertyName)
				UpdateBarBackgroundColor();
			else if (e.PropertyName ==
				  PlatformConfiguration.GTKSpecific.TabbedPage.TabPositionProperty.PropertyName)
				UpdateTabPos();
		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);

			Control?.Content?.SetSize(allocation.Width, allocation.Height);
		}

		protected override void UpdateBackgroundImage()
		{
			Widget?.SetBackgroundImage(Page.BackgroundImageSource);
		}

		protected override void Dispose(bool disposing)
		{
			Page.PagesChanged -= OnPagesChanged;
			Page.ChildAdded -= OnPageAdded;
			Page.ChildRemoved -= OnPageRemoved;

			if (Widget != null)
			{
				Widget.NoteBook.SwitchPage -= OnNotebookPageSwitched;
			}

			base.Dispose(disposing);
		}

		void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Widget.NoteBook.SwitchPage -= OnNotebookPageSwitched;

			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				ResetPages();
			}

			UpdateChildrenOrderIndex();
			UpdateCurrentPage();

			Widget.NoteBook.SwitchPage += OnNotebookPageSwitched;
		}

		void OnPageAdded(object sender, ElementEventArgs e)
		{
			InsertPage(e.Element as Page, Page.Children.IndexOf(e.Element));
		}

		void OnPageRemoved(object sender, ElementEventArgs e)
		{
			RemovePage(e.Element as Page);
		}

		void InsertPage(Page page, int index)
		{
			var pageRenderer = Platform.GetRenderer(page);

			if (pageRenderer == null)
			{
				pageRenderer = Platform.CreateRenderer(page);
				Platform.SetRenderer(page, pageRenderer);
			}

			Widget.InsertPage(
				pageRenderer.Container,
				page.Title,
				page.IconImageSource?.ToPixbuf(new Size(DefaultIconWidth, DefaultIconHeight)),
				index);

			Widget.ShowAll();

			page.PropertyChanged += OnPagePropertyChanged;
		}

		void RemovePage(Page page)
		{
			page.PropertyChanged -= OnPagePropertyChanged;

			var pageRenderer = Platform.GetRenderer(page);

			if (pageRenderer != null)
			{
				Widget.RemovePage(pageRenderer.Container);
			}

			Platform.SetRenderer(page, null);
		}

		void ResetPages()
		{
			foreach (var page in Page.Children)
				RemovePage(page);

			Widget.RemoveAllPages();

			var i = 0;
			foreach (var page in Page.Children)
				InsertPage(page, i++);
		}

		void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Microsoft.Maui.Controls.Compatibility.Page.TitleProperty.PropertyName)
			{
				var page = (Page)sender;
				var index = TabbedPage.GetIndex(page);

				Widget.SetTabLabelText(index, page.Title);
			}
			else if (e.PropertyName == Microsoft.Maui.Controls.Compatibility.Page.IconImageSourceProperty.PropertyName)
			{
				var page = (Page)sender;
				var index = TabbedPage.GetIndex(page);
				var icon = page.IconImageSource;

				Widget.SetTabIcon(index, icon.ToPixbuf());
			}
			else if (e.PropertyName == TabbedPage.BarBackgroundColorProperty.PropertyName)
				UpdateBarBackgroundColor();
			else if (e.PropertyName == TabbedPage.BarTextColorProperty.PropertyName)
				UpdateBarTextColor();
		}

		void UpdateCurrentPage()
		{
			Page page = Page.CurrentPage;

			if (page == null)
				return;

			int selectedIndex = 0;

			for (var i = 0; i < Page.Children.Count; i++)
			{
				if (Page.Children[i].Equals(page))
				{
					break;
				}

				selectedIndex++;
			}

			Widget.NoteBook.CurrentPage = selectedIndex;
			Widget.NoteBook.ShowAll();
		}

		void UpdateChildrenOrderIndex()
		{
			for (var i = 0; i < Page.Children.Count; i++)
			{
				var page = PageController.InternalChildren[i];

				TabbedPage.SetIndex(page as Page, i);
			}
		}

		void UpdateBarBackgroundColor()
		{
			if (Element == null || Page.BarBackgroundColor.IsDefault)
				return;

			var barBackgroundColor = Page.BarBackgroundColor.ToGtkColor();

			for (var i = 0; i < Page.Children.Count; i++)
			{
				Widget.SetTabBackgroundColor(i, barBackgroundColor);
			}
		}

		void UpdateBarTextColor()
		{
			if (Element == null || Page.BarTextColor.IsDefault)
				return;

			var barTextColor = Page.BarTextColor.ToGtkColor();

			for (var i = 0; i < Page.Children.Count; i++)
			{
				Widget.SetTabTextColor(i, barTextColor);
			}
		}

		void UpdateTabPos() // Platform-Specific Functionality
		{
			var tabposition = Page.OnThisPlatform().GetTabPosition();

			switch (tabposition)
			{
				case TabPosition.Top:
					Widget.NoteBook.TabPos = PositionType.Top;
					break;
				case TabPosition.Bottom:
					Widget.NoteBook.TabPos = PositionType.Bottom;
					break;
			}
		}

		void OnNotebookPageSwitched(object o, SwitchPageArgs args)
		{
			var currentPageIndex = (int)args.PageNum;
			VisualElement currentSelectedChild = Page.Children.Count > currentPageIndex
				? Page.Children[currentPageIndex]
				: null;

			if (currentSelectedChild != null)
			{
				ElementController.SetValueFromRenderer(TabbedPage.SelectedItemProperty, currentSelectedChild.BindingContext);

				var pageRenderer = Platform.GetRenderer(currentSelectedChild);
				pageRenderer?.Container.ShowAll();
			}
		}
	}
}
