using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ElmSharp;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using EColor = ElmSharp.Color;
using EToolbarItem = ElmSharp.ToolbarItem;
using EToolbarItemEventArgs = ElmSharp.ToolbarItemEventArgs;

namespace Xamarin.Forms.Platform.Tizen
{
	public class TabbedPageRenderer : VisualElementRenderer<TabbedPage>
	{
		Box _outterLayout;
		Box _innerBox;
		Scroller _scroller;
		Toolbar _toolbar;
		Dictionary<EToolbarItem, Page> _itemToItemPage = new Dictionary<EToolbarItem, Page>();
		List<EToolbarItem> _toolbarItemList = new List<EToolbarItem>();
		bool _isResettingToolbarItems = false;
		bool _isInitialized = false;
		bool _isUpdateByToolbar = false;
		bool _isUpdateByScroller = false;
		bool _isUpdateByCurrentPage = false;

		public TabbedPageRenderer()
		{
			RegisterPropertyHandler(Page.TitleProperty, UpdateTitle);
			RegisterPropertyHandler(nameof(Element.CurrentPage), OnCurrentPageChanged);
			RegisterPropertyHandler(TabbedPage.BarBackgroundColorProperty, UpdateBarBackgroundColor);
			RegisterPropertyHandler(TabbedPage.BarTextColorProperty, UpdateBarTextColor);
			RegisterPropertyHandler(TabbedPage.SelectedTabColorProperty, UpdateSelectedTabColor);
			RegisterPropertyHandler(TabbedPage.UnselectedTabColorProperty, UpdateUnselectedTabColor);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
		{
			if (_toolbar == null)
			{
				//Create box that holds toolbar and selected content
				_outterLayout = new Box(Forms.NativeParent)
				{
					AlignmentX = -1,
					AlignmentY = -1,
					WeightX = 1,
					WeightY = 1,
					IsHorizontal = false,
				};
				_outterLayout.Show();

				//Create toolbar that is placed inside the _outterLayout
				_toolbar = new Toolbar(Forms.NativeParent)
				{
					AlignmentX = -1,
					WeightX = 1,
					SelectionMode = ToolbarSelectionMode.Always,
				};

				if (Device.Idiom == TargetIdiom.Phone)
				{
					//Set ShrinkMode to Expand as defauly only for Mobile profile
					_toolbar.ShrinkMode = ToolbarShrinkMode.Expand;
				}
				else if (Device.Idiom == TargetIdiom.TV)
				{
					//According to TV UX Guideline, toolbar style should be set to "tabbar_with_title" in case of TabbedPage only for TV profile.
					_toolbar.Style = "tabbar_with_title";
				}

				_toolbar.Show();
				//Add callback for Toolbar item selection
				_toolbar.Selected += OnToolbarItemSelected;
				_outterLayout.PackEnd(_toolbar);

				_scroller = new Scroller(_outterLayout)
				{
					AlignmentX = -1,
					AlignmentY = -1,
					WeightX = 1,
					WeightY = 1,
					HorizontalPageScrollLimit = 1,
					ScrollBlock = ScrollBlock.Vertical,
					HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible
				};
				_scroller.SetPageSize(1.0, 1.0);
				_scroller.PageScrolled += OnItemPageScrolled;

				_innerBox = new Box(Forms.NativeParent)
				{
					AlignmentX = -1,
					AlignmentY = -1,
					WeightX = 1,
					WeightY = 1,
					IsHorizontal = true,
				};

				_innerBox.SetLayoutCallback(OnInnerLayoutUpdate);

				_scroller.SetContent(_innerBox);
				_scroller.Show();

				_outterLayout.PackEnd(_scroller);

				SetNativeView(_outterLayout);
				UpdateTitle();
			}

			if (e.OldElement != null)
			{
				e.OldElement.PagesChanged -= OnElementPagesChanged;
				_isInitialized = false;
			}
			if (e.NewElement != null)
			{
				e.NewElement.PagesChanged += OnElementPagesChanged;
			}

			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Element.PagesChanged -= OnElementPagesChanged;
				if (_outterLayout != null)
				{
					_outterLayout.Unrealize();
					_outterLayout = null;
				}
				if (_toolbar != null)
				{
					_toolbar.Selected -= OnToolbarItemSelected;
					_scroller.PageScrolled -= OnItemPageScrolled;
					_toolbar.Unrealize();
					_toolbar = null;
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnElementReady()
		{
			base.OnElementReady();
			_isInitialized = true;
			FillToolbarAndContents();
			Element.UpdateFocusTreePolicy();
		}

		protected override void UpdateThemeStyle()
		{
			var style = Element.OnThisPlatform().GetStyle();
			if (!string.IsNullOrEmpty(style))
			{
				_toolbar.Style = style;
				((IVisualElementController)Element).NativeSizeChanged();
			}
		}

		void OnInnerLayoutUpdate()
		{
			if (!_isInitialized)
				return;

			int baseX = _innerBox.Geometry.X;
			Rect bound = _scroller.Geometry;
			int index = 0;
			foreach (var page in Element.Children)
			{
				var nativeView = Platform.GetRenderer(page).NativeView;
				bound.X = baseX + index * bound.Width;
				nativeView.Geometry = bound;
				index++;
			}
			_innerBox.MinimumWidth = Element.Children.Count * bound.Width;
			if (_toolbar.SelectedItem == null)
				return;
			int currentPage = MultiPage<Page>.GetIndex(_itemToItemPage[_toolbar.SelectedItem]);
			_scroller.ScrollTo(currentPage, 0, false);
		}

		void OnItemPageScrolled(object sender, System.EventArgs e)
		{
			if (_isUpdateByToolbar || _isUpdateByCurrentPage)
				return;
			_isUpdateByScroller = true;

			var oldPage = Element.CurrentPage;
			var toBeSelectedItem = _toolbarItemList[_scroller.HorizontalPageIndex];
			var newPage = _itemToItemPage[toBeSelectedItem];

			if (oldPage != newPage)
			{
				oldPage?.SendDisappearing();
				newPage.SendAppearing();

				toBeSelectedItem.IsSelected = true;
				Element.CurrentPage = newPage;
				Element.UpdateFocusTreePolicy();
			}

			_isUpdateByScroller = false;
		}

		void UpdateBarBackgroundColor(bool initialize)
		{
			if (initialize && Element.BarBackgroundColor.IsDefault)
				return;

			EColor bgColor = Element.BarBackgroundColor.ToNative();
			_toolbar.BackgroundColor = bgColor;
			foreach (EToolbarItem item in _itemToItemPage.Keys)
			{
				ApplyBarItemColors(item, BarItemColorType.Background, bgColor);
			}
		}

		void UpdateBarTextColor(bool initialize)
		{
			if (initialize && Element.BarTextColor.IsDefault)
				return;

			foreach (EToolbarItem item in _itemToItemPage.Keys)
			{
				ApplyBarItemColors(item, BarItemColorType.Text, Element.BarTextColor.ToNative());
			}
		}

		void UpdateSelectedTabColor(bool initialize)
		{
			if (initialize && Element.SelectedTabColor.IsDefault)
				return;

			foreach (EToolbarItem item in _itemToItemPage.Keys)
			{
				ApplyBarItemColors(item, BarItemColorType.SelectedTab, Element.SelectedTabColor.ToNative());
			}
		}

		void UpdateUnselectedTabColor(bool initialize)
		{
			if (initialize && Element.UnselectedTabColor.IsDefault)
				return;

			foreach (EToolbarItem item in _itemToItemPage.Keys)
			{
				ApplyBarItemColors(item, BarItemColorType.UnselectedTab, Element.UnselectedTabColor.ToNative());
			}
		}

		void UpdateTitle()
		{
			_toolbar.Text = Element.Title;
		}

		void UpdateTitle(Page page)
		{
			if (_itemToItemPage.ContainsValue(page))
			{
				var pair = _itemToItemPage.FirstOrDefault(x => x.Value == page);
				pair.Key.SetPartText(null, pair.Value.Title);
			}
		}

		void OnPageTitleChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
			{
				UpdateTitle(sender as Page);
			}
		}

		void OnElementPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddToolbarItems(e);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveToolbarItems(e);
					break;
				default:
					ResetToolbarItems();
					break;
			}
			Element.UpdateFocusTreePolicy();
		}

		void AddToolbarItems(NotifyCollectionChangedEventArgs e)
		{
			int index = e.NewStartingIndex < 0 ? _toolbar.ItemsCount : e.NewStartingIndex;
			foreach (Page item in e.NewItems)
			{
				AddToolbarItem(item, index++);
			}
		}

		EToolbarItem AddToolbarItem(Page newItem, int index)
		{
			EToolbarItem toolbarItem;

			// TODO: investigate if we can use the other image source types: stream, font, uri

			var iconSource = newItem.IconImageSource as FileImageSource;
			var icon = iconSource.IsNullOrEmpty() ? null : ResourcePath.GetPath(iconSource);

			if (index == 0)
			{
				toolbarItem = _toolbar.Prepend(newItem.Title, icon);
			}
			else
			{
				toolbarItem = _toolbar.InsertAfter(_toolbarItemList[index - 1], newItem.Title, icon);
			}
			_toolbarItemList.Insert(index, toolbarItem);
			_itemToItemPage.Add(toolbarItem, newItem);

			ApplyBarItemColors(toolbarItem, BarItemColorType.Background, Element.BarBackgroundColor.ToNative());
			ApplyBarItemColors(toolbarItem, BarItemColorType.Text, Element.BarTextColor.ToNative());
			ApplyBarItemColors(toolbarItem, BarItemColorType.SelectedTab, Element.SelectedTabColor.ToNative());
			ApplyBarItemColors(toolbarItem, BarItemColorType.UnselectedTab, Element.UnselectedTabColor.ToNative());

			var childContent = Platform.GetOrCreateRenderer(newItem).NativeView;
			_innerBox.PackEnd(childContent);

			newItem.PropertyChanged += OnPageTitleChanged;

			return toolbarItem;
		}

		void RemoveToolbarItems(NotifyCollectionChangedEventArgs e)
		{
			foreach (Page item in e.OldItems)
			{
				RemoveToolbarItem(item);
			}
		}

		void RemoveToolbarItem(Page oldItem)
		{
			foreach (var pair in _itemToItemPage)
			{
				if (pair.Value == oldItem)
				{
					pair.Value.PropertyChanged -= OnPageTitleChanged;
					_toolbarItemList.Remove(pair.Key);
					_itemToItemPage.Remove(pair.Key);
					pair.Key.Delete();
					return;
				}
			}
		}

		void ResetToolbarItems()
		{
			_isResettingToolbarItems = true;
			foreach (var pair in _itemToItemPage)
			{
				pair.Value.PropertyChanged -= OnPageTitleChanged;
				pair.Key.Delete();
			}
			_itemToItemPage.Clear();
			_toolbarItemList.Clear();

			FillToolbarAndContents();
			_isResettingToolbarItems = false;
		}

		void FillToolbarAndContents()
		{
			int index = 0;
			//add items to toolbar
			foreach (Page child in Element.Children)
			{
				EToolbarItem toolbarItem = AddToolbarItem(child, index++);

				if (Element.CurrentPage == child)
				{
					//select item on the toolbar and fill content
					toolbarItem.IsSelected = true;
				}
			}
		}

		void OnToolbarItemSelected(object sender, EToolbarItemEventArgs e)
		{
			if (_toolbar.SelectedItem == null || _isResettingToolbarItems)
				return;
			if (_isUpdateByCurrentPage || _isUpdateByScroller)
				return;
			_isUpdateByToolbar = true;

			var oldPage = Element.CurrentPage;
			var newPage = _itemToItemPage[_toolbar.SelectedItem];

			if (oldPage != newPage)
			{
				oldPage?.SendDisappearing();
				Element.CurrentPage = newPage;
				newPage?.SendAppearing();

				int index = MultiPage<Page>.GetIndex(newPage);
				_scroller.ScrollTo(index, 0, true);

				Element.UpdateFocusTreePolicy();
			}
			_isUpdateByToolbar = false;
		}

		void OnCurrentPageChanged()
		{
			// To update TabIndex order
			CustomFocusManager.StartReorderTabIndex();

			if (_isUpdateByScroller || _isUpdateByToolbar || !_isInitialized)
				return;

			_isUpdateByCurrentPage = true;
			Page oldPage = null;
			if (_toolbar.SelectedItem != null && _itemToItemPage.ContainsKey(_toolbar.SelectedItem))
				oldPage = _itemToItemPage[_toolbar.SelectedItem];

			oldPage?.SendDisappearing();
			Element.CurrentPage?.SendAppearing();

			int index = MultiPage<Page>.GetIndex(Element.CurrentPage);
			_toolbarItemList[index].IsSelected = true;
			_scroller.ScrollTo(index, 0, true);

			Element.UpdateFocusTreePolicy();
			_isUpdateByCurrentPage = false;
		}

		void ApplyBarItemColors(EToolbarItem item, BarItemColorType type, EColor color)
		{
			if (color.IsDefault)
			{
				ClearBarItemColors(item, type);
			}
			else
			{
				switch (type)
				{
					case BarItemColorType.Background:
						item.SetPartColor("bg", color);
						break;
					case BarItemColorType.Text:
						if (string.IsNullOrEmpty(item.Icon))
						{
							item.SetPartColor("text", color);
							item.SetPartColor("text_pressed", color);
							item.SetPartColor("text_selected", color);
						}
						else
						{
							item.SetPartColor("text_under_icon", color);
							item.SetPartColor("text_under_icon_pressed", color);
							item.SetPartColor("text_under_icon_selected", color);
						}
						item.SetPartColor("underline", color);
						break;
					case BarItemColorType.SelectedTab:
						if (string.IsNullOrEmpty(item.Icon))
						{
							item.SetPartColor("text_selected", color);
						}
						else
						{
							item.SetPartColor("text_under_icon_selected", color);
							item.SetPartColor("icon_selected", color);
						}
						item.SetPartColor("underline", color);
						break;
					case BarItemColorType.UnselectedTab:
						if (string.IsNullOrEmpty(item.Icon))
						{
							item.SetPartColor("text", color);
							item.SetPartColor("text_pressed", color);
						}
						else
						{
							item.SetPartColor("text_under_icon", color);
							item.SetPartColor("text_under_icon_pressed", color);
							item.SetPartColor("icon", color);
							item.SetPartColor("icon_pressed", color);
						}
						break;
					default:
						break;
				}
			}
		}

		void ClearBarItemColors(EToolbarItem item, BarItemColorType type)
		{
			switch (type)
			{
				case BarItemColorType.Background:
					item.DeletePartColor("bg");
					break;
				case BarItemColorType.Text:
					if (string.IsNullOrEmpty(item.Icon))
					{
						item.DeletePartColor("text");
						item.DeletePartColor("text_pressed");
						item.DeletePartColor("text_selected");
					}
					else
					{
						item.DeletePartColor("text_under_icon");
						item.DeletePartColor("text_under_icon_pressed");
						item.DeletePartColor("text_under_icon_selected");
					}
					item.DeletePartColor("underline");
					break;
				case BarItemColorType.SelectedTab:
					if (string.IsNullOrEmpty(item.Icon))
					{
						item.DeletePartColor("text_selected");
					}
					else
					{
						item.DeletePartColor("text_under_icon_selected");
						item.DeletePartColor("icon_selected");
					}
					item.DeletePartColor("underline");
					break;
				case BarItemColorType.UnselectedTab:
					if (string.IsNullOrEmpty(item.Icon))
					{
						item.DeletePartColor("text");
						item.DeletePartColor("text_pressed");
					}
					else
					{
						item.DeletePartColor("text_under_icon");
						item.DeletePartColor("text_under_icon_pressed");
						item.DeletePartColor("icon");
						item.DeletePartColor("icon_pressed");
					}
					break;
				default:
					break;
			}
		}
	}

	enum BarItemColorType
	{
		Background,
		Text,
		SelectedTab,
		UnselectedTab
	}
}
