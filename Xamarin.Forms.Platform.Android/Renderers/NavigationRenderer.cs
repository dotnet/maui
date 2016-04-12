using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Views;
using AButton = Android.Widget.Button;
using AView = Android.Views.View;
using AndroidAnimation = Android.Animation;

namespace Xamarin.Forms.Platform.Android
{
	public class NavigationRenderer : VisualElementRenderer<NavigationPage>
	{
		static ViewPropertyAnimator s_currentAnimation;

		Page _current;

		public NavigationRenderer()
		{
			AutoPackage = false;
		}

		public Task<bool> PopToRootAsync(Page page, bool animated = true)
		{
			return OnPopToRootAsync(page, animated);
		}

		public Task<bool> PopViewAsync(Page page, bool animated = true)
		{
			return OnPopViewAsync(page, animated);
		}

		public Task<bool> PushViewAsync(Page page, bool animated = true)
		{
			return OnPushAsync(page, animated);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (VisualElement child in Element.InternalChildren)
				{
					IVisualElementRenderer renderer = Platform.GetRenderer(child);
					if (renderer != null)
						renderer.Dispose();
				}

				if (Element != null)
				{
					Element.PushRequested -= OnPushed;
					Element.PopRequested -= OnPopped;
					Element.PopToRootRequested -= OnPoppedToRoot;
					Element.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;
					Element.RemovePageRequested -= OnRemovePageRequested;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			Element.SendAppearing();
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			Element.SendDisappearing();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				NavigationPage oldNav = e.OldElement;
				oldNav.PushRequested -= OnPushed;
				oldNav.PopRequested -= OnPopped;
				oldNav.PopToRootRequested -= OnPoppedToRoot;
				oldNav.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;
				oldNav.RemovePageRequested -= OnRemovePageRequested;

				RemoveAllViews();
			}

			NavigationPage nav = e.NewElement;
			nav.PushRequested += OnPushed;
			nav.PopRequested += OnPopped;
			nav.PopToRootRequested += OnPoppedToRoot;
			nav.InsertPageBeforeRequested += OnInsertPageBeforeRequested;
			nav.RemovePageRequested += OnRemovePageRequested;

			// If there is already stuff on the stack we need to push it
			nav.StackCopy.Reverse().ForEach(p => PushViewAsync(p, false));
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);

			for (var i = 0; i < ChildCount; i++)
				GetChildAt(i).Layout(0, 0, r - l, b - t);
		}

		protected virtual Task<bool> OnPopToRootAsync(Page page, bool animated)
		{
			return SwitchContentAsync(page, animated, true);
		}

		protected virtual Task<bool> OnPopViewAsync(Page page, bool animated)
		{
			Page pageToShow = Element.StackCopy.Skip(1).FirstOrDefault();
			if (pageToShow == null)
				return Task.FromResult(false);

			return SwitchContentAsync(pageToShow, animated, true);
		}

		protected virtual Task<bool> OnPushAsync(Page view, bool animated)
		{
			return SwitchContentAsync(view, animated);
		}

		void InsertPageBefore(Page page, Page before)
		{
		}

		void OnInsertPageBeforeRequested(object sender, NavigationRequestedEventArgs e)
		{
			InsertPageBefore(e.Page, e.BeforePage);
		}

		void OnPopped(object sender, NavigationRequestedEventArgs e)
		{
			e.Task = PopViewAsync(e.Page, e.Animated);
		}

		void OnPoppedToRoot(object sender, NavigationRequestedEventArgs e)
		{
			e.Task = PopToRootAsync(e.Page, e.Animated);
		}

		void OnPushed(object sender, NavigationRequestedEventArgs e)
		{
			e.Task = PushViewAsync(e.Page, e.Animated);
		}

		void OnRemovePageRequested(object sender, NavigationRequestedEventArgs e)
		{
			RemovePage(e.Page);
		}

		void RemovePage(Page page)
		{
			IVisualElementRenderer rendererToRemove = Platform.GetRenderer(page);
			PageContainer containerToRemove = rendererToRemove == null ? null : (PageContainer)rendererToRemove.ViewGroup.Parent;

			containerToRemove.RemoveFromParent();

			if (rendererToRemove != null)
			{
				rendererToRemove.ViewGroup.RemoveFromParent();
				rendererToRemove.Dispose();
			}

			containerToRemove?.Dispose();

			Device.StartTimer(TimeSpan.FromMilliseconds(0), () =>
			{
				((Platform)Element.Platform).UpdateNavigationTitleBar();
				return false;
			});
		}

		Task<bool> SwitchContentAsync(Page view, bool animated, bool removed = false)
		{
			Context.HideKeyboard(this);

			IVisualElementRenderer rendererToAdd = Platform.GetRenderer(view);
			bool existing = rendererToAdd != null;
			if (!existing)
				Platform.SetRenderer(view, rendererToAdd = Platform.CreateRenderer(view));

			Page pageToRemove = _current;
			IVisualElementRenderer rendererToRemove = pageToRemove == null ? null : Platform.GetRenderer(pageToRemove);
			PageContainer containerToRemove = rendererToRemove == null ? null : (PageContainer)rendererToRemove.ViewGroup.Parent;
			PageContainer containerToAdd = (PageContainer)rendererToAdd.ViewGroup.Parent ?? new PageContainer(Context, rendererToAdd);

			containerToAdd.SetWindowBackground();

			_current = view;

			((Platform)Element.Platform).NavAnimationInProgress = true;

			var tcs = new TaskCompletionSource<bool>();

			if (animated)
			{
				if (s_currentAnimation != null)
					s_currentAnimation.Cancel();

				if (removed)
				{
					// animate out
					if (containerToAdd.Parent != this)
						AddView(containerToAdd, Element.LogicalChildren.IndexOf(rendererToAdd.Element));
					else
						((Page)rendererToAdd.Element).SendAppearing();
					containerToAdd.Visibility = ViewStates.Visible;

					if (containerToRemove != null)
					{
						Action<AndroidAnimation.Animator> done = a =>
						{
							containerToRemove.Visibility = ViewStates.Gone;
							containerToRemove.Alpha = 1;
							containerToRemove.ScaleX = 1;
							containerToRemove.ScaleY = 1;
							RemoveView(containerToRemove);

							tcs.TrySetResult(true);
							((Platform)Element.Platform).NavAnimationInProgress = false;

							VisualElement removedElement = rendererToRemove.Element;
							rendererToRemove.Dispose();
							if (removedElement != null)
								Platform.SetRenderer(removedElement, null);
						};

						// should always happen
						s_currentAnimation = containerToRemove.Animate().Alpha(0).ScaleX(0.8f).ScaleY(0.8f).SetDuration(250).SetListener(new GenericAnimatorListener { OnEnd = a =>
						{
							s_currentAnimation = null;
							done(a);
						},
							OnCancel = done });
					}
				}
				else
				{
					bool containerAlreadyAdded = containerToAdd.Parent == this;
					// animate in
					if (!containerAlreadyAdded)
						AddView(containerToAdd);
					else
						((Page)rendererToAdd.Element).SendAppearing();

					if (existing)
						Element.ForceLayout();

					containerToAdd.Alpha = 0;
					containerToAdd.ScaleX = containerToAdd.ScaleY = 0.8f;
					containerToAdd.Visibility = ViewStates.Visible;
					s_currentAnimation = containerToAdd.Animate().Alpha(1).ScaleX(1).ScaleY(1).SetDuration(250).SetListener(new GenericAnimatorListener { OnEnd = a =>
					{
						if (containerToRemove != null && containerToRemove.Handle != IntPtr.Zero)
						{
							containerToRemove.Visibility = ViewStates.Gone;
							if (pageToRemove != null)
								pageToRemove.SendDisappearing();
						}
						s_currentAnimation = null;
						tcs.TrySetResult(true);
						((Platform)Element.Platform).NavAnimationInProgress = false;
					} });
				}
			}
			else
			{
				// just do it fast
				if (containerToRemove != null)
				{
					if (removed)
						RemoveView(containerToRemove);
					else
						containerToRemove.Visibility = ViewStates.Gone;
				}

				if (containerToAdd.Parent != this)
					AddView(containerToAdd);
				else
					((Page)rendererToAdd.Element).SendAppearing();

				if (containerToRemove != null && !removed)
					pageToRemove.SendDisappearing();

				if (existing)
					Element.ForceLayout();

				containerToAdd.Visibility = ViewStates.Visible;
				tcs.SetResult(true);
				((Platform)Element.Platform).NavAnimationInProgress = false;
			}

			return tcs.Task;
		}
	}
}