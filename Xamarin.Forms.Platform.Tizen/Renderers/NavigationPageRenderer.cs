using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using ElmSharp;
using EButton = ElmSharp.Button;
using EToolbar = ElmSharp.Toolbar;
using EToolbarItem = ElmSharp.ToolbarItem;

namespace Xamarin.Forms.Platform.Tizen
{
	public class NavigationPageRenderer : VisualElementRenderer<NavigationPage>
	{
		enum ToolbarButtonPosition
		{
			Left,
			Right
		};

		const string PartTitle = "default";
		const string PartBackButton = "elm.swallow.prev_btn";
		const string PartLeftToolbar = "title_left_btn";
		const string PartRightToolbar = "title_right_btn";
		const string PartNavigationBar = "navigationbar";
		const string StyleLeftToolBarButton = "naviframe/title_left";
		const string StyleRightToolbarButton = "naviframe/title_right";
		const string StyleBackButton = "naviframe/back_btn/default";
		const string StyleDefaultToolbarIcon = "naviframe/drawers";
		const string StyleNavigationBar = "navigationbar";

		readonly List<Widget> _naviItemContentPartList = new List<Widget>();
		Naviframe _naviFrame = null;
		Page _previousPage = null;
		TaskCompletionSource<bool> _currentTaskSource = null;
		ToolbarTracker _toolbarTracker = null;
		IDictionary<Page, NaviItem> _naviItemMap;

		Page CurrentPage => Element.CurrentPage;
		Page PreviousPage => Element.Navigation.NavigationStack.Count > 1 ? Element.Navigation.NavigationStack[Element.Navigation.NavigationStack.Count - 2] : null;
		NaviItem CurrentNaviItem => _naviFrame.NavigationStack.Count > 0 ? _naviFrame.NavigationStack.Last() : null;
		NaviItem PreviousNaviItem => _naviFrame.NavigationStack.Count > 1 ? _naviFrame.NavigationStack[_naviFrame.NavigationStack.Count - 2] : null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_naviFrame != null)
				{
					_naviFrame.AnimationFinished -= OnAnimationFinished;
				}
				if (_toolbarTracker != null)
				{
					_toolbarTracker.CollectionChanged -= OnToolbarCollectionChanged;
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
		{
			if (_naviFrame == null)
			{
				_naviFrame = new Naviframe(Forms.NativeParent);
				_naviFrame.PreserveContentOnPop = true;
				_naviFrame.DefaultBackButtonEnabled = false;
				_naviFrame.AnimationFinished += OnAnimationFinished;

				SetNativeView(_naviFrame);
				_naviItemMap = new Dictionary<Page, NaviItem>();
			}

			if (_toolbarTracker == null)
			{
				_toolbarTracker = new ToolbarTracker();
				_toolbarTracker.CollectionChanged += OnToolbarCollectionChanged;
			}

			if (e.OldElement != null)
			{
				var navigation = e.OldElement as INavigationPageController;
				navigation.PopRequested -= OnPopRequested;
				navigation.PopToRootRequested -= OnPopToRootRequested;
				navigation.PushRequested -= OnPushRequested;
				navigation.RemovePageRequested -= OnRemovePageRequested;
				navigation.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;

				var pageController = e.OldElement as IPageController;
				pageController.InternalChildren.CollectionChanged -= OnPageCollectionChanged;
			}

			if (e.NewElement != null)
			{
				var navigation = e.NewElement as INavigationPageController;
				navigation.PopRequested += OnPopRequested;
				navigation.PopToRootRequested += OnPopToRootRequested;
				navigation.PushRequested += OnPushRequested;
				navigation.RemovePageRequested += OnRemovePageRequested;
				navigation.InsertPageBeforeRequested += OnInsertPageBeforeRequested;

				_toolbarTracker.Target = e.NewElement;
				_previousPage = e.NewElement.CurrentPage;
			}
			base.OnElementChanged(e);
		}

		protected override void OnElementReady()
		{
			base.OnElementReady();
			var pageController = Element as IPageController;
			pageController.InternalChildren.CollectionChanged += OnPageCollectionChanged;

			foreach (Page page in pageController.InternalChildren)
			{
				_naviItemMap[page] = _naviFrame.Push(Platform.GetRenderer(page).NativeView, SpanTitle(page.Title));
				page.PropertyChanged += NavigationBarPropertyChangedHandler;

				UpdateHasNavigationBar(page);
			}
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					(_previousPage as IPageController)?.SendDisappearing();
					_previousPage = Element.CurrentPage;
					(_previousPage as IPageController)?.SendAppearing();
				});
			}
			else if (e.PropertyName == NavigationPage.BarTextColorProperty.PropertyName)
				UpdateTitle(CurrentPage);
			// Tizen does not support 'Tint', but only 'BarBackgroundColor'
			else if (e.PropertyName == NavigationPage.BarBackgroundColorProperty.PropertyName)
				UpdateBarBackgroundColor(CurrentNaviItem);
			else if (TizenPlatformServices.AppDomain.IsTizenSpecificAvailable && e.PropertyName == "HasBreadCrumbsBar")
				UpdateBreadCrumbsBar(CurrentNaviItem);

		}

		void OnPageCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
				foreach (Page page in e.OldItems)
					page.PropertyChanged -= NavigationBarPropertyChangedHandler;
			if (e.NewItems != null)
				foreach (Page page in e.NewItems)
					page.PropertyChanged += NavigationBarPropertyChangedHandler;
		}

		void OnToolbarCollectionChanged(object sender, EventArgs eventArgs)
		{
			UpdateToolbarItem(Element.CurrentPage);
		}

		void NavigationBarPropertyChangedHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			// this handler is invoked only for child pages (contained on a navigation stack)
			if (e.PropertyName == NavigationPage.HasNavigationBarProperty.PropertyName)
				UpdateHasNavigationBar(sender as Page);
			else if (e.PropertyName == NavigationPage.HasBackButtonProperty.PropertyName ||
				e.PropertyName == NavigationPage.BackButtonTitleProperty.PropertyName)
				UpdateHasBackButton(sender as Page);
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateTitle(sender as Page);
			else if (TizenPlatformServices.AppDomain.IsTizenSpecificAvailable && e.PropertyName == "BreadCrumb")
				UpdateBreadCrumbsBar(GetNaviItemForPage(sender as Page));
		}

		void UpdateHasNavigationBar(Page page)
		{
			NaviItem item = GetNaviItemForPage(page);
			//According to TV UX Guideline, item style should be set to "tabbar" in case of TabbedPage only for TV profile.
			if (Device.Idiom == TargetIdiom.TV)
			{
				item.Style = page is TabbedPage ? "tabbar" : "default";
			}
			item.TitleBarVisible = (bool)page.GetValue(NavigationPage.HasNavigationBarProperty);
			UpdateToolbarItem(page, item);
			UpdateBarBackgroundColor(item);
			if (TizenPlatformServices.AppDomain.IsTizenSpecificAvailable)
				UpdateBreadCrumbsBar(item);
		}

		void UpdateToolbarItem(Page page, NaviItem item = null)
		{
			if (item == null)
				item = GetNaviItemForPage(page);

			if (_naviFrame.NavigationStack.Count == 0 || item == null || item != _naviFrame.NavigationStack.Last())
				return;

			Native.Button rightButton = GetToolbarButton(ToolbarButtonPosition.Right);
			item.SetPartContent(PartRightToolbar, rightButton);

			Native.Button leftButton = GetToolbarButton(ToolbarButtonPosition.Left);
			item.SetPartContent(PartLeftToolbar, leftButton);
			UpdateHasBackButton(page, item);
		}

		void UpdateHasBackButton(Page page, NaviItem item = null)
		{
			if (item == null)
				item = GetNaviItemForPage(page);

			EButton button = null;

			if ((bool)page.GetValue(NavigationPage.HasBackButtonProperty) && _naviFrame.NavigationStack.Count > 1)
			{
				button = CreateNavigationButton((string)page.GetValue(NavigationPage.BackButtonTitleProperty));
			}
			item.SetPartContent(PartBackButton, button);
		}

		void UpdateTitle(Page page, NaviItem item = null)
		{
			if (item == null)
				item = GetNaviItemForPage(page);

			item.SetPartText(PartTitle, SpanTitle(page.Title));
		}

		string SpanTitle(string Title)
		{
			Native.Span span = new Native.Span
			{
				Text = Title,
				HorizontalTextAlignment = Native.TextAlignment.Center,
				ForegroundColor = Element.BarTextColor.ToNative()
			};
			return span.GetMarkupText();
		}

		void UpdateBarBackgroundColor(NaviItem item)
		{
			item.TitleBarBackgroundColor = Element.BarBackgroundColor.ToNative();
		}

		void UpdateNavigationBar(Page page, NaviItem item = null)
		{
			if (item == null)
				item = GetNaviItemForPage(page);

			UpdateTitle(page, item);
			UpdateBarBackgroundColor(item);
		}

		void UpdateBreadCrumbsBar(NaviItem item)
		{
			if (Element.OnThisPlatform().HasBreadCrumbsBar())
			{
				item.Style = StyleNavigationBar;
				item.SetPartContent(PartNavigationBar, GetBreadCrumbsBar());
			}
			else
			{
				item.SetPartContent(PartNavigationBar, null, false);
			}
		}

		EButton CreateNavigationButton(string text)
		{
			EButton button = new EButton(Forms.NativeParent);
			button.Clicked += (sender, e) =>
			{
				if (!Element.SendBackButtonPressed())
					Forms.Context.Exit();
			};

			button.Style = StyleBackButton;
			button.Text = text;

			_naviItemContentPartList.Add(button);
			button.Deleted += NaviItemPartContentDeletedHandler;

			return button;
		}

		void NaviItemPartContentDeletedHandler(object sender, EventArgs e)
		{
			_naviItemContentPartList.Remove(sender as Widget);
		}

		NaviItem GetNaviItemForPage(Page page)
		{
			NaviItem item;
			if (_naviItemMap.TryGetValue(page, out item))
			{
				return item;
			}
			return null;
		}

		Native.Button GetToolbarButton(ToolbarButtonPosition position)
		{
			ToolbarItem item = _toolbarTracker.ToolbarItems.Where(
				i => (position == ToolbarButtonPosition.Right && i.Order <= ToolbarItemOrder.Primary)
				|| (position == ToolbarButtonPosition.Left && i.Order == ToolbarItemOrder.Secondary))
				.OrderBy(i => i.Priority).FirstOrDefault();

			if (item == default(ToolbarItem))
				return null;

			Native.Button button = new Native.Button(Forms.NativeParent);
			button.Clicked += (s, e) =>
			{
				IMenuItemController control = item;
				control.Activate();
			};
			button.Text = item.Text;
			button.BackgroundColor = Color.Transparent.ToNative();

			if (string.IsNullOrEmpty(item.Icon))
			{
				if (string.IsNullOrEmpty(item.Text))
				{
					// We assumed the default toolbar icon is "naviframe/drawer" if there are no icon and text.
					button.Style = StyleDefaultToolbarIcon;
				}
				else
				{
					if (position == ToolbarButtonPosition.Right)
						button.Style = StyleRightToolbarButton;
					else
						button.Style = StyleLeftToolBarButton;
				}
			}
			else
			{
				Native.Image iconImage = new Native.Image(Forms.NativeParent);
				var task = iconImage.LoadFromImageSourceAsync(item.Icon);
				button.Image = iconImage;
			}

			return button;
		}

		EToolbar GetBreadCrumbsBar()
		{
			EToolbar toolbar = new EToolbar(Forms.NativeParent)
			{
				Style = StyleNavigationBar,
				ItemAlignment = 0,
				Homogeneous = false,
				ShrinkMode = ToolbarShrinkMode.Scroll
			};

			foreach (var p in Element.Navigation.NavigationStack)
			{
				string breadCrumb = p.OnThisPlatform().GetBreadCrumb();
				if (!string.IsNullOrEmpty(breadCrumb))
				{
					EToolbarItem toolbarItem = toolbar.Append(breadCrumb);
					toolbarItem.Selected += (s, e) =>
					{
						var copyOfStack = Element.Navigation.NavigationStack.Reverse().Skip(1);
						foreach (var lp in copyOfStack)
						{
							if (lp == p) break;
							Element.Navigation.RemovePage(lp);
						}
						if (Element.Navigation.NavigationStack.Last() != p)
							Element.Navigation.PopAsync();
					};
				}
			}

			return toolbar;
		}

		void OnPopRequested(object sender, NavigationRequestedEventArgs nre)
		{
			if ((Element as IPageController).InternalChildren.Count == _naviFrame.NavigationStack.Count)
			{
				nre.Page?.SendDisappearing();
				UpdateNavigationBar(PreviousPage, PreviousNaviItem);

				if (nre.Animated)
				{
					_naviFrame.Pop();

					_currentTaskSource = new TaskCompletionSource<bool>();
					nre.Task = _currentTaskSource.Task;

					// There is no TransitionFinished (AnimationFinished) event after Pop the last page
					if (_naviFrame.NavigationStack.Count == 0)
						CompleteCurrentNavigationTask();
				}
				else
				{
					CurrentNaviItem?.Delete();
				}

				if (_naviItemMap.ContainsKey(nre.Page))
					_naviItemMap.Remove(nre.Page);
			}
		}

		void OnPopToRootRequested(object sender, NavigationRequestedEventArgs nre)
		{
			List<NaviItem> copyOfStack = new List<NaviItem>(_naviFrame.NavigationStack);
			NaviItem rootItem = copyOfStack.FirstOrDefault();
			NaviItem topItem = copyOfStack.LastOrDefault();

			foreach (NaviItem naviItem in copyOfStack)
				if (naviItem != rootItem && naviItem != topItem)
					naviItem.Delete();

			if (topItem != rootItem)
			{
				UpdateNavigationBar(Element.Navigation.NavigationStack.Last(), rootItem);
				if (nre.Animated)
				{
					_naviFrame.Pop();

					_currentTaskSource = new TaskCompletionSource<bool>();
					nre.Task = _currentTaskSource.Task;
				}
				else
					topItem?.Delete();
			}

			_naviItemMap.Clear();
			_naviItemMap[Element.Navigation.NavigationStack.Last()] = rootItem;
		}

		void OnPushRequested(object sender, NavigationRequestedEventArgs nre)
		{
			if (nre.Animated || _naviFrame.NavigationStack.Count == 0)
			{
				_naviItemMap[nre.Page] = _naviFrame.Push(Platform.GetOrCreateRenderer(nre.Page).NativeView, SpanTitle(nre.Page.Title));
				_currentTaskSource = new TaskCompletionSource<bool>();
				nre.Task = _currentTaskSource.Task;

				// There is no TransitionFinished (AnimationFinished) event after the first Push
				if (_naviFrame.NavigationStack.Count == 1)
					CompleteCurrentNavigationTask();
			}
			else
			{
				_naviItemMap[nre.Page] = _naviFrame.InsertAfter(_naviFrame.NavigationStack.Last(), Platform.GetOrCreateRenderer(nre.Page).NativeView, SpanTitle(nre.Page.Title));
			}
			UpdateHasNavigationBar(nre.Page);
		}

		void OnRemovePageRequested(object sender, NavigationRequestedEventArgs nre)
		{
			GetNaviItemForPage(nre.Page).Delete();
			if (_naviItemMap.ContainsKey(nre.Page))
				_naviItemMap.Remove(nre.Page);
		}

		void OnInsertPageBeforeRequested(object sender, NavigationRequestedEventArgs nre)
		{
			if (nre.BeforePage == null)
				throw new ArgumentNullException("before");
			if (nre.Page == null)
				throw new ArgumentNullException("page");

			EvasObject page = Platform.GetOrCreateRenderer(nre.Page).NativeView;
			_naviItemMap[nre.Page] = _naviFrame.InsertBefore(GetNaviItemForPage(nre.BeforePage), page, SpanTitle(nre.Page.Title));
			UpdateHasNavigationBar(nre.Page);
		}

		void OnAnimationFinished(object sender, EventArgs e)
		{
			CompleteCurrentNavigationTask();
		}

		void CompleteCurrentNavigationTask()
		{
			if (_currentTaskSource != null)
			{
				var tmp = _currentTaskSource;
				_currentTaskSource = null;
				tmp.SetResult(true);
			}
		}
	}
}
