using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElmSharp;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Handlers;
using Tizen.UIExtensions.ElmSharp;
using TButton = Tizen.UIExtensions.ElmSharp.Button;
using TSpan = Tizen.UIExtensions.Common.Span;
using TTextAlignment = Tizen.UIExtensions.Common.TextAlignment;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class NavigationPageHandler :
		ViewHandler<NavigationPage, Naviframe>
	{
		readonly List<Widget> _naviItemContentPartList = new List<Widget>();
		TaskCompletionSource<bool> _currentTaskSource = null;
		IDictionary<Page, NaviItem> _naviItemMap;

		Page PreviousPage => VirtualView.Navigation.NavigationStack.Count > 1 ? VirtualView.Navigation.NavigationStack[VirtualView.Navigation.NavigationStack.Count - 2] : null;
		NaviItem CurrentNaviItem => NativeView.NavigationStack.Count > 0 ? NativeView.NavigationStack.Last() : null;
		NaviItem PreviousNaviItem => NativeView.NavigationStack.Count > 1 ? NativeView.NavigationStack[NativeView.NavigationStack.Count - 2] : null;

		protected override Naviframe CreateNativeView()
		{
			return new Naviframe(NativeParent)
			{
				PreserveContentOnPop = true,
				DefaultBackButtonEnabled = false,
			};
		}

		protected override void ConnectHandler(Naviframe nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.AnimationFinished += OnAnimationFinished;
			_naviItemMap = new Dictionary<Page, NaviItem>();

			if (VirtualView == null)
				return;

			VirtualView.PushRequested += OnPushRequested;
			VirtualView.PopRequested += OnPopRequested;
			VirtualView.InternalChildren.CollectionChanged += OnPageCollectionChanged;

			foreach (Page page in VirtualView.InternalChildren)
			{
				_naviItemMap[page] = NativeView.Push(CreateNavItem(page), SpanTitle(page.Title));
				page.PropertyChanged += NavigationBarPropertyChangedHandler;

				UpdateHasNavigationBar(page);
			}
		}

		protected override void DisconnectHandler(Naviframe nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.AnimationFinished -= OnAnimationFinished;

			VirtualView.PushRequested -= OnPushRequested;
			VirtualView.PopRequested -= OnPopRequested;
			VirtualView.InternalChildren.CollectionChanged -= OnPageCollectionChanged;
		}

		public static void MapPadding(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapBarTextColor(NavigationPageHandler handler, NavigationPage view) 
		{ 
			handler.UpdateTitle(view.CurrentPage); 
		}

		public static void MapBarBackground(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapTitleIcon(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapTitleView(NavigationPageHandler handler, NavigationPage view) { }

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

		void OnPushRequested(object sender, NavigationRequestedEventArgs nre)
		{
			if (nre.Animated || NativeView.NavigationStack.Count == 0)
			{
				_naviItemMap[nre.Page] = NativeView.Push(CreateNavItem(nre.Page), SpanTitle(nre.Page.Title));
				_currentTaskSource = new TaskCompletionSource<bool>();
				nre.Task = _currentTaskSource.Task;

				// There is no TransitionFinished (AnimationFinished) event after the first Push
				if (NativeView.NavigationStack.Count == 1)
					CompleteCurrentNavigationTask();
			}
			else
			{
				_naviItemMap[nre.Page] = NativeView.InsertAfter(NativeView.NavigationStack.Last(), CreateNavItem(nre.Page), SpanTitle(nre.Page.Title));
			}
			UpdateHasNavigationBar(nre.Page);
		}

		void OnPopRequested(object sender, NavigationRequestedEventArgs nre)
		{
			if (VirtualView.InternalChildren.Count == NativeView.NavigationStack.Count)
			{
				nre.Page?.SendDisappearing();
				UpdateNavigationBar(PreviousPage, PreviousNaviItem);

				if (nre.Animated)
				{
					NativeView.Pop();

					_currentTaskSource = new TaskCompletionSource<bool>();
					nre.Task = _currentTaskSource.Task;

					// There is no TransitionFinished (AnimationFinished) event after Pop the last page
					if (NativeView.NavigationStack.Count == 0)
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

		void UpdateHasNavigationBar(Page page)
		{
			NaviItem item = GetNaviItemForPage(page);
			item.SetTabBarStyle();
			item.TitleBarVisible = (bool)page.GetValue(NavigationPage.HasNavigationBarProperty);
			UpdateBarBackgroundColor(item);
		}

		void UpdateNavigationBar(Page page, NaviItem item = null)
		{
			if (item == null)
				item = GetNaviItemForPage(page);

			UpdateTitle(page, item);
			UpdateBarBackgroundColor(item);
		}

		void UpdateHasBackButton(Page page, NaviItem item = null)
		{
			if (item == null)
				item = GetNaviItemForPage(page);

			TButton button = null;

			if ((bool)page.GetValue(NavigationPage.HasBackButtonProperty) && NativeView.NavigationStack.Count > 1)
			{
				button = CreateNavigationButton((string)page.GetValue(NavigationPage.BackButtonTitleProperty));
			}
			item.SetBackButton(button);
		}

		void UpdateTitle(Page page, NaviItem item = null)
		{
			if (item == null)
				item = GetNaviItemForPage(page);

			item.SetTitle(SpanTitle(page.Title));
		}

		string SpanTitle(string Title)
		{
			TSpan span = new TSpan
			{
				Text = Title,
				HorizontalTextAlignment = TTextAlignment.Center,
				ForegroundColor = VirtualView.BarTextColor.ToNative()
			};
			return span.GetMarkupText();
		}

		void UpdateBarBackgroundColor(NaviItem item)
		{
			item.TitleBarBackgroundColor = VirtualView.BarBackgroundColor.ToNativeEFL();
		}

		TButton CreateNavigationButton(string text)
		{
			var button = new TButton(NativeParent)
			{
				Text = text
			};
			button.SetNavigationBackStyle();
			button.Clicked += (sender, e) =>
			{
				if (!VirtualView.SendBackButtonPressed())
					Tizen.Applications.Application.Current.Exit();
			};
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

		EvasObject CreateNavItem(Page page)
		{
			return page.ToNative(MauiContext);
		}
	}
}
