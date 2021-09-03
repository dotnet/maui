using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public class Platform : BindableObject, IPlatformLayout, INavigation
	{
		readonly Context _context;
		readonly PlatformRenderer _renderer;
		bool _disposed;
		bool _navAnimationInProgress;
		NavigationModel _navModel = new NavigationModel();
		NavigationModel _previousNavModel = null;
		readonly bool _embedded;

		internal static string PackageName { get; private set; }
		internal static string GetPackageName() => PackageName;

		internal const string CloseContextActionsSignalName = "Xamarin.CloseContextActions";

		internal static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer", typeof(IVisualElementRenderer), typeof(Platform), default(IVisualElementRenderer),
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var view = bindable as VisualElement;
				if (view != null)
					view.IsPlatformEnabled = newvalue != null;

				if (bindable is IView mauiView)
				{
					if (mauiView.Handler == null && newvalue is IVisualElementRenderer ver)
						mauiView.Handler = new RendererToHandlerShim(ver);
				}

			});

		internal Platform(Context context) : this(context, false)
		{
		}

		internal Platform(Context context, bool embedded)
		{
			_embedded = embedded;
			_context = context;
			PackageName = context?.PackageName;
			_renderer = new PlatformRenderer(context, this);
			var activity = _context.GetActivity();

			if (embedded && activity != null)
			{
				// Set up handling of DisplayAlert/DisplayActionSheet/UpdateProgressBarVisibility
				if (_context == null)
				{
					// Can't show dialogs if it's not an activity
					return;
				}

				PopupManager.Subscribe(_context.GetActivity());
				return;
			}

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
					MessagingCenter.Send(this, CloseContextActionsSignalName);
			}
		}

		Page Page { get; set; }

		IPageController CurrentPageController => _navModel.CurrentPage;

		internal void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;

			FormsAppCompatActivity.BackPressed -= HandleBackPressed;

			SetPage(null);

			var activity = _context?.GetActivity();
			if (_embedded && activity != null)
			{
				PopupManager.Unsubscribe(activity);
			}
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

			IVisualElementRenderer modalRenderer = GetRenderer(modal);
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

		public static SizeRequest GetNativeSize(
			IVisualElementRenderer visualElementRenderer,
			double widthConstraint,
			double heightConstraint)
		{
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

			return result;
		}

		internal static SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			Performance.Start(out string reference);

			IVisualElementRenderer visualElementRenderer = GetRenderer(view);
			SizeRequest returnValue;

			if (visualElementRenderer != null && !visualElementRenderer.View.IsAlive())
			{
				returnValue = new SizeRequest(Size.Zero, Size.Zero);
			}
			else if ((visualElementRenderer == null || visualElementRenderer is HandlerToRendererShim) && view is IView iView)
			{
				returnValue = iView.Handler.GetDesiredSize(widthConstraint, heightConstraint);
			}
			else if (visualElementRenderer != null)
			{
				returnValue = GetNativeSize(visualElementRenderer, widthConstraint, heightConstraint);
			}
			else
			{
				returnValue = new SizeRequest(Size.Zero, Size.Zero);
			}

			Performance.Stop(reference);

			return returnValue;
		}

		public static void ClearRenderer(AView renderedView)
		{
			var element = (renderedView as IVisualElementRenderer)?.Element;
			var view = element as View;
			if (view != null)
			{
				var renderer = GetRenderer(view);
				if (renderer == renderedView)
					element.ClearValue(RendererProperty);
				renderer?.Dispose();
				renderer = null;
			}
			var layout = view as IVisualElementRenderer;
			layout?.Dispose();
			layout = null;
		}

		internal static IVisualElementRenderer CreateRenderer(
			VisualElement element, 
			Context context,
			AndroidX.Fragment.App.FragmentManager fragmentManager = null,
			global::Android.Views.LayoutInflater layoutInflater = null)
		{
			IVisualElementRenderer renderer = null;

			// temporary hack to fix the following issues
			// https://github.com/xamarin/Microsoft.Maui.Controls.Compatibility/issues/13261
			// https://github.com/xamarin/Microsoft.Maui.Controls.Compatibility/issues/12484
			if (element is RadioButton tv && tv.ResolveControlTemplate() != null)
			{
				renderer = new DefaultRenderer(context);
			}

			// This code is duplicated across all platforms currently
			// So if any changes are made here please make sure to apply them to other platform.cs files
			if (renderer == null)
			{
				IViewHandler handler = null;

				//TODO: Handle this with AppBuilderHost
				try
				{
					var mauiContext = Forms.MauiContext;

					if (fragmentManager != null || layoutInflater != null)
						mauiContext = new ScopedMauiContext(mauiContext, null, null, layoutInflater, fragmentManager);

					handler = mauiContext.Handlers.GetHandler(element.GetType()) as IViewHandler;
					handler.SetMauiContext(mauiContext);
				}
				catch
				{
					// TODO define better catch response or define if this is needed?
				}

				if (handler == null)
				{
					renderer = Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(element, context)
										?? new DefaultRenderer(context);
				}
				// This means the only thing registered is the RendererToHandlerShim
				// Which is only used when you are running a .NET MAUI app
				// This indicates that the user hasn't registered a specific handler for this given type
				else if (handler is RendererToHandlerShim shim)
				{
					renderer = shim.VisualElementRenderer;

					if (renderer == null)
					{
						renderer = Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(element, context)
										?? new DefaultRenderer(context);
					}
				}
				else if (handler is IVisualElementRenderer ver)
					renderer = ver;
				else if (handler is INativeViewHandler vh)
				{
					renderer = new HandlerToRendererShim(vh);
					element.Handler = handler;
					SetRenderer(element, renderer);
				}
			}

			renderer.SetElement(element);

			if (fragmentManager != null)
			{
				var managesFragments = renderer as IManageFragments;
				managesFragments?.SetFragmentManager(fragmentManager);
			}

			return renderer;
		}

		public static IVisualElementRenderer CreateRendererWithContext(VisualElement element, Context context)
		{
			// This is an interim method to allow public access to CreateRenderer(element, context), which we 
			// can't make public yet because it will break the previewer
			return CreateRenderer(element, context);
		}

		public static IVisualElementRenderer GetRenderer(VisualElement bindable)
		{
			return (IVisualElementRenderer)bindable?.GetValue(RendererProperty);
		}

		public static void SetRenderer(VisualElement bindable, IVisualElementRenderer value)
		{
			bindable.SetValue(RendererProperty, value);
		}

		internal ViewGroup GetViewGroup()
		{
			return _renderer;
		}

		void IPlatformLayout.OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (Page == null)
				return;

			if (changed)
			{
				LayoutRootPage(Page, r - l, b - t);
			}

			GetRenderer(Page)?.UpdateLayout();

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
					if (GetRenderer(rootPage) is ILifeCycleState nr)
						nr.MarkedForDispose = true;
				}

				var viewsToRemove = new List<AView>();
				var renderersToDispose = new List<IVisualElementRenderer>();

				for (int i = 0; i < _renderer.ChildCount; i++)
					viewsToRemove.Add(_renderer.GetChildAt(i));

				foreach (var root in navModel.Roots)
					renderersToDispose.Add(GetRenderer(root));

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

			var pageRenderer = GetRenderer(page);
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
					rootRenderer?.Element.ClearValue(RendererProperty);
					rootRenderer?.Dispose();
				}
			}
		}

		void AddChild(Page page, bool layout = false)
		{
			if (page == null)
				return;

			if (GetRenderer(page) != null)
				return;

			IVisualElementRenderer renderView = CreateRenderer(page, _context);
			SetRenderer(page, renderView);

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

				_renderer = CreateRenderer(modal, context);
				SetRenderer(modal, _renderer);

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
						_modal.ClearValue(RendererProperty);
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
				if (modalBkgndColor == null)
					_backgroundView.SetWindowBackground();
				else
					_backgroundView.SetBackgroundColor(modalBkgndColor.ToAndroid());
			}
		}

		internal static int GenerateViewId()
		{
			// getting unique Id's is an art, and I consider myself the Jackson Pollock of the field
			if ((int)Forms.SdkInt >= 17)
				return global::Android.Views.View.GenerateViewId();

			// Numbers higher than this range reserved for xml
			// If we roll over, it can be exceptionally problematic for the user if they are still retaining things, android's internal implementation is
			// basically identical to this except they do a lot of locking we don't have to because we know we only do this
			// from the UI thread
			if (s_id >= 0x00ffffff)
				s_id = 0x00000400;

			return s_id++;
		}

		static int s_id = 0x00000400;

		#region Statics

		public static implicit operator ViewGroup(Platform canvas)
		{
			return canvas._renderer;
		}

		#endregion

		internal class DefaultRenderer : VisualElementRenderer<View>, ILayoutChanges
		{
			public bool NotReallyHandled { get; private set; }

			IOnTouchListener _touchListener;
			bool _disposed;
			bool _hasLayoutOccurred;

			readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();

			public DefaultRenderer(Context context) : base(context)
			{
				ChildrenDrawingOrderEnabled = true;
			}

			internal void NotifyFakeHandling()
			{
				NotReallyHandled = true;
			}

			public override bool OnTouchEvent(MotionEvent e)
			{
				if (base.OnTouchEvent(e))
					return true;

				return _motionEventHelper.HandleMotionEvent(Parent, e);
			}

			protected override void OnElementChanged(ElementChangedEventArgs<View> e)
			{
				base.OnElementChanged(e);

				_motionEventHelper.UpdateElement(e.NewElement);
			}

			public override bool DispatchTouchEvent(MotionEvent e)
			{
				#region Excessive explanation
				// Normally dispatchTouchEvent feeds the touch events to its children one at a time, top child first,
				// (and only to the children in the hit-test area of the event) stopping as soon as one of them has handled
				// the event. 

				// But to be consistent across the platforms, we don't want this behavior; if an element is not input transparent
				// we don't want an event to "pass through it" and be handled by an element "behind/under" it. We just want the processing
				// to end after the first non-transparent child, regardless of whether the event has been handled.

				// This is only an issue for a couple of controls; the interactive controls (switch, button, slider, etc) already "handle" their touches 
				// and the events don't propagate to other child controls. But for image, label, and box that doesn't happen. We can't have those controls 
				// lie about their events being handled because then the events won't propagate to *parent* controls (e.g., a frame with a label in it would
				// never get a tap gesture from the label). In other words, we *want* parent propagation, but *do not want* sibling propagation. So we need to short-circuit 
				// base.DispatchTouchEvent here, but still return "false".

				// Duplicating the logic of ViewGroup.dispatchTouchEvent and modifying it slightly for our purposes is a non-starter; the method is too
				// complex and does a lot of micro-optimization. Instead, we provide a signalling mechanism for the controls which don't already "handle" touch
				// events to tell us that they will be lying about handling their event; they then return "true" to short-circuit base.DispatchTouchEvent.

				// The container gets this message and after it gets the "handled" result from dispatchTouchEvent, 
				// it then knows to ignore that result and return false/unhandled. This allows the event to propagate up the tree.
				#endregion

				NotReallyHandled = false;

				var result = base.DispatchTouchEvent(e);

				if (result && NotReallyHandled)
				{
					// If the child control returned true from its touch event handler but signalled that it was a fake "true", then we
					// don't consider the event truly "handled" yet. 
					// Since a child control short-circuited the normal dispatchTouchEvent stuff, this layout never got the chance for
					// IOnTouchListener.OnTouch and the OnTouchEvent override to try handling the touches; we'll do that now
					// Any associated Touch Listeners are called from DispatchTouchEvents if all children of this view return false
					// So here we are simulating both calls that would have typically been called from inside DispatchTouchEvent
					// but were not called due to the fake "true"
					result = _touchListener?.OnTouch(this, e) ?? false;
					return result || OnTouchEvent(e);
				}

				return result;
			}

			public override void SetOnTouchListener(IOnTouchListener l)
			{
				_touchListener = l;
				base.SetOnTouchListener(l);
			}

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;

				if (disposing)
					SetOnTouchListener(null);

				base.Dispose(disposing);
			}

			bool ILayoutChanges.HasLayoutOccurred => _hasLayoutOccurred;

			protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
			{
				base.OnLayout(changed, left, top, right, bottom);
				_hasLayoutOccurred = true;
			}
		}

		internal static string ResolveMsAppDataUri(Uri uri)
		{
			if (uri.Scheme == "ms-appdata")
			{
				string filePath = string.Empty;

				if (uri.LocalPath.StartsWith("/local"))
				{
					filePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), uri.LocalPath.Substring(7));
				}
				else if (uri.LocalPath.StartsWith("/temp"))
				{
					filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), uri.LocalPath.Substring(6));
				}
				else
				{
					throw new ArgumentException("Invalid Uri", "Source");
				}

				return filePath;
			}
			else
			{
				throw new ArgumentException("uri");
			}

		}
	}
}
