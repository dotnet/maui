using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Xamarin.Forms.Internals;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	internal class Platform : BindableObject, IPlatformLayout, INavigation, IDisposable
#pragma warning disable CS0618
		, IPlatform
#pragma warning restore
	{
		readonly Context _context;
		readonly PlatformRenderer _renderer;
		bool _disposed;
		bool _navAnimationInProgress;
		NavigationModel _navModel = new NavigationModel();
		NavigationModel _previousNavModel = null;

		internal static string PackageName { get; private set; }
		internal static string GetPackageName() => PackageName ?? Android.Platform.PackageName;

		public Platform(Context context)
		{
			_context = context;
			PackageName = context?.PackageName;
			_renderer = new PlatformRenderer(context, this);

			FormsAppCompatActivity.BackPressed += HandleBackPressed;
		}

		internal bool NavAnimationInProgress
		{
			get { return _navAnimationInProgress; }
			set
			{
				if (_navAnimationInProgress == value)
					return;
				_navAnimationInProgress = value;
				if (value)
					MessagingCenter.Send(this, Android.Platform.CloseContextActionsSignalName);
			}
		}

		Page Page { get; set; }

		IPageController CurrentPageController => _navModel.CurrentPage as IPageController;

		public void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;

			FormsAppCompatActivity.BackPressed -= HandleBackPressed;

			SetPage(null);
		}

		void INavigation.InsertPageBefore(Page page, Page before)
		{
			throw new InvalidOperationException("InsertPageBefore is not supported globally on Android, please use a NavigationPage.");
		}

		IReadOnlyList<Page> INavigation.ModalStack => _navModel.Modals.ToList();

		IReadOnlyList<Page> INavigation.NavigationStack => new List<Page>();

		Task<Page> INavigation.PopAsync()
		{
			return ((INavigation)this).PopAsync(true);
		}

		Task<Page> INavigation.PopAsync(bool animated)
		{
			throw new InvalidOperationException("PopAsync is not supported globally on Android, please use a NavigationPage.");
		}

		Task<Page> INavigation.PopModalAsync()
		{
			return ((INavigation)this).PopModalAsync(true);
		}

		Task<Page> INavigation.PopModalAsync(bool animated)
		{
			Page modal = _navModel.PopModal();
			((IPageController)modal).SendDisappearing();
			var source = new TaskCompletionSource<Page>();

			IVisualElementRenderer modalRenderer = Android.Platform.GetRenderer(modal);
			if (modalRenderer != null)
			{
				var modalContainer = modalRenderer.View.Parent as ModalContainer;
				if (animated)
				{
					modalContainer.Animate().TranslationY(_renderer.Height).SetInterpolator(new AccelerateInterpolator(1)).SetDuration(300).SetListener(new GenericAnimatorListener
					{
						OnEnd = a =>
						{
							modalContainer.RemoveFromParent();
							modalContainer.Dispose();
							source.TrySetResult(modal);
							CurrentPageController?.SendAppearing();
							modalContainer = null;
						}
					});
				}
				else
				{
					modalContainer.RemoveFromParent();
					modalContainer.Dispose();
					source.TrySetResult(modal);
					CurrentPageController?.SendAppearing();
				}
			}

			UpdateAccessibilityImportance(CurrentPageController as Page, ImportantForAccessibility.Auto, true);

			return source.Task;
		}

		Task INavigation.PopToRootAsync()
		{
			return ((INavigation)this).PopToRootAsync(true);
		}

		Task INavigation.PopToRootAsync(bool animated)
		{
			throw new InvalidOperationException("PopToRootAsync is not supported globally on Android, please use a NavigationPage.");
		}

		Task INavigation.PushAsync(Page root)
		{
			return ((INavigation)this).PushAsync(root, true);
		}

		Task INavigation.PushAsync(Page root, bool animated)
		{
			throw new InvalidOperationException("PushAsync is not supported globally on Android, please use a NavigationPage.");
		}

		Task INavigation.PushModalAsync(Page modal)
		{
			return ((INavigation)this).PushModalAsync(modal, true);
		}

		async Task INavigation.PushModalAsync(Page modal, bool animated)
		{
			CurrentPageController?.SendDisappearing();
			UpdateAccessibilityImportance(CurrentPageController as Page, ImportantForAccessibility.NoHideDescendants, false);

			_navModel.PushModal(modal);

#pragma warning disable CS0618 // Type or member is obsolete
			// The Platform property is no longer necessary, but we have to set it because some third-party
			// library might still be retrieving it and using it
			modal.Platform = this;
#pragma warning restore CS0618 // Type or member is obsolete

			Task presentModal = PresentModal(modal, animated);

			await presentModal;

			UpdateAccessibilityImportance(modal, ImportantForAccessibility.Auto, true);

			// Verify that the modal is still on the stack
			if (_navModel.CurrentPage == modal)
				((IPageController)modal).SendAppearing();
		}

		void INavigation.RemovePage(Page page)
		{
			throw new InvalidOperationException("RemovePage is not supported globally on Android, please use a NavigationPage.");
		}

		public static SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			Performance.Start(out string reference);

			// FIXME: potential crash
			IVisualElementRenderer visualElementRenderer = Android.Platform.GetRenderer(view);

			var context = visualElementRenderer.View.Context;

			// negative numbers have special meanings to android they don't to us
			widthConstraint = widthConstraint <= -1 ? double.PositiveInfinity : context.ToPixels(widthConstraint);
			heightConstraint = heightConstraint <= -1 ? double.PositiveInfinity : context.ToPixels(heightConstraint);

			bool widthConstrained = !double.IsPositiveInfinity(widthConstraint);
			bool heightConstrained = !double.IsPositiveInfinity(heightConstraint);

			int widthMeasureSpec = widthConstrained
							? MeasureSpecFactory.MakeMeasureSpec((int)widthConstraint, MeasureSpecMode.AtMost)
							: MeasureSpecFactory.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);

			int heightMeasureSpec = heightConstrained
							 ? MeasureSpecFactory.MakeMeasureSpec((int)heightConstraint, MeasureSpecMode.AtMost)
							 : MeasureSpecFactory.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);

			SizeRequest rawResult = visualElementRenderer.GetDesiredSize(widthMeasureSpec, heightMeasureSpec);
			if (rawResult.Minimum == Size.Zero)
				rawResult.Minimum = rawResult.Request;
			var result = new SizeRequest(new Size(context.FromPixels(rawResult.Request.Width), context.FromPixels(rawResult.Request.Height)),
				new Size(context.FromPixels(rawResult.Minimum.Width), context.FromPixels(rawResult.Minimum.Height)));

			if ((widthConstrained && result.Request.Width < widthConstraint)
				|| (heightConstrained && result.Request.Height < heightConstraint))
			{
				// Do a final exact measurement in case the native control needs to fill the container
				(visualElementRenderer as IViewRenderer)?.MeasureExactly();
			}

			Performance.Stop(reference);

			return result;
		}

		void IPlatformLayout.OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (Page == null)
				return;

			if (changed)
			{
				LayoutRootPage(Page, r - l, b - t);
			}

			Android.Platform.GetRenderer(Page).UpdateLayout();

			for (var i = 0; i < _renderer.ChildCount; i++)
			{
				AView child = _renderer.GetChildAt(i);
				if (child is ModalContainer)
				{
					child.Measure(MeasureSpecFactory.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly), MeasureSpecFactory.MakeMeasureSpec(t - b, MeasureSpecMode.Exactly));
					child.Layout(l, t, r, b);
				}
			}
		}

		protected override void OnBindingContextChanged()
		{
			SetInheritedBindingContext(Page, BindingContext);

			base.OnBindingContextChanged();
		}

		internal void SettingNewPage()
		{
			if (Page != null)
			{
				_previousNavModel = _navModel;
				_navModel = new NavigationModel();
			}
		}

		internal void SetPage(Page newRoot)
		{
			if (Page == newRoot)
			{
				return;
			}

			if (Page != null)
			{
				var navModel = (_previousNavModel ?? _navModel);
				foreach (var rootPage in navModel.Roots)
				{
					if (Android.Platform.GetRenderer(rootPage) is ILifeCycleState nr)
						nr.MarkedForDispose = true;
				}

				var viewsToRemove = new List<AView>();
				var renderersToDispose = new List<IVisualElementRenderer>();

				for (int i = 0; i < _renderer.ChildCount; i++)
					viewsToRemove.Add(_renderer.GetChildAt(i));

				foreach (var root in navModel.Roots)
					renderersToDispose.Add(Android.Platform.GetRenderer(root));

				SetPageInternal(newRoot);

				Cleanup(viewsToRemove, renderersToDispose);
			}
			else
			{
				SetPageInternal(newRoot);
			}
		}

		void UpdateAccessibilityImportance(Page page, ImportantForAccessibility importantForAccessibility, bool forceFocus)
		{

			var pageRenderer = Android.Platform.GetRenderer(page);
			if (pageRenderer?.View == null)
				return;
			pageRenderer.View.ImportantForAccessibility = importantForAccessibility;
			if (forceFocus)
				pageRenderer.View.SendAccessibilityEvent(global::Android.Views.Accessibility.EventTypes.ViewFocused);

		}

		void SetPageInternal(Page newRoot)
		{
			var layout = false;

			if (Page != null)
			{
				// if _previousNavModel has been set than _navModel has already been reinitialized
				if (_previousNavModel != null)
				{
					_previousNavModel = null;
					if (_navModel == null)
						_navModel = new NavigationModel();
				}
				else
					_navModel = new NavigationModel();

				layout = true;
			}

			if (newRoot == null)
			{
				Page = null;

				return;
			}

			_navModel.Push(newRoot, null);

			Page = newRoot;

			AddChild(Page, layout);

			Application.Current.NavigationProxy.Inner = this;
		}

		void Cleanup(List<AView> viewsToRemove, List<IVisualElementRenderer> renderersToDispose)
		{
			// If trigger by dispose, cleanup now, otherwise queue it for later
			if (_disposed)
			{
				DoCleanup();
			}
			else
			{
				new Handler(Looper.MainLooper).Post(DoCleanup);
			}

			void DoCleanup()
			{
				for (int i = 0; i < viewsToRemove.Count; i++)
				{
					AView view = viewsToRemove[i];
					_renderer?.RemoveView(view);
				}

				for (int i = 0; i < renderersToDispose.Count; i++)
				{
					IVisualElementRenderer rootRenderer = renderersToDispose[i];
					rootRenderer?.Element.ClearValue(Android.Platform.RendererProperty);
					rootRenderer?.Dispose();
				}
			}
		}

		void AddChild(Page page, bool layout = false)
		{
			if (page == null)
				return;

			if (Android.Platform.GetRenderer(page) != null)
				return;

			IVisualElementRenderer renderView = Android.Platform.CreateRenderer(page, _context);
			Android.Platform.SetRenderer(page, renderView);

			if (layout)
				LayoutRootPage(page, _renderer.Width, _renderer.Height);

			_renderer.AddView(renderView.View);
		}

		bool HandleBackPressed(object sender, EventArgs e)
		{
			if (NavAnimationInProgress)
				return true;

			Page root = _navModel.Roots.LastOrDefault();
			bool handled = root?.SendBackButtonPressed() ?? false;

			return handled;
		}

		void LayoutRootPage(Page page, int width, int height)
		{
			page.Layout(new Rectangle(0, 0, _context.FromPixels(width), _context.FromPixels(height)));
		}

		Task PresentModal(Page modal, bool animated)
		{
			var modalContainer = new ModalContainer(_context, modal);

			_renderer.AddView(modalContainer);

			var source = new TaskCompletionSource<bool>();
			NavAnimationInProgress = true;
			if (animated)
			{
				modalContainer.TranslationY = _renderer.Height;
				modalContainer.Animate().TranslationY(0).SetInterpolator(new DecelerateInterpolator(1)).SetDuration(300).SetListener(new GenericAnimatorListener
				{
					OnEnd = a =>
					{
						source.TrySetResult(false);
						modalContainer = null;
					},
					OnCancel = a =>
					{
						source.TrySetResult(true);
						modalContainer = null;
					}
				});
			}
			else
			{
				source.TrySetResult(true);
			}

			return source.Task.ContinueWith(task => NavAnimationInProgress = false);
		}

		sealed class ModalContainer : ViewGroup
		{
			AView _backgroundView;
			bool _disposed;
			Page _modal;
			IVisualElementRenderer _renderer;

			public ModalContainer(Context context, Page modal) : base(context)
			{
				_modal = modal;

				_backgroundView = new AView(context);
				UpdateBackgroundColor();
				AddView(_backgroundView);

				_renderer = Android.Platform.CreateRenderer(modal, context);
				Android.Platform.SetRenderer(modal, _renderer);

				AddView(_renderer.View);

				Id = Platform.GenerateViewId();

				_modal.PropertyChanged += OnModalPagePropertyChanged;
			}

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				if (disposing)
				{
					RemoveAllViews();
					if (_renderer != null)
					{
						_renderer.Dispose();
						_renderer = null;
						_modal.ClearValue(Android.Platform.RendererProperty);
						_modal.PropertyChanged -= OnModalPagePropertyChanged;
						_modal = null;
					}

					if (_backgroundView != null)
					{
						_backgroundView.Dispose();
						_backgroundView = null;
					}
				}

				_disposed = true;
				base.Dispose(disposing);
			}

			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				if (changed)
				{
					_modal.Layout(new Rectangle(0, 0, Context.FromPixels(r - l), Context.FromPixels(b - t)));
					_backgroundView.Layout(0, 0, r - l, b - t);
				}

				_renderer.UpdateLayout();
			}

			void OnModalPagePropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == Page.BackgroundColorProperty.PropertyName)
					UpdateBackgroundColor();
			}

			void UpdateBackgroundColor()
			{
				Color modalBkgndColor = _modal.BackgroundColor;
				if (modalBkgndColor.IsDefault)
					_backgroundView.SetWindowBackground();
				else
					_backgroundView.SetBackgroundColor(modalBkgndColor.ToAndroid());
			}
		}

		internal static int GenerateViewId()
		{
			return Android.Platform.GenerateViewId();
		}

		#region Statics

		public static implicit operator ViewGroup(Platform canvas)
		{
			return canvas._renderer;
		}

		#endregion

		#region Obsolete 

		SizeRequest IPlatform.GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return GetNativeSize(view, widthConstraint, heightConstraint);
		}

		#endregion
	}
}