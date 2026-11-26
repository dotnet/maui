#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Activity;
using AndroidX.Core.View;
using Google.Android.Material.BottomSheet;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		/// <summary>
		/// Hosts a modal page presented as a <see cref="BottomSheet"/> in a Material <see cref="BottomSheetDialog"/>.
		/// </summary>
		internal sealed class BottomSheetModalFragment : BottomSheetDialogFragment
		{
			// Device-independent units within which two detents are the same resting position.
			const double Tolerance = 1;

			Page? _modal;
			Window? _window;
			IMauiContext? _mauiWindowContext;
			NavigationRootManager? _navigationRootManager;
			BottomSheetBehavior? _behavior;
			SheetCallback? _sheetCallback;
			AView? _parent;
			LinearLayout? _container;
			BottomSheetDragHandleView? _dragHandle;
			readonly bool _animated = true;

			List<BottomSheetStop> _stops = new();
			int _parentHeight;
			int _parentWidth;
			double _contentHeight = -1;
			double _measuredWidth = -1;
			double _measureWidth = -1;
			double _measureInset;
			global::Android.Content.Res.ColorStateList? _themeFillColor;
			Google.Android.Material.Shape.ShapeAppearanceModel? _themeShape;
			bool _applyingConfiguration;
			bool _contentRemeasurePending;

			// The state set programmatically that the sheet is settling into; reaching it is not a user change.
			int _pendingProgrammaticState = -1;
			bool _updatingFromPlatform;
			bool _backProgressActive;
			bool _dismissRequested;
			bool _cancelledByUser;
			bool _pendingPresentation = true;
			IElementHandler? _connectedHandler;

			readonly TaskCompletionSource _presented = new(TaskCreationOptions.RunContinuationsAsynchronously);
			readonly TaskCompletionSource _dismissed = new(TaskCreationOptions.RunContinuationsAsynchronously);

			// Used by Android when it recreates fragments; MAUI doesn't restore modal pages, so the fragment removes itself.
			public BottomSheetModalFragment()
			{
			}

			public BottomSheetModalFragment(IMauiContext mauiContext, Page modal, bool animated)
			{
				_modal = modal;
				_window = modal.Window;
				_mauiWindowContext = mauiContext;
				_animated = animated;

				// The sheet handles dismissal itself (see OnStart), so the dialog never cancels on its own.
				Cancelable = false;

				modal.Loaded += OnModalLoaded;
				modal.PropertyChanged += OnModalPropertyChanged;
				modal.MeasureInvalidated += OnModalMeasureInvalidated;
			}

			/// <summary>Completes once the sheet is on screen and its page is loaded.</summary>
			internal Task PresentationTask => _presented.Task;

			bool IsDismissed => _dismissed.Task.IsCompleted;

			public override void OnCreate(Bundle? savedInstanceState)
			{
				base.OnCreate(savedInstanceState);

				if (_modal is null)
					DismissAllowingStateLoss();
			}

			public override global::Android.App.Dialog OnCreateDialog(Bundle? savedInstanceState)
			{
				var dialog = new MauiBottomSheetDialog(RequireContext(), Theme, this)
				{
					DismissWithAnimation = true
				};

				if (!_animated)
					dialog.Window?.SetWindowAnimations(0);

				// Lets the sheet move above the keyboard instead of the keyboard covering it.
				dialog.Window?.SetSoftInputMode(SoftInput.AdjustResize);

				return dialog;
			}

			public override AView? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
			{
				if (_modal is null || _mauiWindowContext is null)
					return null;

				var modalContext = _mauiWindowContext
					.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager, registerNewNavigationRoot: true);

				_navigationRootManager = modalContext.GetNavigationRootManager();
				if (!_navigationRootManager.Connect(_modal, modalContext))
					throw new InvalidOperationException("A modal navigation root must be initialized synchronously.");

				_connectedHandler = _modal.Handler;

				var rootView = _navigationRootManager.RootView ??
					throw new InvalidOperationException("Root view not initialized");

				var context = RequireContext();
				_container = new LinearLayout(context) { Orientation = Orientation.Vertical };
				ViewCompat.SetOnApplyWindowInsetsListener(_container, new SheetContentInsetsListener());

				// The Material 2 theme has no drag handle style, so the handle takes the Material 3 one there.
				var handleContext = RuntimeFeature.IsMaterial3Enabled
					? context
					: new ContextThemeWrapper(context, Resource.Style.Theme_Material3_DayNight_BottomSheetDialog);
				_dragHandle = new BottomSheetDragHandleView(handleContext);
				_container.AddView(_dragHandle, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
				_container.AddView(rootView, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0, 1f));

				UpdateDragHandle();
				_container.ViewTreeObserver?.AddOnGlobalLayoutListener(new ContentLayoutListener(this));
				return _container;
			}

			public override void OnStart()
			{
				base.OnStart();

				if (_modal is null || Dialog is not BottomSheetDialog dialog)
				{
					FirePresentationCompleted();
					return;
				}

				var behavior = dialog.Behavior;
				if (_behavior != behavior)
				{
					_behavior = behavior;
					_sheetCallback ??= new SheetCallback(this);
					behavior.AddBottomSheetCallback(_sheetCallback);
				}

				// Material only dismisses a cancelable dialog on an outside tap; this sheet decides with IsDismissible.
				var touchOutside = dialog.FindViewById(Resource.Id.touch_outside);
				touchOutside?.SetOnClickListener(new OutsideTapListener(this));

				var sheet = dialog.FindViewById(Resource.Id.design_bottom_sheet);
				if (sheet?.Parent is AView parent && parent != _parent)
				{
					_parent?.LayoutChange -= OnParentLayoutChange;
					_parent = parent;
					_parent.LayoutChange += OnParentLayoutChange;
				}

				ApplyConfiguration(selectDetent: true);
			}

			public override void OnResume()
			{
				base.OnResume();

				if (_modal?.IsLoaded == true)
					FirePresentationCompleted();
			}

			void OnModalLoaded(object? sender, EventArgs e)
			{
				if (IsResumed)
					FirePresentationCompleted();
			}

			void FirePresentationCompleted()
			{
				if (!_pendingPresentation)
					return;

				_pendingPresentation = false;
				_modal?.Loaded -= OnModalLoaded;
				_presented.TrySetResult();
			}

			/// <summary>Dismisses the sheet and completes once it is off screen. Completes immediately when it already is.</summary>
			internal Task DismissAsync(bool animated)
			{
				if (IsDismissed)
				{
					// The user dismissed it: the page was kept until the modal stack caught up.
					ReleasePage();
					return _dismissed.Task;
				}

				if (_dismissRequested)
					return _dismissed.Task;

				_dismissRequested = true;

				if (Dialog is not BottomSheetDialog dialog || !IsAdded || !dialog.IsShowing)
				{
					// Without a showing dialog, OnDismiss may never come to release the page.
					if (IsAdded)
						DismissAllowingStateLoss();

					ReleasePage();
					_dismissed.TrySetResult();
					return _dismissed.Task;
				}

				var behavior = dialog.Behavior;
				if (_backProgressActive)
				{
					// A predictive back gesture committed: let Material finish its own dismissal animation.
					_backProgressActive = false;
					behavior.Hideable = true;
					behavior.HandleBackInvoked();
				}
				else if (animated)
				{
					behavior.Hideable = true;
					dialog.DismissWithAnimation = true;
					DismissAllowingStateLoss();
				}
				else
				{
					dialog.DismissWithAnimation = false;
					dialog.Window?.SetWindowAnimations(0);
					DismissAllowingStateLoss();
				}

				return _dismissed.Task;
			}

			// Material cancels the dialog when the user swipes the sheet away or taps outside it; a dismissal without a cancel, such as
			// the activity going away, is not the user's.
			public override void OnCancel(IDialogInterface dialog)
			{
				_cancelledByUser = !_dismissRequested;
				base.OnCancel(dialog);
			}

			public override void OnDismiss(IDialogInterface dialog)
			{
				var modal = _modal;
				var window = _window;
				bool dismissedByUser = _cancelledByUser && !_dismissRequested && modal is not null;

				DetachFromPage();

				// After a user dismissal the modal stack still lists the page and needs its handler until it pops it (see DismissAsync).
				if (!dismissedByUser)
					ReleasePage();

				base.OnDismiss(dialog);
				_dismissed.TrySetResult();

				// Swiping the sheet down, tapping outside it, or a back press that Material handled.
				if (dismissedByUser && window is not null)
					window.ModalNavigationManager.OnPlatformModalDismissedAsync(modal!).FireAndForget();
			}

			public override void OnDestroy()
			{
				base.OnDestroy();
				FirePresentationCompleted();
				_dismissed.TrySetResult();
			}

			void DetachFromPage()
			{
				if (_behavior is not null && _sheetCallback is not null)
					_behavior.RemoveBottomSheetCallback(_sheetCallback);

				_parent?.LayoutChange -= OnParentLayoutChange;
				_parent = null;

				if (_modal is not null)
				{
					_modal.Loaded -= OnModalLoaded;
					_modal.PropertyChanged -= OnModalPropertyChanged;
					_modal.MeasureInvalidated -= OnModalMeasureInvalidated;
				}
			}

			/// <summary>Releases the page's platform views. Safe to call more than once.</summary>
			internal void ReleasePage()
			{
				DetachFromPage();

				// The page may already be presented again, with another handler, if it was pushed again right after it was dismissed.
				if (_modal is not null && ReferenceEquals(_modal.Handler, _connectedHandler))
				{
					if (_modal.Toolbar?.Handler is not null)
						_modal.Toolbar.Handler = null;

					_modal.Handler = null;
				}

				_modal = null;
				_connectedHandler = null;
				_container = null;
				_dragHandle = null;

				_window = null;
				_mauiWindowContext = null;
				_navigationRootManager?.Disconnect();
				_navigationRootManager = null;
			}

			void OnModalPropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (_modal is null)
					return;

				if (e.PropertyName == BottomSheet.SelectedDetentProperty.PropertyName)
				{
					if (!_updatingFromPlatform)
						ApplyConfiguration(selectDetent: true);
				}
				else if (e.PropertyName == BottomSheet.DetentsProperty.PropertyName)
				{
					ApplyConfiguration(selectDetent: true);
				}
				else if (e.PropertyName == BottomSheet.IsDismissibleProperty.PropertyName
					|| e.PropertyName == BottomSheet.CornerRadiusProperty.PropertyName
					|| e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName
					|| e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				{
					ApplyConfiguration(selectDetent: false);
				}
				else if (e.PropertyName == BottomSheet.HasDragHandleProperty.PropertyName)
				{
					UpdateDragHandle();
					_contentHeight = -1;
					ApplyConfiguration(selectDetent: false);
				}
			}

			void OnModalMeasureInvalidated(object? sender, EventArgs e)
			{
				if (_modal is null || !HasFitContent(_modal) || _contentRemeasurePending || _container is null)
					return;

				// Content changes come in bursts; measure once they are done.
				_contentRemeasurePending = true;
				_container.Post(() =>
				{
					_contentRemeasurePending = false;
					_contentHeight = -1;
					ApplyConfiguration(selectDetent: false);
				});
			}

			// Content measured while it changes can be stale, so the height of a FitContent sheet is checked again after each layout.
			void OnContentLayout()
			{
				if (_modal is null || _measureWidth <= 0 || !HasFitContent(_modal) || IsDismissed || _dismissRequested)
					return;

				double previous = _contentHeight;
				_contentHeight = -1;
				double current = MeasureContent(_modal, _measureWidth, _measureInset);
				if (Math.Abs(current - previous) > Tolerance)
					_container?.Post(() => ApplyConfiguration(selectDetent: false));
			}

			void OnParentLayoutChange(object? sender, AView.LayoutChangeEventArgs e)
			{
				int height = e.Bottom - e.Top;
				int width = e.Right - e.Left;
				if (height == _parentHeight && width == _parentWidth)
					return;

				// Changing the sheet's layout from inside a layout pass is ignored, so update after it.
				_parent?.Post(() => ApplyConfiguration(selectDetent: false));
			}

			void UpdateDragHandle()
			{
				if (_dragHandle is not null && _modal is not null)
					_dragHandle.Visibility = BottomSheet.GetHasDragHandle(_modal) ? ViewStates.Visible : ViewStates.Gone;
			}

			static bool HasFitContent(Page page)
			{
				foreach (var detent in BottomSheet.GetDetents(page))
				{
					if (detent.Kind == BottomSheetDetentKind.FitContent)
						return true;
				}

				return false;
			}

			void ApplyConfiguration(bool selectDetent)
			{
				if (_modal is null || _behavior is null || Dialog is not BottomSheetDialog dialog || IsDismissed || _dismissRequested || _applyingConfiguration)
					return;

				// Configuring the behavior can make Material change state on the spot; that is not the user moving the sheet.
				_applyingConfiguration = true;
				try
				{
					ApplyConfiguration(dialog, selectDetent);
				}
				finally
				{
					_applyingConfiguration = false;
				}
			}

			void ApplyConfiguration(BottomSheetDialog dialog, bool selectDetent)
			{
				if (_modal is null || _behavior is null)
					return;

				var page = _modal;
				var behavior = _behavior;
				var sheet = dialog.FindViewById(Resource.Id.design_bottom_sheet);
				var context = dialog.Context;

				behavior.Hideable = BottomSheet.GetIsDismissible(page);
				UpdateCornerRadius(sheet, page, context);
				UpdateSheetBackground(sheet, page);

				if (_parent is null || _parent.Height <= 0 || sheet is null)
					return;

				// The detent the sheet rests at, by value: FitContent can change its position among the detents.
				int previousIndex = CurrentStopIndex(behavior.State);
				var selected = BottomSheet.GetSelectedDetent(page);
				var restingDetent = previousIndex >= 0 && previousIndex < _stops.Count ? _stops[previousIndex].ReportedDetent(selected) : selected;
				_parentHeight = _parent.Height;
				_parentWidth = _parent.Width;

				// Detents measure the content area, like on iOS: below the status bar and above the navigation bar, which the sheet
				// covers too. Material adds the bottom inset to the collapsed position itself.
				// When the dialog isn't edge-to-edge, its window already keeps the sheet clear of the system bars.
				bool fitsSystemWindows = _parent.FitsSystemWindows || (_parent.Parent as AView)?.FitsSystemWindows == true;
				var insets = fitsSystemWindows ? null : ViewCompat.GetRootWindowInsets(_parent)?.GetInsets(WindowInsetsCompat.Type.SystemBars() | WindowInsetsCompat.Type.DisplayCutout());
				int topInset = insets?.Top ?? 0;
				int bottomInset = insets?.Bottom ?? 0;
				int availablePx = Math.Max(1, _parentHeight - topInset - bottomInset);

				double available = context.FromPixels(availablePx);
				double sheetWidth = context.FromPixels(sheet.Width > 0 ? sheet.Width : _parentWidth);
				double bottomInsetDp = context.FromPixels(bottomInset);
				_measureWidth = sheetWidth;
				_measureInset = bottomInsetDp;
				Func<double>? measure = HasFitContent(page) ? () => MeasureContent(page, sheetWidth, bottomInsetDp) : null;

				_stops = BottomSheetDetentResolver.Resolve(BottomSheet.GetDetents(page), available, measure, Tolerance);

				// The content heights of the resting positions, strictly increasing as Material requires.
				var heights = new int[_stops.Count];
				for (int i = 0; i < heights.Length; i++)
				{
					heights[i] = Math.Min(availablePx, (int)Math.Round(context.ToPixels(_stops[i].Height)));
					if (i > 0 && heights[i] <= heights[i - 1])
						heights[i] = heights[i - 1] + 2;
				}

				int sheetHeight = Math.Min(_parentHeight - topInset, heights[heights.Length - 1] + bottomInset);
				if (sheet.LayoutParameters is ViewGroup.LayoutParams layoutParams && layoutParams.Height != sheetHeight)
				{
					layoutParams.Height = sheetHeight;
					sheet.LayoutParameters = layoutParams;
				}

				int previousExpandedOffset = behavior.ExpandedOffset;
				float previousHalfExpandedRatio = behavior.HalfExpandedRatio;

				switch (heights.Length)
				{
					case 1:
						// The collapsed position is the sole detent too, so a sheet that can't be dismissed has nowhere else to rest.
						behavior.FitToContents = true;
						behavior.SkipCollapsed = true;
						behavior.PeekHeight = heights[0];
						break;
					case 2:
						behavior.FitToContents = true;
						behavior.SkipCollapsed = false;
						behavior.PeekHeight = heights[0];
						break;
					default:
						behavior.FitToContents = false;
						behavior.SkipCollapsed = false;
						behavior.PeekHeight = heights[0];
						behavior.ExpandedOffset = Math.Max(topInset, _parentHeight - sheetHeight);
						behavior.HalfExpandedRatio = Math.Clamp((float)(heights[1] + bottomInset) / _parentHeight, 0.01f, 0.99f);
						break;
				}

				// Material only moves to a changed offset or ratio when it lays the sheet out again.
				if (behavior.ExpandedOffset != previousExpandedOffset || behavior.HalfExpandedRatio != previousHalfExpandedRatio)
					sheet.RequestLayout();

				// A layout change while the user drags the sheet doesn't take the sheet out of their hands.
				if (!selectDetent && behavior.State is BottomSheetBehavior.StateDragging or BottomSheetBehavior.StateSettling or BottomSheetBehavior.StateHidden)
					return;

				int index = BottomSheetDetentResolver.IndexOf(_stops, selectDetent ? selected : restingDetent);
				if (index < 0)
				{
					// A detent the sheet doesn't have leaves the sheet where it is.
					index = selectDetent && previousIndex >= 0 ? previousIndex : BottomSheetDetentResolver.IndexOf(_stops, restingDetent);
					index = index < 0 ? 0 : Math.Min(index, _stops.Count - 1);
				}

				SetState(behavior, StateForIndex(index, _stops.Count));
			}

			void SetState(BottomSheetBehavior behavior, int state)
			{
				if (behavior.State == state)
					return;

				_pendingProgrammaticState = state;
				behavior.State = state;

				// Settled at once, while OnSheetStateChanged ignored it: nothing is pending any more.
				if (behavior.State == state)
					_pendingProgrammaticState = -1;
			}

			static int StateForIndex(int index, int count) => count switch
			{
				1 => BottomSheetBehavior.StateExpanded,
				2 => index == 0 ? BottomSheetBehavior.StateCollapsed : BottomSheetBehavior.StateExpanded,
				_ => index switch
				{
					0 => BottomSheetBehavior.StateCollapsed,
					1 => BottomSheetBehavior.StateHalfExpanded,
					_ => BottomSheetBehavior.StateExpanded,
				},
			};

			int CurrentStopIndex(int state)
			{
				int count = _stops.Count;
				if (count == 0)
					return -1;

				return state switch
				{
					BottomSheetBehavior.StateCollapsed => 0,
					BottomSheetBehavior.StateHalfExpanded => count >= 3 ? 1 : -1,
					BottomSheetBehavior.StateExpanded => count - 1,
					_ => -1,
				};
			}

			double MeasureContent(Page page, double width, double bottomInset)
			{
				if (_contentHeight >= 0 && Math.Abs(width - _measuredWidth) <= Tolerance)
					return _contentHeight;

				double handleHeight = 0;
				if (_dragHandle is { Visibility: ViewStates.Visible } handle)
				{
					if (handle.MeasuredHeight == 0)
						handle.Measure(AView.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified), AView.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
					handleHeight = handle.Context.FromPixels(handle.MeasuredHeight);
				}

				_measuredWidth = width;
				_contentHeight = BottomSheetDetentResolver.MeasureContentHeight(page, width) + handleHeight;
				return _contentHeight;
			}

			// A solid page background colors the whole sheet surface, including the drag handle area and the rounded corners.
			void UpdateSheetBackground(AView? sheet, Page page)
			{
				if (sheet?.Background is not Google.Android.Material.Shape.MaterialShapeDrawable shape)
					return;

				_themeFillColor ??= shape.FillColor;

				var color = page.BackgroundColor ?? (page.Background as SolidColorBrush)?.Color;
				shape.FillColor = color is null ? _themeFillColor : global::Android.Content.Res.ColorStateList.ValueOf(color.ToPlatform());
			}

			void UpdateCornerRadius(AView? sheet, Page page, Context context)
			{
				if (sheet?.Background is not Google.Android.Material.Shape.MaterialShapeDrawable shape)
					return;

				var radius = BottomSheet.GetCornerRadius(page);
				if (radius < 0)
				{
					if (_themeShape is not null)
						shape.ShapeAppearanceModel = _themeShape;
					return;
				}

				_themeShape ??= shape.ShapeAppearanceModel;
				float px = context.ToPixels(radius);
				shape.ShapeAppearanceModel = _themeShape.ToBuilder()
					.SetTopLeftCornerSize(px)
					.SetTopRightCornerSize(px)
					.Build();
			}

			void OnSheetStateChanged(int newState)
			{
				if (_applyingConfiguration)
					return;

				if (newState == BottomSheetBehavior.StateDragging)
				{
					_pendingProgrammaticState = -1;
					return;
				}

				if (newState == BottomSheetBehavior.StateSettling || newState == BottomSheetBehavior.StateHidden)
					return;

				if (newState == _pendingProgrammaticState)
				{
					_pendingProgrammaticState = -1;
					return;
				}

				if (_modal is null)
					return;

				int index = CurrentStopIndex(newState);
				if (index < 0 || index >= _stops.Count)
					return;

				var current = BottomSheet.GetSelectedDetent(_modal);
				var reported = _stops[index].ReportedDetent(current);
				if (reported.Equals(current))
					return;

				_updatingFromPlatform = true;
				try
				{
					_modal.SetValueFromRenderer(BottomSheet.SelectedDetentProperty, reported);
				}
				finally
				{
					_updatingFromPlatform = false;
				}
			}

			void OnOutsideTapped()
			{
				if (_modal is not null && BottomSheet.GetIsDismissible(_modal) && Dialog is BottomSheetDialog dialog)
					dialog.Cancel();
			}

			// Predictive back only previews the dismissal when a back press would dismiss the sheet.
			bool BackDismissesSheet =>
				_modal is not null &&
				BottomSheet.GetIsDismissible(_modal) &&
				!(_modal is NavigationPage navigationPage && navigationPage.Navigation.NavigationStack.Count > 1);

			void OnBackStarted(BackEventCompat backEvent)
			{
				if (_behavior is null || !BackDismissesSheet)
					return;

				_backProgressActive = true;
				_behavior.StartBackProgress(backEvent);
			}

			void OnBackProgressed(BackEventCompat backEvent)
			{
				if (_backProgressActive)
					_behavior?.UpdateBackProgress(backEvent);
			}

			void OnBackCancelled()
			{
				if (!_backProgressActive)
					return;

				_backProgressActive = false;
				_behavior?.CancelBackProgress();
			}

			void OnBackPressed(MauiBottomSheetDialog dialog)
			{
				// The window routes the press to the page (and a NavigationPage's stack) first; the default for a sheet pops it.
				if (dialog.Context.GetActivity() is global::Android.App.Activity activity)
				{
					IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnBackPressed>(del =>
					{
						del(activity);
					});
				}

				// The page handled the press without dismissing the sheet: put the previewed sheet back.
				if (_backProgressActive && !_dismissRequested)
					OnBackCancelled();
			}

			// The sheet is never under the status bar, and it pads itself above the navigation bar like the content area of a sheet on iOS,
			// so only the keyboard and the side insets reach the page.
			sealed class SheetContentInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
			{
				static readonly int[] s_types =
				{
					WindowInsetsCompat.Type.SystemBars(),
					WindowInsetsCompat.Type.StatusBars(),
					WindowInsetsCompat.Type.DisplayCutout(),
					WindowInsetsCompat.Type.TappableElement(),
					WindowInsetsCompat.Type.SystemGestures(),
					WindowInsetsCompat.Type.MandatorySystemGestures(),
					WindowInsetsCompat.Type.CaptionBar(),
				};

				public WindowInsetsCompat? OnApplyWindowInsets(AView? v, WindowInsetsCompat? insets)
				{
					if (insets is null)
						return insets;

					// Material can pad the sheet itself for the navigation bar; only what it leaves is padded here.
					int bottom = insets.GetInsets(WindowInsetsCompat.Type.SystemBars())?.Bottom ?? 0;
					bottom = Math.Max(0, bottom - ((v?.Parent as AView)?.PaddingBottom ?? 0));
					if (v is not null && v.PaddingBottom != bottom)
						v.SetPadding(v.PaddingLeft, v.PaddingTop, v.PaddingRight, bottom);

					var builder = new WindowInsetsCompat.Builder(insets);
					foreach (var type in s_types)
					{
						if (insets.GetInsets(type) is { } inset)
							builder.SetInsets(type, AndroidX.Core.Graphics.Insets.Of(inset.Left, 0, inset.Right, 0));
					}

					return builder.Build();
				}
			}

			sealed class ContentLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
			{
				readonly WeakReference<BottomSheetModalFragment> _fragment;

				public ContentLayoutListener(BottomSheetModalFragment fragment) => _fragment = new(fragment);

				public void OnGlobalLayout()
				{
					if (_fragment.TryGetTarget(out var fragment))
						fragment.OnContentLayout();
				}
			}

			sealed class SheetCallback : BottomSheetBehavior.BottomSheetCallback
			{
				readonly WeakReference<BottomSheetModalFragment> _fragment;

				public SheetCallback(BottomSheetModalFragment fragment) => _fragment = new(fragment);

				public override void OnSlide(AView bottomSheet, float newState)
				{
				}

				public override void OnStateChanged(AView bottomSheet, int newState)
				{
					if (_fragment.TryGetTarget(out var fragment))
						fragment.OnSheetStateChanged(newState);
				}
			}

			sealed class OutsideTapListener : Java.Lang.Object, AView.IOnClickListener
			{
				readonly WeakReference<BottomSheetModalFragment> _fragment;

				public OutsideTapListener(BottomSheetModalFragment fragment) => _fragment = new(fragment);

				public void OnClick(AView? v)
				{
					if (_fragment.TryGetTarget(out var fragment))
						fragment.OnOutsideTapped();
				}
			}

			sealed class MauiBottomSheetDialog : BottomSheetDialog
			{
				public MauiBottomSheetDialog(Context context, int theme, BottomSheetModalFragment fragment) : base(context, theme)
				{
					OnBackPressedDispatcher.AddCallback(new BackCallback(this, fragment));
				}

				public override bool OnKeyDown(Keycode keyCode, KeyEvent e) =>
					ModalDialogEvents.OnKeyDown(this, keyCode, e) || base.OnKeyDown(keyCode, e);

				public override bool OnKeyLongPress(Keycode keyCode, KeyEvent e) =>
					ModalDialogEvents.OnKeyLongPress(this, keyCode, e) || base.OnKeyLongPress(keyCode, e);

				public override bool OnKeyMultiple(Keycode keyCode, int repeatCount, KeyEvent e) =>
					ModalDialogEvents.OnKeyMultiple(this, keyCode, repeatCount, e) || base.OnKeyMultiple(keyCode, repeatCount, e);

				public override bool OnKeyShortcut(Keycode keyCode, KeyEvent e) =>
					ModalDialogEvents.OnKeyShortcut(this, keyCode, e) || base.OnKeyShortcut(keyCode, e);

				public override bool OnKeyUp(Keycode keyCode, KeyEvent e) =>
					ModalDialogEvents.OnKeyUp(this, keyCode, e) || base.OnKeyUp(keyCode, e);

				public override bool DispatchTouchEvent(MotionEvent? e)
				{
					if (e is null)
						return false;

					bool handled = base.DispatchTouchEvent(e);
					return ModalDialogEvents.DispatchTouchEvent(this, e) || handled;
				}

				sealed class BackCallback : OnBackPressedCallback
				{
					readonly WeakReference<MauiBottomSheetDialog> _dialog;
					readonly WeakReference<BottomSheetModalFragment> _fragment;

					public BackCallback(MauiBottomSheetDialog dialog, BottomSheetModalFragment fragment) : base(true)
					{
						_dialog = new(dialog);
						_fragment = new(fragment);
					}

					public override void HandleOnBackStarted(BackEventCompat backEvent)
					{
						if (_fragment.TryGetTarget(out var fragment))
							fragment.OnBackStarted(backEvent);
					}

					public override void HandleOnBackProgressed(BackEventCompat backEvent)
					{
						if (_fragment.TryGetTarget(out var fragment))
							fragment.OnBackProgressed(backEvent);
					}

					public override void HandleOnBackCancelled()
					{
						if (_fragment.TryGetTarget(out var fragment))
							fragment.OnBackCancelled();
					}

					public override void HandleOnBackPressed()
					{
						if (_fragment.TryGetTarget(out var fragment) && _dialog.TryGetTarget(out var dialog))
							fragment.OnBackPressed(dialog);
					}
				}
			}
		}
	}
}
