using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Xamarin.Forms.Internals;
using AndroidAnimation = Android.Animation;

namespace Xamarin.Forms.Platform.Android
{
	public class NavigationRenderer : VisualElementRenderer<NavigationPage>
	{
		static ViewPropertyAnimator s_currentAnimation;

		Page _current;
		bool _disposed;
		Platform _platform;

		public NavigationRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use NavigationRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public NavigationRenderer()
		{
			AutoPackage = false;
		}

		Platform Platform
		{
			get
			{
				if (_platform == null)
				{
					if (Context is FormsApplicationActivity activity)
					{
						_platform = activity.Platform;
					}
				}

				return _platform;
			}
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

		IPageController PageController => Element;

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;

				if (Element != null)
				{
					foreach (Element element in PageController.InternalChildren)
					{
						var child = (VisualElement)element;
						if (child == null)
						{
							continue;
						}

						IVisualElementRenderer renderer = Platform.GetRenderer(child);
						renderer?.Dispose();
					}

					var navController = (INavigationPageController)Element;

					navController.PushRequested -= OnPushed;
					navController.PopRequested -= OnPopped;
					navController.PopToRootRequested -= OnPoppedToRoot;
					navController.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;
					navController.RemovePageRequested -= OnRemovePageRequested;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			PageController?.SendAppearing();
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			PageController?.SendDisappearing();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				var oldNavController = (INavigationPageController)e.OldElement;

				oldNavController.PushRequested -= OnPushed;
				oldNavController.PopRequested -= OnPopped;
				oldNavController.PopToRootRequested -= OnPoppedToRoot;
				oldNavController.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;
				oldNavController.RemovePageRequested -= OnRemovePageRequested;

				RemoveAllViews();
			}

			var newNavController = (INavigationPageController)e.NewElement;

			newNavController.PushRequested += OnPushed;
			newNavController.PopRequested += OnPopped;
			newNavController.PopToRootRequested += OnPoppedToRoot;
			newNavController.InsertPageBeforeRequested += OnInsertPageBeforeRequested;
			newNavController.RemovePageRequested += OnRemovePageRequested;

			// If there is already stuff on the stack we need to push it
			foreach (Page page in newNavController.Pages)
			{
				PushViewAsync(page, false);
			}
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
			Page pageToShow = ((INavigationPageController)Element).Peek(1);
			if (pageToShow == null)
				return Task.FromResult(false);

			return SwitchContentAsync(pageToShow, animated, true);
		}

		protected virtual Task<bool> OnPushAsync(Page view, bool animated)
		{
			return SwitchContentAsync(view, animated);
		}

		void UpdateActionBar()
		{
			Device.StartTimer(TimeSpan.FromMilliseconds(0), () =>
			{
				Platform?.UpdateActionBar();
				return false;
			});
		}

		void InsertPageBefore(Page page, Page before)
		{
			int index = PageController.InternalChildren.IndexOf(before);
			if (index == -1)
				throw new InvalidOperationException("This should never happen, please file a bug");

			UpdateActionBar();
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
			PageContainer containerToRemove = rendererToRemove == null ? null : (PageContainer)rendererToRemove.View.Parent;

			containerToRemove.RemoveFromParent();

			if (rendererToRemove != null)
			{
				rendererToRemove.View.RemoveFromParent();
				rendererToRemove.Dispose();
			}

			containerToRemove?.Dispose();

			UpdateActionBar();
		}

		Task<bool> SwitchContentAsync(Page view, bool animated, bool removed = false)
		{
			Context.HideKeyboard(this);

			IVisualElementRenderer rendererToAdd = Platform.GetRenderer(view);
			bool existing = rendererToAdd != null;
			if (!existing)
				Platform.SetRenderer(view, rendererToAdd = Platform.CreateRenderer(view, Context));

			Page pageToRemove = _current;
			IVisualElementRenderer rendererToRemove = pageToRemove == null ? null : Platform.GetRenderer(pageToRemove);
			PageContainer containerToRemove = rendererToRemove == null ? null : (PageContainer)rendererToRemove.View.Parent;
			PageContainer containerToAdd = (PageContainer)rendererToAdd.View.Parent ?? new PageContainer(Context, rendererToAdd);

			containerToAdd.SetWindowBackground();

			_current = view;

			if (Platform != null)
			{
				Platform.NavAnimationInProgress = true;
			}

			var tcs = new TaskCompletionSource<bool>();

			if (animated)
			{
				if (s_currentAnimation != null)
					s_currentAnimation.Cancel();

				if (removed)
				{
					// animate out
					if (containerToAdd.Parent != this)
					{
						var indexRenderToAdd = Math.Min(ChildCount, ((IElementController)Element).LogicalChildren.IndexOf(rendererToAdd.Element));
						AddView(containerToAdd, indexRenderToAdd);
					}
					else
						((IPageController)rendererToAdd.Element).SendAppearing();
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

							VisualElement removedElement = rendererToRemove.Element;
							rendererToRemove.Dispose();
							if (removedElement != null)
								Platform.SetRenderer(removedElement, null);
						};

						// should always happen
						s_currentAnimation = containerToRemove.Animate().Alpha(0).ScaleX(0.8f).ScaleY(0.8f).SetDuration(250).SetListener(new GenericAnimatorListener
						{
							OnEnd = a =>
{
s_currentAnimation = null;
done(a);
},
							OnCancel = done
						});
					}
				}
				else
				{
					bool containerAlreadyAdded = containerToAdd.Parent == this;
					// animate in
					if (!containerAlreadyAdded)
						AddView(containerToAdd);
					else
						((IPageController)rendererToAdd.Element).SendAppearing();

					if (existing)
						Element.ForceLayout();

					containerToAdd.Alpha = 0;
					containerToAdd.ScaleX = containerToAdd.ScaleY = 0.8f;
					containerToAdd.Visibility = ViewStates.Visible;
					s_currentAnimation = containerToAdd.Animate().Alpha(1).ScaleX(1).ScaleY(1).SetDuration(250).SetListener(new GenericAnimatorListener
					{
						OnEnd = a =>
{
if (containerToRemove != null && containerToRemove.Handle != IntPtr.Zero)
{
containerToRemove.Visibility = ViewStates.Gone;
((IPageController)pageToRemove)?.SendDisappearing();
}
s_currentAnimation = null;
tcs.TrySetResult(true);
}
					});
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
					((IPageController)rendererToAdd.Element).SendAppearing();

				if (containerToRemove != null && !removed)
					((IPageController)pageToRemove).SendDisappearing();

				if (existing)
					Element.ForceLayout();

				containerToAdd.Visibility = ViewStates.Visible;
				tcs.SetResult(true);
			}

			return tcs.Task.ContinueWith(task =>
			{
				if (Platform != null)
				{
					Platform.NavAnimationInProgress = false;
				}

				return task.Result;
			});
		}
	}
}