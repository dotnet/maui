using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.Common.GraphicsView;
using Tizen.UIExtensions.NUI;
using NColor = Tizen.NUI.Color;
using NShadow = Tizen.NUI.Shadow;
using NVector2 = Tizen.NUI.Vector2;
using NView = Tizen.NUI.BaseComponents.View;
using TButton = Tizen.UIExtensions.NUI.Button;
using TColor = Tizen.UIExtensions.Common.Color;
using TDeviceInfo = Tizen.UIExtensions.Common.DeviceInfo;
using TMaterialIconButton = Tizen.UIExtensions.NUI.GraphicsView.MaterialIconButton;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class NavigationPageRenderer : VisualElementRenderer<NavigationPage>
	{
		const double s_toolbarItemTextSize = 16d;
		const double s_titleViewTextSize = 20d;

		Dictionary<Page, NaviPage> _pageMap = new Dictionary<Page, NaviPage>();

		Page _previousPage = null;
		NavigationStack Control => NativeView as NavigationStack;
		ToolbarTracker _toolbarTracker = null;
		TColor _accentColor = TColor.White;

		protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
		{
			if (NativeView == null)
			{
				SetNativeView(new NavigationStack
				{
					HeightSpecification = LayoutParamPolicies.MatchParent,
					WidthSpecification = LayoutParamPolicies.MatchParent,
					PushAnimation = (v, p) => v.Opacity = 0.5f + 0.5f * (float)p,
					PopAnimation = (v, p) => v.Opacity = 0.5f + 0.5f * (float)(1 - p),
				});
			}
			if (_toolbarTracker == null)
			{
				_toolbarTracker = new ToolbarTracker();
				_toolbarTracker.CollectionChanged += OnToolbarCollectionChanged;
			}

			if (e.NewElement != null)
			{
				var navigation = e.NewElement as INavigationPageController;
				navigation.PopRequested += OnPopRequested;
				navigation.PopToRootRequested += OnPopToRootRequested;
				navigation.PushRequested += OnPushRequested;
				navigation.RemovePageRequested += OnRemovePageRequested;
				navigation.InsertPageBeforeRequested += OnInsertPageBeforeRequested;
				(Element as IPageController).InternalChildren.CollectionChanged += OnPageCollectionChanged;

				_toolbarTracker.Target = Element;
				_previousPage = e.NewElement.CurrentPage;
			}
			base.OnElementChanged(e);

			var pageController = Element as IPageController;
			foreach (Page page in pageController.InternalChildren)
			{
				Control.Push(GetNavigationItem(page), false);
				page.PropertyChanged += OnPagePropertyChanged;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
			{
				Application.Current.Dispatcher.Dispatch(() =>
				{
					if (IsDisposed)
						return;

					(_previousPage as IPageController)?.SendDisappearing();
					_previousPage = Element.CurrentPage;
					(_previousPage as IPageController)?.SendAppearing();
				});
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_toolbarTracker != null)
				{
					_toolbarTracker.CollectionChanged -= OnToolbarCollectionChanged;
				}

				var navigation = Element as INavigationPageController;
				navigation.PopRequested -= OnPopRequested;
				navigation.PopToRootRequested -= OnPopToRootRequested;
				navigation.PushRequested -= OnPushRequested;
				navigation.RemovePageRequested -= OnRemovePageRequested;
				navigation.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;
				(Element as IPageController).InternalChildren.CollectionChanged -= OnPageCollectionChanged;
				foreach (var child in (Element as IPageController).InternalChildren)
				{
					child.PropertyChanged -= OnPagePropertyChanged;
				}
			}
			base.Dispose(disposing);
		}


		void OnPageCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
				foreach (Page page in e.OldItems)
					page.PropertyChanged -= OnPagePropertyChanged;
			if (e.NewItems != null)
				foreach (Page page in e.NewItems)
					page.PropertyChanged += OnPagePropertyChanged;
		}

		void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == NavigationPage.HasNavigationBarProperty.PropertyName)
				UpdateNavigationBar(sender as Page);
			else if (e.PropertyName == NavigationPage.HasBackButtonProperty.PropertyName)
				UpdateNavigationBar(sender as Page);
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateNavigationBar(sender as Page);
		}

		async void OnPopRequested(object sender, NavigationRequestedEventArgs nre)
		{
			var tcs = new TaskCompletionSource<bool>();
			nre.Task = tcs.Task;
			nre.Page?.SendDisappearing();

			try
			{
				await Control.Pop(nre.Animated);
				tcs.SetResult(true);
			}
			catch
			{
				tcs.SetResult(false);
			}
			finally
			{
				_pageMap.Remove(nre.Page);
				Platform.GetRenderer(nre.Page)?.Dispose();
			}
		}

		void OnPopToRootRequested(object sender, NavigationRequestedEventArgs nre)
		{
			if (Control.Stack.Count <= 1)
			{
				nre.Task = Task.FromResult(true);
				return;
			}

			var rootPage = nre.Page;
			var rootNaviPage = _pageMap[rootPage];

			Control.PopToRoot();

			foreach (var child in _pageMap.Keys)
			{
				if (child != rootPage)
				{
					// remove popped page renderer
					Platform.GetRenderer(child)?.Dispose();
				}
			}

			_pageMap.Clear();
			_pageMap[rootPage] = rootNaviPage;
			nre.Task = Task.FromResult(true);
		}

		async void OnPushRequested(object sender, NavigationRequestedEventArgs nre)
		{
			var tcs = new TaskCompletionSource<bool>();
			nre.Task = tcs.Task;
			try
			{
				await Control.Push(GetNavigationItem(nre.Page), nre.Animated);
				tcs.SetResult(true);
			}
			catch
			{
				tcs.SetResult(false);
			}
		}

		void OnRemovePageRequested(object sender, NavigationRequestedEventArgs nre)
		{
			Control.RemovePage(GetNavigationItem(nre.Page));
			_pageMap.Remove(nre.Page);
			Platform.GetRenderer(nre.Page)?.Dispose();
			nre.Task = Task.FromResult(true);
		}

		void OnInsertPageBeforeRequested(object sender, NavigationRequestedEventArgs nre)
		{
			if (nre.BeforePage == null)
				throw new ArgumentException("BeforePage is null");
			if (nre.Page == null)
				throw new ArgumentException("Page is null");

			Control.Insert(GetNavigationItem(nre.BeforePage), GetNavigationItem(nre.Page));
			nre.Task = Task.FromResult(true);
		}

		NaviPage GetNavigationItem(Page page)
		{
			if (_pageMap.ContainsKey(page))
			{
				return _pageMap[page];
			}

			var content = Platform.GetOrCreateRenderer(page).NativeView;
			content.WidthSpecification = LayoutParamPolicies.MatchParent;
			content.HeightSpecification = LayoutParamPolicies.MatchParent;

			var naviPage = new NaviPage
			{
				Content = content
			};
			_pageMap[page] = naviPage;
			UpdateNavigationBar(page);
			return naviPage;
		}

		void OnToolbarCollectionChanged(object sender, EventArgs eventArgs)
		{
			UpdateNavigationBar(Element.CurrentPage);
		}

		void UpdateNavigationBar(Page page)
		{
			var naviPage = GetNaviItemForPage(page);
			if (naviPage == null)
				return;

			if (!NavigationPage.GetHasNavigationBar(page))
			{
				DisposeTitleViewRenderer(page);
				naviPage.TitleView = null;
				return;
			}

			if (naviPage.TitleView == null)
			{
				naviPage.TitleView = new TitleView();
				naviPage.TitleView.BoxShadow = new NShadow((float)20d.ToScaledDP(), NColor.Black, new NVector2(0, 0));
				naviPage.TitleView.Label.FontSize = s_titleViewTextSize.ToScaledPoint();
			}

			var titleView = naviPage.TitleView;
			if (Element.BarBackgroundColor.IsNotDefault())
			{
				titleView.UpdateBackgroundColor(Element.BarBackgroundColor.ToNative());
			}

			if (Element.BarTextColor.IsNotDefault())
			{
				titleView.Label.TextColor = _accentColor = Element.BarTextColor.ToNative();
			}
			else
			{
				var grayscale = (titleView.BackgroundColor.R + titleView.BackgroundColor.G + titleView.BackgroundColor.B) / 3.0f;
				titleView.Label.TextColor = _accentColor = grayscale > 0.5 ? TColor.Black : TColor.White;
			}


			var hasBackButton = NavigationPage.GetHasBackButton(page) && Control.Stack.Count > 0 && Control.Stack.IndexOf(naviPage) != 0;
			var leftToolbarButton = GetLeftToolbar();

			if (leftToolbarButton != null)
			{
				titleView.Icon = leftToolbarButton;
			}
			else if (hasBackButton)
			{
				titleView.Icon = CreateBackButton();
			}
			else
			{
				titleView.Icon = null;
			}

			titleView.Actions.Clear();
			foreach (var action in GetActions())
			{
				titleView.Actions.Add(action);
			}

			var titleContent = GetTitleContent(page);
			if (titleContent != null)
			{
				titleView.Title = string.Empty;
				titleView.Content = titleContent;
			}
			else
			{
				titleView.Title = page.Title;
			}
		}

		NView GetLeftToolbar()
		{
			ToolbarItem item = _toolbarTracker.ToolbarItems.Where(
				i => i.Order == ToolbarItemOrder.Secondary)
				.OrderBy(i => i.Priority).FirstOrDefault();

			if (item == default(ToolbarItem))
				return null;

			return CreateToolbarButton(item);
		}

		IEnumerable<NView> GetActions()
		{
			return _toolbarTracker.ToolbarItems.Where(i => i.Order <= ToolbarItemOrder.Primary).OrderBy(i => i.Priority).Select(i => CreateToolbarButton(i));
		}

		NView CreateToolbarButton(ToolbarItem item)
		{
			var button = new TButton
			{
				FontSize = s_toolbarItemTextSize.ToScaledPoint(),
				Text = item.Text,
				TextColor = _accentColor,
				HeightSpecification = LayoutParamPolicies.MatchParent,
				WidthSpecification = LayoutParamPolicies.WrapContent,
			};
			button.SizeWidth = (float)button.Measure(TDeviceInfo.ScalingFactor * 80, double.PositiveInfinity).Width;
			button.UpdateBackgroundColor(TColor.Transparent);

			if (item.IconImageSource != null)
			{
				button.Text = string.Empty;
				button.Icon.AdjustViewSize = true;
				button.Icon.HeightSpecification = LayoutParamPolicies.MatchParent;
				_ = button.Icon.LoadFromImageSourceAsync(item.IconImageSource);
				button.SizeWidth = 0;
				button.WidthSpecification = LayoutParamPolicies.WrapContent;
			}
			button.Clicked += (s, e) =>
			{
				item.Command?.Execute(item.CommandParameter);
			};
			return button;
		}

		NView CreateBackButton()
		{
			var button = new TMaterialIconButton
			{
				Icon = MaterialIcons.ArrowBack,
				Color = _accentColor,
			};
			button.Clicked += (s, e) =>
			{
				Element.SendBackButtonPressed();
			};
			return button;
		}

		NView GetTitleContent(Page page)
		{
			View titleView = NavigationPage.GetTitleView(page);
			if (titleView != null)
			{
				titleView.Parent = Element;
				return Platform.GetOrCreateRenderer(titleView).NativeView;
			}
			return null;
		}

		NaviPage GetNaviItemForPage(Page page)
		{
			NaviPage item;
			if (_pageMap.TryGetValue(page, out item))
			{
				return item;
			}
			return null;
		}

		void DisposeTitleViewRenderer(Page page)
		{
			View titleView = NavigationPage.GetTitleView(page);
			if (titleView != null)
				Platform.GetRenderer(titleView)?.Dispose();
		}
	}
	static class NavigationStackEx
	{
		public static void RemovePage(this NavigationStack stack, NaviPage page)
		{
			var property = typeof(NavigationStack).GetProperty("InternalStack", BindingFlags.NonPublic | BindingFlags.Instance);
			List<NView> internalStack = (List<NView>)property.GetValue(stack);
			stack.Remove(page);
			internalStack.Remove(page);
		}
	}
}
