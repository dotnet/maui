using System;
using System.Collections.Specialized;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, MauiPicker>
	{
		readonly MauiPickerProxy _proxy = new();
		UIPickerView? _pickerView;

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
			var paddingTitle = 0;
			if (!string.IsNullOrEmpty(VirtualView.Title))
				paddingTitle += 25;

			var pickerHeight = 240;
			var frame = new RectangleF(0, paddingTitle, 269, pickerHeight);
			var pickerView = new UIPickerView(frame);
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
			var pickerController = UIAlertController.Create("", "", UIAlertControllerStyle.ActionSheet);

			// needs translation
			pickerController.AddAction(UIAlertAction.Create("Done",
								UIAlertActionStyle.Default,
								action => FinishSelectItem(pickerView, uITextField)
							));

			if (pickerController.View != null && pickerView != null)
			{
				pickerController.View.AddSubview(pickerView);
				var doneButtonHeight = 90;
				var height = NSLayoutConstraint.Create(pickerController.View, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, pickerHeight + doneButtonHeight);
				pickerController.View.AddConstraint(height);
			}

			var popoverPresentation = pickerController.PopoverPresentationController;
			if (popoverPresentation != null)
			{
				popoverPresentation.SourceView = uITextField;
				popoverPresentation.SourceRect = uITextField.Bounds;
			}

			EventHandler? editingDidEndHandler = null;

			editingDidEndHandler = async (s, e) =>
			{
				await pickerController.DismissViewControllerAsync(true);
				if (VirtualView is IPicker virtualView)
					virtualView.IsFocused = false;
				uITextField.EditingDidEnd -= editingDidEndHandler;
				
				// Restore VoiceOver focus to the picker field when the alert closes
				uITextField.PostAccessibilityFocusNotification();
			};

			uITextField.EditingDidEnd += editingDidEndHandler;

			var platformWindow = MauiContext?.GetPlatformWindow();
			if (platformWindow is null)
			{
				return;
			}

			var currentViewController = GetCurrentViewController(platformWindow.RootViewController);
			platformWindow.BeginInvokeOnMainThread(async () =>
			{
				if (currentViewController is not null)
				{
					await currentViewController.PresentViewControllerAsync(pickerController, true);
				}

				// Notify VoiceOver that the picker alert has appeared
				pickerView?.PostAccessibilityFocusNotification();
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

		// Uncomment me on NET8 [Obsolete]
		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		internal static void MapItems(IPickerHandler handler, IPicker picker) => Reload(handler);

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

		void UpdatePickerFromPickerSource(PickerSource? pickerSource)
		{
			if (VirtualView == null || PlatformView == null || pickerSource == null)
				return;

			PlatformView.Text = VirtualView.GetItem(pickerSource.SelectedIndex);
			VirtualView.SelectedIndex = pickerSource.SelectedIndex;
		}

		void UpdatePickerSelectedIndex(UIPickerView? pickerView, int formsIndex)
		{
			if (VirtualView == null || pickerView == null)
				return;

			var source = (PickerSource)pickerView.Model;
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
					virtualView.IsFocused = true;
				
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
			}

			void OnEnded(object? sender, EventArgs eventArgs)
			{
				if (Handler is not PickerHandler handler || handler._pickerView is not UIPickerView pickerView)
					return;

				PickerSource? model = (PickerSource)pickerView.Model;
				if (model.SelectedIndex != -1 && model.SelectedIndex != pickerView.SelectedRowInComponent(0))
				{
					pickerView.Select(model.SelectedIndex, 0, false);
				}

				if (VirtualView is IPicker virtualView)
				{
					virtualView.IsFocused = false;
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