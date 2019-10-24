using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class NavigationPageRenderer : VisualElementRenderer<NavigationPage, FrameworkElement>
	{
		Page _currentRoot;
		bool _isRemoving;

		public NavigationPageRenderer()
		{
			AutoPackage = false;
		}

		IPageController PageController => Element as IPageController;
		IElementController ElementController => Element as IElementController;

		protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
		{
			base.OnElementChanged(e);

			Debug.WriteLine("Warning, Windows Phone backend does not support NavigationPage, falling back to global navigation.");

			Action init = () =>
			{
				var navController = (INavigationPageController)Element;

				navController.PushRequested += PageOnPushed;
				navController.PopRequested += PageOnPopped;
				navController.PopToRootRequested += PageOnPoppedToRoot;
				navController.RemovePageRequested += RemovePageRequested;
				navController.InsertPageBeforeRequested += ElementOnInsertPageBeforeRequested;
				Element.PropertyChanged += OnElementPropertyChanged;

				var platform = (Platform)Element.Platform;

				PageController.ContainerArea = new Rectangle(new Point(0, 0), platform.Size);

				platform.SizeChanged += (sender, args) => PageController.ContainerArea = new Rectangle(new Point(0, 0), platform.Size);

				List<Page> stack = GetStack();
				if (stack.Count > 0)
					UpdateRootPage(stack);
				else
					return;

				Device.BeginInvokeOnMainThread(() =>
				{
					for (var i = 0; i < stack.Count; i++)
						PageOnPushed(this, new NavigationRequestedEventArgs(stack[i], false, i != 0));
				});
			};

			if (Element.Platform == null)
				Element.PlatformSet += (sender, args) => init();
			else
				init();

			Loaded += (sender, args) => PageController.SendAppearing();
			Unloaded += OnUnloaded;
		}

		void ElementOnInsertPageBeforeRequested(object sender, NavigationRequestedEventArgs eventArgs)
		{
			if (Element.Platform == null)
				return;
			var platform = Element.Platform as Platform;
			if (platform != null)
				((INavigation)platform).InsertPageBefore(eventArgs.Page, eventArgs.BeforePage);

			List<Page> stack = GetStack();
			stack.Insert(stack.IndexOf(eventArgs.BeforePage), eventArgs.Page);

			UpdateRootPage(stack);
		}

		List<Page> GetStack()
		{
			int count = PageController.InternalChildren.Count;
			var stack = new List<Page>(count);
			for (var i = 0; i < count; i++)
				stack.Add((Page)PageController.InternalChildren[i]);

			return stack;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "Parent" || Element.RealParent != null)
				return;

			var platform = Element.Platform as Platform;

			if (platform == null)
				return;

			for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
			{
				var page = ElementController.LogicalChildren[i] as Page;
				if (page != null)
					platform.RemovePage(page, false);
			}
		}

		void OnUnloaded(object sender, RoutedEventArgs args)
		{
			PageController.SendDisappearing();
		}

		void PageOnPopped(object sender, NavigationRequestedEventArgs eventArg)
		{
			if (Element.Platform == null)
				return;
			var platform = Element.Platform as Platform;
			if (platform != null)
				eventArg.Task = platform.Pop(Element, eventArg.Animated).ContinueWith((t, o) => true, null);
		}

		void PageOnPoppedToRoot(object sender, NavigationRequestedEventArgs eventArgs)
		{
			if (Element.Platform == null)
				return;
			var platform = Element.Platform as Platform;
			if (platform != null)
				eventArgs.Task = platform.PopToRoot(Element, eventArgs.Animated).ContinueWith((t, o) => true, null);
		}

		void PageOnPushed(object sender, NavigationRequestedEventArgs e)
		{
			if (Element.Platform == null)
				return;
			var platform = Element.Platform as Platform;
			if (platform != null)
			{
				if (e.Page == ((INavigationPageController)Element).Pages.FirstOrDefault()) 
					((IPageController)e.Page).IgnoresContainerArea = true;
				e.Task = platform.PushCore(e.Page, Element, e.Animated, e.Realize).ContinueWith((t, o) => true, null);
			}
		}

		void RemovePageRequested(object sender, NavigationRequestedEventArgs eventArgs)
		{
			if (Element.Platform == null)
				return;
			var platform = Element.Platform as Platform;
			if (platform != null)
				((INavigation)platform).RemovePage(eventArgs.Page);

			List<Page> stack = GetStack();
			stack.Remove(eventArgs.Page);
			_isRemoving = true;
			UpdateRootPage(stack);
			_isRemoving = false;
		}

		void UpdateRootPage(IReadOnlyList<Page> stack)
		{
			Page first = stack.FirstOrDefault();
			if (first == _currentRoot)
				return;

			if (Children.Count > 0)
			{
				var renderer = Children[0] as IVisualElementRenderer;
				if (renderer != null)
				{
					Children.RemoveAt(0);

					var page = renderer.Element as IPageController;
					if (page != null)
						page.IgnoresContainerArea = false;

					if (!stack.Contains(renderer.Element))
						Platform.SetRenderer(renderer.Element, null);
				}
			}

			_currentRoot = first;

			if (first == null)
				return;

			((IPageController)first).IgnoresContainerArea = true;

			IVisualElementRenderer firstRenderer = Platform.GetRenderer(first);
			if (firstRenderer == null)
			{
				firstRenderer = Platform.CreateRenderer(first);
				Platform.SetRenderer(first, firstRenderer);
			}
			var uiElement = (UIElement)firstRenderer;
			var platform = Element.Platform as Platform;
			Canvas canvas = platform?.GetCanvas();

			//We could be swapping the visible page,
			//so let's make sure we remove it
			if (canvas.Children.Contains(uiElement))
				canvas.Children.Remove(uiElement);
			Children.Add(uiElement);

			// we removed the previous root page, and the new root page is the one being presented
			// at this time there's only 1 page now on the stack (the navigationpage with root)
			// we need to update the platform to set this root page as the visible again
			bool updateRoot = Element.CurrentPage == first && _isRemoving;
			if (updateRoot)
				platform.SetCurrent(Element, false);
		}
	}
}