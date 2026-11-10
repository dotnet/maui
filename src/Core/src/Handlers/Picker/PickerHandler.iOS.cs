using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, MauiPicker>
	{
		readonly MauiPickerProxy _proxy = new();
		UIPickerView? _pickerView;
		UITapGestureRecognizer? _tapGestureRecognizer;
		bool _selectionWasCleared;

#if MACCATALYST
		UIAlertController? _pickerController;
		UIButton? _dialogClearButton;
		NSLayoutConstraint? _dialogClearButtonHeight;
		NSLayoutConstraint? _dialogHeight;
#endif

#if !MACCATALYST
		protected override MauiPicker CreatePlatformView()
		{
			_pickerView = new UIPickerView();

			var platformPicker = new MauiPicker(_pickerView)
			{
				BorderStyle = UITextBorderStyle.RoundedRect,
				InputView = _pickerView,
				InputAccessoryView = new MauiDoneAccessoryView(_proxy.OnDone),
			};

			platformPicker.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			platformPicker.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			platformPicker.InputAssistantItem.LeadingBarButtonGroups = null;
			platformPicker.InputAssistantItem.TrailingBarButtonGroups = null;
			platformPicker.AccessibilityTraits = UIAccessibilityTrait.Button;

			_pickerView.Model = new PickerSource(this);

			return platformPicker;
		}

		void OnDone() => FinishSelectItem(_pickerView, PlatformView);
#else
		protected override MauiPicker CreatePlatformView() =>
			new MauiPicker(null) { BorderStyle = UITextBorderStyle.RoundedRect };

		void DisplayAlert(MauiPicker uITextField, int selectedIndex)
		{
			DisposeDialogClearButton();
			var paddingTitle = 0;
			if (!string.IsNullOrEmpty(VirtualView.Title))
				paddingTitle += 25;

			var pickerHeight = 240;
			var pickerView = new UIPickerView();
			_pickerView = pickerView;
			pickerView.Model = new PickerSource(this);
			pickerView?.ReloadAllComponents();

			if (pickerView?.Model is PickerSource source)
			{
				source.SelectedIndex = selectedIndex;
				pickerView.Select(Math.Max(selectedIndex, 0), 0, true);
				pickerView.ReloadAllComponents();
			}

			// The UIPickerView is displayed as a subview of the UIAlertController when an empty string is provided as the title, instead of using the VirtualView title. 
			// This behavior deviates from the expected native macOS behavior.
			_pickerController = UIAlertController.Create("", "", UIAlertControllerStyle.ActionSheet);
			_dialogClearButton = UIButton.FromType(UIButtonType.System);
			_dialogClearButton.SetTitle(NSBundle.FromIdentifier("com.apple.UIKit")?.GetLocalizedString("Clear") ?? "Clear", UIControlState.Normal);
			_dialogClearButton.AccessibilityIdentifier = "PickerClearButton";
			_dialogClearButton.TranslatesAutoresizingMaskIntoConstraints = false;
			_dialogClearButton.TouchUpInside += _proxy.OnClear;
			_dialogClearButton.Hidden = !VirtualView.ShouldShowClearButton();
			_dialogClearButton.Enabled = VirtualView.IsEnabled;
			var clearButtonHeight = VirtualView is IPickerClearButton { IsClearButtonVisible: true } ? 44 : 0;
			_dialogClearButtonHeight = _dialogClearButton.HeightAnchor.ConstraintEqualTo(clearButtonHeight);

			// needs translation
			// Handle picker dismissal directly in the Done action instead of using EditingDidEnd event
			// This simplifies the cleanup logic, avoids duplicate dismiss calls, and prevents VoiceOver issues
			// Note: EditingDidEnd event breaks VoiceOver accessibility when dismissing the picker, which is why it hasn't been used
			_pickerController.AddAction(UIAlertAction.Create("Done",
								UIAlertActionStyle.Default,
								action =>
								{
									FinishSelectItem(pickerView, uITextField);
									if (VirtualView is IPicker virtualView)
										virtualView.IsFocused = virtualView.IsOpen = false;
								}
							));

			if (_pickerController.View != null && pickerView != null)
			{
				_pickerController.View.AddSubview(_dialogClearButton);
				_pickerController.View.AddSubview(pickerView);
				
				var container = _pickerController.View;
				pickerView.TranslatesAutoresizingMaskIntoConstraints = false;
				NSLayoutConstraint.ActivateConstraints(
				[
					pickerView.CenterXAnchor.ConstraintEqualTo(container.CenterXAnchor),
					pickerView.WidthAnchor.ConstraintEqualTo(container.WidthAnchor),
					_dialogClearButton.TopAnchor.ConstraintEqualTo(container.TopAnchor),
					_dialogClearButton.LeadingAnchor.ConstraintEqualTo(container.LayoutMarginsGuide.LeadingAnchor),
					_dialogClearButtonHeight,
					pickerView.TopAnchor.ConstraintEqualTo(_dialogClearButton.BottomAnchor, paddingTitle),
					pickerView.HeightAnchor.ConstraintEqualTo(pickerHeight),
				]);

				var doneButtonHeight = 90;
				_dialogHeight = NSLayoutConstraint.Create(_pickerController.View, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, pickerHeight + doneButtonHeight + clearButtonHeight);
				_pickerController.View.AddConstraint(_dialogHeight);
			}

			var popoverPresentation = _pickerController.PopoverPresentationController;
			if (popoverPresentation != null)
			{
				popoverPresentation.SourceView = uITextField;
				popoverPresentation.SourceRect = uITextField.Bounds;
			}

			var platformWindow = MauiContext?.GetPlatformWindow();
			if (platformWindow is null)
			{
				return;
			}

			var currentViewController = GetCurrentViewController(platformWindow.RootViewController);
			platformWindow.BeginInvokeOnMainThread(() =>
			{
				if (currentViewController is not null)
				{
					currentViewController.PresentViewControllerAsync(_pickerController, true)
						.ContinueWith(_ => pickerView?.PostAccessibilityFocusNotification(), TaskScheduler.FromCurrentSynchronizationContext())
						.FireAndForget();
				}
			});
		}

		static UIViewController? GetCurrentViewController(UIViewController? viewController)
		{
			while (viewController?.PresentedViewController != null)
			{
				viewController = viewController.PresentedViewController;
			}
 
			return viewController;
		}
#endif

		internal bool UpdateImmediately { get; set; }

		protected override void ConnectHandler(MauiPicker platformView)
		{
			_proxy.Connect(this, VirtualView, platformView);

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiPicker platformView)
		{
			_proxy.Disconnect(platformView);
			RemoveTouchDismissGesture();

#if MACCATALYST
			DisposeDialogClearButton();
			if (_pickerController != null)
			{
				_pickerController.DismissViewController(false, null);
				_pickerController = null;
			}
#endif

			if (_pickerView != null)
			{
				if (_pickerView.Model != null)
				{
					_pickerView.Model = null;
				}

				_pickerView.RemoveFromSuperview();
				_pickerView = null;
			}

			base.DisconnectHandler(platformView);
		}

		static void Reload(IPickerHandler handler)
		{
			handler.PlatformView.UpdatePicker(handler.VirtualView);
#if MACCATALYST
			if (handler is PickerHandler pickerHandler)
				pickerHandler._pickerView?.ReloadAllComponents();
#endif
			handler.UpdateValue(nameof(IPickerClearButton.IsClearButtonVisible));
		}

		[Obsolete("Use Microsoft.Maui.Handlers.PickerHandler.MapItems instead")]
		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		internal static void MapItems(IPickerHandler handler, IPicker picker) => Reload(handler);

#if MACCATALYST
		// Handle programmatic unfocus on MacCatalyst by dismissing the picker dialog
		// This allows external code to close the picker (e.g., clicking outside, navigation)
		internal static async void MapUnfocus(IPickerHandler handler, IPicker picker, object? args)
		{
			if (handler is PickerHandler pickerHandler && 
				pickerHandler._pickerController is not null)
			{
				try
				{
					await pickerHandler._pickerController.DismissViewControllerAsync(true);
				}
				catch (ObjectDisposedException)
				{
					// Already dismissed - safe to ignore
				}
				
				if (handler.VirtualView is IPicker virtualView)
					virtualView.IsFocused = virtualView.IsOpen = false;
			}
		}
#endif

		public static void MapTitle(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTitle(picker);
		}

		public static void MapTitleColor(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTitleColor(picker);
		}

		public static void MapSelectedIndex(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateSelectedIndex(picker);
			if (handler is PickerHandler pickerHandler)
			{
				if (picker.SelectedIndex >= 0)
					pickerHandler._selectionWasCleared = false;
#if MACCATALYST
				if (pickerHandler._pickerView?.Model is PickerSource source)
				{
					source.SelectedIndex = picker.SelectedIndex;
					pickerHandler._pickerView.Select(Math.Max(picker.SelectedIndex, 0), 0, false);
				}
#endif
			}
			handler.UpdateValue(nameof(IPickerClearButton.IsClearButtonVisible));
		}

		partial void UpdateClearButton()
		{
			if (VirtualView is not { } picker || PlatformView is not { } platformView)
				return;

			platformView.ClearButtonMode = picker.ShouldShowClearButton()
				? UITextFieldViewMode.Always
				: UITextFieldViewMode.Never;
#if MACCATALYST
			if (_dialogClearButton is { } button)
			{
				button.Hidden = !picker.ShouldShowClearButton();
				button.Enabled = picker.IsEnabled;
				var height = picker is IPickerClearButton { IsClearButtonVisible: true } ? 44 : 0;
				if (_dialogClearButtonHeight is { } buttonHeight)
					buttonHeight.Constant = height;
				if (_dialogHeight is { } dialogHeight)
					dialogHeight.Constant = 240 + 90 + height;
			}
#endif
		}

		internal void ClearSelection()
		{
			if (VirtualView is not { IsEnabled: true } picker || !picker.ShouldShowClearButton())
				return;

			_selectionWasCleared = true;
			if (_pickerView?.Model is PickerSource source)
				source.SelectedIndex = -1;

			picker.ClearSelection();
		}

#if MACCATALYST
		void DisposeDialogClearButton()
		{
			if (_dialogClearButton is { } button)
			{
				button.TouchUpInside -= _proxy.OnClear;
				button.RemoveFromSuperview();
				button.Dispose();
				_dialogClearButton = null;
			}
			_dialogClearButtonHeight?.Dispose();
			_dialogClearButtonHeight = null;
			_dialogHeight?.Dispose();
			_dialogHeight = null;
		}
#endif

		public static void MapCharacterSpacing(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(picker);
		}

		public static void MapFont(IPickerHandler handler, IPicker picker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(picker, fontManager);
		}

		public static void MapHorizontalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(picker);
		}

		public static void MapTextColor(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTextColor(picker);
		}

		public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(picker);
		}

		internal static void MapIsOpen(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateIsOpen(picker);
		}

		void UpdatePickerFromPickerSource(PickerSource? pickerSource)
		{
			if (VirtualView == null || PlatformView == null || pickerSource == null)
				return;

			PlatformView.Text = VirtualView.GetItem(pickerSource.SelectedIndex);

			VirtualView.SelectedIndex = pickerSource.SelectedIndex;
			// Re-apply kern: plain Text assignment above clears iOS kern attributes.
			PlatformView.UpdateCharacterSpacing(VirtualView);
		}

		void UpdatePickerSelectedIndex(UIPickerView? pickerView, int formsIndex)
		{
			if (VirtualView == null || pickerView == null)
				return;

			if (pickerView.Model is not PickerSource source)
				return;
			source.SelectedIndex = formsIndex;
			pickerView.Select(Math.Max(formsIndex, 0), 0, true);
		}

		void FinishSelectItem(UIPickerView? pickerView, MauiPicker textField)
		{
			var pickerSource = pickerView?.Model as PickerSource;
			var count = VirtualView?.GetCount() ?? 0;
			if (pickerSource != null && pickerSource.SelectedIndex == -1 && count > 0 && !_selectionWasCleared)
				UpdatePickerSelectedIndex(pickerView, 0);

			UpdatePickerFromPickerSource(pickerSource);
			textField.ResignFirstResponder();
		}

		void SetupTouchDismissGesture()
		{
			// Don't setup if window isn't available yet
			if (PlatformView?.Window is null)
			{
    			return;
			}

			// Don't setup if already configured
			if (_tapGestureRecognizer is not null)
			{
    			return;
			}
			var weakHandler = new WeakReference<PickerHandler>(this);
			_tapGestureRecognizer = new UITapGestureRecognizer(() =>
			{
				if (weakHandler.TryGetTarget(out var handler))
				{
					handler.DismissPicker();
				}
			});
			_tapGestureRecognizer.CancelsTouchesInView = false;
			_tapGestureRecognizer.ShouldReceiveTouch = _proxy.ShouldReceiveDismissTouch;
			PlatformView.Window.AddGestureRecognizer(_tapGestureRecognizer);
		}

		bool ShouldReceiveDismissTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			if (VirtualView is not { } picker || !picker.ShouldShowClearButton())
				return true;

			var touchedView = touch.View;
			if (touchedView is null)
				return true;

			if (PlatformView is { } platformView && touchedView.IsDescendantOfView(platformView))
				return false;

#if MACCATALYST
			if (_dialogClearButton is { } button && touchedView.IsDescendantOfView(button))
				return false;
#endif
			return true;
		}
		
		void DismissPicker()
		{
#if MACCATALYST
			// On MacCatalyst, dismiss the UIAlertController and clean up the tap gesture directly,
			// since ResignFirstResponder is not called here and OnEnded will not fire.
			if (_pickerController is not null)
			{
				_pickerController.DismissViewController(true, null);
				if (VirtualView is IPicker virtualView)
					virtualView.IsFocused = virtualView.IsOpen = false;
			}
			RemoveTouchDismissGesture();
#else
			// On iOS, dismiss by ending editing
			PlatformView?.EndEditing(true);
#endif
		}
  

		void RemoveTouchDismissGesture()
		{
			if (_tapGestureRecognizer is not null)
			{
				_tapGestureRecognizer.View?.RemoveGestureRecognizer(_tapGestureRecognizer);
				_tapGestureRecognizer.Dispose();
				_tapGestureRecognizer = null;
			}
		}

		class MauiPickerProxy
		{
			WeakReference<PickerHandler>? _handler;
			WeakReference<IPicker>? _virtualView;

			IPicker? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			PickerHandler? Handler => _handler is not null && _handler.TryGetTarget(out var h) ? h : null;

			public void Connect(PickerHandler handler, IPicker virtualView, MauiPicker platformView)
			{
				_handler = new(handler);
				_virtualView = new(virtualView);

				platformView.EditingDidBegin += OnStarted;
				platformView.EditingDidEnd += OnEnded;
				platformView.EditingChanged += OnEditing;
				platformView.ShouldClear += OnShouldClear;
			}

			public void Disconnect(MauiPicker platformView)
			{
				Handler?.RemoveTouchDismissGesture();
				platformView.EditingDidBegin -= OnStarted;
				platformView.EditingDidEnd -= OnEnded;
				platformView.EditingChanged -= OnEditing;
				platformView.ShouldClear -= OnShouldClear;
			}

			bool OnShouldClear(UITextField textField)
			{
				if (Handler is PickerHandler handler &&
					handler.VirtualView is IPickerClearButton { IsClearButtonVisible: true })
				{
					handler.ClearSelection();
					return false;
				}
				return true;
			}

			public void OnClear(object? sender, EventArgs e) => Handler?.ClearSelection();

			public bool ShouldReceiveDismissTouch(UIGestureRecognizer recognizer, UITouch touch) =>
				Handler?.ShouldReceiveDismissTouch(recognizer, touch) ?? true;

#if !MACCATALYST
			public void OnDone()
			{
				if (Handler is PickerHandler handler)
				{
					handler.OnDone();
				}
			}
#endif

			void OnStarted(object? sender, EventArgs eventArgs)
			{
				if (VirtualView is IPicker virtualView)
					virtualView.IsFocused = virtualView.IsOpen = true;

				// Notify VoiceOver that the picker popup has appeared
				if (sender is MauiPicker platformView && platformView.InputView is not null)
				{
					platformView.PostAccessibilityFocusNotification(platformView.InputView);
				}
#if MACCATALYST
				if (Handler is not PickerHandler handler)
					return;

				int selectedIndex = handler.VirtualView?.SelectedIndex ?? 0;
				handler.DisplayAlert(handler.PlatformView, selectedIndex);
#endif
				Handler?.SetupTouchDismissGesture();
			}

			void OnEnded(object? sender, EventArgs eventArgs)
			{
				Handler?.RemoveTouchDismissGesture();
#if !MACCATALYST
				if (Handler is not PickerHandler handler || handler._pickerView is not UIPickerView pickerView)
					return;

				var model = pickerView.Model as PickerSource;
				if (model is not null && model.SelectedIndex != -1 && model.SelectedIndex != pickerView.SelectedRowInComponent(0))
				{
					pickerView.Select(model.SelectedIndex, 0, false);
				}

				if (VirtualView is IPicker virtualView)
				{
					virtualView.IsFocused = virtualView.IsOpen = false;
				}

				// Restore VoiceOver focus to the picker field when the popup closes
				if (sender is MauiPicker platformView)
				{
					platformView.PostAccessibilityFocusNotification();
				}
#endif
			}

			void OnEditing(object? sender, EventArgs eventArgs)
			{
				if (sender is not MauiPicker platformView || VirtualView is not IPicker virtualView)
					return;

				// Reset the TextField's Text so it appears as if typing with a keyboard does not work.
				var selectedIndex = virtualView.SelectedIndex;

				platformView.Text = virtualView.GetItem(selectedIndex);

				// Also clears the undo stack (undo/redo possible on iPads)
				platformView.UndoManager?.RemoveAllActions();
			}
		}
	}

	public class PickerSource : UIPickerViewModel
	{
		WeakReference<PickerHandler>? _weakReference;

		public PickerSource(PickerHandler? handler)
		{
			Handler = handler;
		}

		public PickerHandler? Handler
		{
			get
			{
				if (_weakReference?.TryGetTarget(out PickerHandler? target) == true)
					return target;

				return null;
			}
			set
			{
				_weakReference = null;
				if (value == null)
					return;

				_weakReference = new WeakReference<PickerHandler>(value);
			}
		}

		public int SelectedIndex { get; internal set; }

		public override nint GetComponentCount(UIPickerView picker) => 1;

		public override nint GetRowsInComponent(UIPickerView pickerView, nint component) =>
			Handler?.VirtualView?.GetCount() ?? 0;

		public override string GetTitle(UIPickerView picker, nint row, nint component) =>
			Handler?.VirtualView?.GetItem((int)row) ?? string.Empty;

		public override void Selected(UIPickerView picker, nint row, nint component)
		{
			SelectedIndex = (int)row;

			if (Handler != null && Handler.UpdateImmediately)  // Platform Specific
			{
				var virtualView = Handler?.VirtualView;
				var platformView = Handler?.PlatformView;

				if (virtualView == null || platformView == null)
					return;

				platformView.UpdatePicker(virtualView, SelectedIndex);
			}
		}
	}
}
