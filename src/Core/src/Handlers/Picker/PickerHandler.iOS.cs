using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, MauiPicker>
	{
		readonly MauiPickerProxy _proxy = new();
		UIPickerView? _pickerView;
		UITapGestureRecognizer? _tapGestureRecognizer;

#if MACCATALYST
		UIAlertController? _pickerController;
		MacIdiomMenuButton? _macIdiomMenuButton;
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
		protected override MauiPicker CreatePlatformView()
		{
			var platformPicker = new MauiPicker(null) { BorderStyle = UITextBorderStyle.RoundedRect };

			// In the Mac idiom UIPickerView throws when it moves to a window, so show the items in a native menu instead.
			if (OperatingSystem.IsMacCatalystVersionAtLeast(15) && MacIdiomExtensions.IsMacIdiom)
			{
				_macIdiomMenuButton = new MacIdiomMenuButton(this);
				platformPicker.AddSubview(_macIdiomMenuButton);
				NSLayoutConstraint.ActivateConstraints(
				[
					_macIdiomMenuButton.LeadingAnchor.ConstraintEqualTo(platformPicker.LeadingAnchor),
					_macIdiomMenuButton.TrailingAnchor.ConstraintEqualTo(platformPicker.TrailingAnchor),
					_macIdiomMenuButton.TopAnchor.ConstraintEqualTo(platformPicker.TopAnchor),
					_macIdiomMenuButton.BottomAnchor.ConstraintEqualTo(platformPicker.BottomAnchor),
				]);
			}

			return platformPicker;
		}

		void DisplayAlert(MauiPicker uITextField, int selectedIndex)
		{
			var paddingTitle = 0;
			if (!string.IsNullOrEmpty(VirtualView.Title))
				paddingTitle += 25;

			var pickerHeight = 240;
            var pickerView = new UIPickerView();
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
				_pickerController.View.AddSubview(pickerView);
				
				var container = _pickerController.View;
				pickerView.TranslatesAutoresizingMaskIntoConstraints = false;
				NSLayoutConstraint.ActivateConstraints(
				[
					pickerView.CenterXAnchor.ConstraintEqualTo(container.CenterXAnchor),
					pickerView.WidthAnchor.ConstraintEqualTo(container.WidthAnchor),
					pickerView.TopAnchor.ConstraintEqualTo(container.TopAnchor, paddingTitle),
					pickerView.HeightAnchor.ConstraintEqualTo(pickerHeight),
				]);

				var doneButtonHeight = 90;
				var height = NSLayoutConstraint.Create(_pickerController.View, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, pickerHeight + doneButtonHeight);
				_pickerController.View.AddConstraint(height);
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

		// These run from native menu callbacks, so they use the nullable accessors and locals: raising a picker event can
		// disconnect the handler.
		void SelectMacIdiomMenuItem(int index, string item)
		{
			if (((IElementHandler)this).VirtualView is not IPicker picker || ((IElementHandler)this).PlatformView is not MauiPicker platformPicker)
				return;

			// Ignore a menu that was showing while the items changed.
			if (index >= picker.GetCount() || picker.GetItem(index) != item)
				return;

			platformPicker.Text = item;
			picker.SelectedIndex = index;
			platformPicker.UpdateCharacterSpacing(picker);
		}

		void OnMacIdiomMenuDisplayed()
		{
			if (((IElementHandler)this).VirtualView is IPicker picker)
				picker.IsFocused = picker.IsOpen = true;
		}

		void OnMacIdiomMenuEnded()
		{
			var platformPicker = ((IElementHandler)this).PlatformView as MauiPicker;

			if (((IElementHandler)this).VirtualView is IPicker picker)
				picker.IsFocused = picker.IsOpen = false;

			if (platformPicker?.IsFirstResponder == true)
				platformPicker.ResignFirstResponder();
		}

		// An invisible button over the picker field that shows the items in a native menu when the app uses the Mac idiom.
		sealed class MacIdiomMenuButton : UIButton
		{
			readonly WeakReference<PickerHandler> _handler;
			bool _isDisconnected;

			public MacIdiomMenuButton(PickerHandler handler)
			{
				_handler = new(handler);
				TranslatesAutoresizingMaskIntoConstraints = false;
				// Lets VoiceOver interact with the picker field itself, which opens the menu when it gets focus.
				IsAccessibilityElement = false;

				if (OperatingSystem.IsMacCatalystVersionAtLeast(15))
				{
					// The Pad behavioral style keeps the Mac idiom from drawing a push button bezel over the field.
					PreferredBehavioralStyle = UIBehavioralStyle.Pad;
					ShowsMenuAsPrimaryAction = true;
					// The items are built each time the menu opens, so they reflect the current items and selection.
					var weakButton = new WeakReference<MacIdiomMenuButton>(this);
					Menu = UIMenu.Create([UIDeferredMenuElement.CreateUncached(completion =>
						completion(weakButton.TryGetTarget(out var button) ? button.CreateMenuItems() : []))]);
				}
			}

			public bool IsMenuOpen { get; private set; }

			public void UpdateIsOpen(bool isOpen)
			{
				if (isOpen && !IsMenuOpen)
				{
					// Presenting a control's menu programmatically needs Mac Catalyst 17.4; earlier versions open it on click only.
					// When it's shown, WillDisplayMenu runs before PerformPrimaryAction returns.
					if (OperatingSystem.IsMacCatalystVersionAtLeast(17, 4) && Superview is UIControl { Enabled: true })
						PerformPrimaryAction();

					if (!IsMenuOpen)
					{
						// The menu couldn't be shown, so don't leave the picker focused and open. This runs after the current
						// property change completes, so it can't be overwritten by the caller.
						CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
						{
							if (!IsMenuOpen)
								Handler?.OnMacIdiomMenuEnded();
						});
					}
				}
				else if (!isOpen && IsMenuOpen)
				{
					// On Mac this may not close a menu that is already showing; the user closes it with a click or Escape.
					ContextMenuInteraction?.DismissMenu();
				}
			}

			PickerHandler? Handler => !_isDisconnected && _handler.TryGetTarget(out var handler) ? handler : null;

			// A menu that is already showing can outlive the handler, so its callbacks must not reach the handler afterwards.
			public void Disconnect()
			{
				_isDisconnected = true;
				Menu = null;
			}

			UIMenuElement[] CreateMenuItems()
			{
				if (Handler is not PickerHandler handler || ((IElementHandler)handler).VirtualView is not IPicker picker)
					return [];

				var count = picker.GetCount();
				var items = new UIMenuElement[count];

				for (int i = 0; i < count; i++)
				{
					var index = i;
					var item = picker.GetItem(i) ?? string.Empty;
					var weakButton = new WeakReference<MacIdiomMenuButton>(this);
					var action = UIAction.Create(item, null, null, _ =>
					{
						if (weakButton.TryGetTarget(out var button))
							button.Handler?.SelectMacIdiomMenuItem(index, item);
					});

					action.State = i == picker.SelectedIndex ? UIMenuElementState.On : UIMenuElementState.Off;
					items[i] = action;
				}

				return items;
			}

			public override UIView? HitTest(CoreGraphics.CGPoint point, UIEvent? uievent)
			{
				// MAUI disables the picker field (a UIControl) for IsEnabled = false, which doesn't stop its subviews receiving clicks.
				if (Superview is UIControl { Enabled: false })
					return null;

				return base.HitTest(point, uievent);
			}

			public override void WillDisplayMenu(UIContextMenuInteraction interaction, UIContextMenuConfiguration configuration, IUIContextMenuInteractionAnimating? animator)
			{
				base.WillDisplayMenu(interaction, configuration, animator);
				IsMenuOpen = true;
				Handler?.OnMacIdiomMenuDisplayed();
			}

			public override void WillEnd(UIContextMenuInteraction interaction, UIContextMenuConfiguration configuration, IUIContextMenuInteractionAnimating? animator)
			{
				base.WillEnd(interaction, configuration, animator);
				IsMenuOpen = false;
				Handler?.OnMacIdiomMenuEnded();
			}
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
			if (_pickerController != null)
			{
				_pickerController.DismissViewController(false, null);
				_pickerController = null;
			}

			if (_macIdiomMenuButton is not null)
			{
				// Not disposed: a menu that is already showing on Mac can't be closed and still calls back into the button.
				_macIdiomMenuButton.Disconnect();
				_macIdiomMenuButton.RemoveFromSuperview();
				_macIdiomMenuButton = null;
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
		}

		[Obsolete("Use Microsoft.Maui.Handlers.PickerHandler.MapItems instead")]
		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		internal static void MapItems(IPickerHandler handler, IPicker picker) => Reload(handler);

#if MACCATALYST
		// Handle programmatic unfocus on MacCatalyst by dismissing the picker dialog
		// This allows external code to close the picker (e.g., clicking outside, navigation)
		internal static async void MapUnfocus(IPickerHandler handler, IPicker picker, object? args)
		{
			if (handler is PickerHandler { _macIdiomMenuButton: { } menuButton } macIdiomHandler)
			{
				if (menuButton.IsMenuOpen)
					menuButton.UpdateIsOpen(false);
				else
					macIdiomHandler.OnMacIdiomMenuEnded();

				return;
			}

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
		}

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
#if MACCATALYST
			if (handler is PickerHandler { _macIdiomMenuButton: { } menuButton })
			{
				menuButton.UpdateIsOpen(picker.IsOpen);
				return;
			}
#endif
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
			if (pickerSource != null && pickerSource.SelectedIndex == -1 && count > 0)
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
			PlatformView.Window.AddGestureRecognizer(_tapGestureRecognizer);
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
			}

			public void Disconnect(MauiPicker platformView)
			{
				Handler?.RemoveTouchDismissGesture();
				platformView.EditingDidBegin -= OnStarted;
				platformView.EditingDidEnd -= OnEnded;
				platformView.EditingChanged -= OnEditing;
			}

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

				// In the Mac idiom, setting IsOpen above usually opened the menu already; this also covers IsOpen having been
				// true before. The menu closes itself on an outside click.
				if (handler._macIdiomMenuButton is { } menuButton)
				{
					menuButton.UpdateIsOpen(true);
					return;
				}

				int selectedIndex = handler.VirtualView?.SelectedIndex ?? 0;
				handler.DisplayAlert(handler.PlatformView, selectedIndex);
#endif
				Handler?.SetupTouchDismissGesture();
			}

			void OnEnded(object? sender, EventArgs eventArgs)
			{
				Handler?.RemoveTouchDismissGesture();

#if MACCATALYST
				// In the Mac idiom the field can lose focus without a menu showing (for example on Tab).
				if (Handler is PickerHandler { _macIdiomMenuButton.IsMenuOpen: false } && VirtualView is IPicker macIdiomPicker)
				{
					macIdiomPicker.IsFocused = macIdiomPicker.IsOpen = false;
					return;
				}
#endif

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
