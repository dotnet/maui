using System;
using Tizen.UIExtensions.NUI;
using NView = Tizen.NUI.BaseComponents.View;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class NavigationPageRenderer : VisualElementRenderer<NavigationPage>
	{
		Page _previousPage = null;

		NavigationStack Control => NativeView as NavigationStack;

		protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
		{
			if (NativeView == null)
			{
				SetNativeView(new NavigationStack
				{
					HeightSpecification = global::Tizen.NUI.BaseComponents.LayoutParamPolicies.MatchParent,
					WidthSpecification = global::Tizen.NUI.BaseComponents.LayoutParamPolicies.MatchParent,
					WidthResizePolicy = global::Tizen.NUI.ResizePolicyType.FillToParent,
					HeightResizePolicy = global::Tizen.NUI.ResizePolicyType.FillToParent,
				});
			}

			if (e.NewElement != null)
			{
				var navigation = e.NewElement as INavigationPageController;
				navigation.PopRequested += OnPopRequested;
				navigation.PopToRootRequested += OnPopToRootRequested;
				navigation.PushRequested += OnPushRequested;
				navigation.RemovePageRequested += OnRemovePageRequested;
				navigation.InsertPageBeforeRequested += OnInsertPageBeforeRequested;
			}
			base.OnElementChanged(e);
		}

		protected override void OnElementReady()
		{
			base.OnElementReady();
			var pageController = Element as IPageController;
			foreach (Page page in pageController.InternalChildren)
			{
				Control.Push(GetNavigationItem(page), false);
			}
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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

		void OnPopRequested(object sender, NavigationRequestedEventArgs nre)
		{
			nre.Page?.SendDisappearing();
			Control.Pop(false);
			nre.Task = Task.FromResult(true);
		}

		void OnPopToRootRequested(object sender, NavigationRequestedEventArgs nre)
		{
			Control.PopToRoot();
			nre.Task = Task.FromResult(true);
		}

		void OnPushRequested(object sender, NavigationRequestedEventArgs nre)
		{
			Control.Push(GetNavigationItem(nre.Page), false);
			nre.Task = Task.FromResult(true);
		}

		void OnRemovePageRequested(object sender, NavigationRequestedEventArgs nre)
		{
			Control.Remove(Platform.GetRenderer(nre.Page).NativeView);
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

		NView GetNavigationItem(Page page)
		{
			return Platform.GetOrCreateRenderer(page).NativeView;
		}
	}
}
