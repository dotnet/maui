using System;
using System.Collections.Specialized;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, MauiPicker>
	{
		UIPickerView? _pickerView;

#if !MACCATALYST
		protected override MauiPicker CreatePlatformView()
		{
			_pickerView = new UIPickerView();

			var platformPicker = new MauiPicker(_pickerView) { BorderStyle = UITextBorderStyle.RoundedRect };
			platformPicker.InputView = _pickerView;
			platformPicker.InputAccessoryView = new MauiDoneAccessoryView(() =>
			{
				FinishSelectItem(_pickerView, platformPicker);
			});

			platformPicker.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			platformPicker.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			platformPicker.InputAssistantItem.LeadingBarButtonGroups = null;
			platformPicker.InputAssistantItem.TrailingBarButtonGroups = null;
			platformPicker.AccessibilityTraits = UIAccessibilityTrait.Button;

			_pickerView.Model = new PickerSource(this);

			return platformPicker;
		}
#else
		protected override MauiPicker CreatePlatformView()
		{
			var platformPicker = new MauiPicker(null) { BorderStyle = UITextBorderStyle.RoundedRect };

			platformPicker.ShouldBeginEditing += (textField) =>
			{
				var alertController = CreateAlert(textField);
				var platformWindow = MauiContext?.GetPlatformWindow();
				platformWindow?.BeginInvokeOnMainThread(() =>
				{
					_ = platformWindow?.RootViewController?.PresentViewControllerAsync(alertController, true);
				});
				return false;
			};

			return platformPicker;
		}

		UIAlertController CreateAlert(UITextField uITextField)
		{
			var paddingTitle = 0;
			if (!string.IsNullOrEmpty(VirtualView.Title))
				paddingTitle += 25;

			var pickerHeight = 240;
			var frame = new RectangleF(0, paddingTitle, 269, pickerHeight);
			var pickerView = new UIPickerView(frame);
			pickerView.Model = new PickerSource(this);
			pickerView?.ReloadAllComponents();

			var pickerController = UIAlertController.Create(VirtualView.Title, "", UIAlertControllerStyle.ActionSheet);

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

			return pickerController;
		}
#endif

		internal bool UpdateImmediately { get; set; }

		protected override void ConnectHandler(MauiPicker platformView)
		{
			platformView.EditingDidBegin += OnStarted;
			platformView.EditingDidEnd += OnEnded;
			platformView.EditingChanged += OnEditing;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiPicker platformView)
		{
			platformView.EditingDidBegin -= OnStarted;
			platformView.EditingDidEnd -= OnEnded;
			platformView.EditingChanged -= OnEditing;

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

		void OnStarted(object? sender, EventArgs eventArgs)
		{
			if (VirtualView != null)
				VirtualView.IsFocused = true;
		}

		void OnEnded(object? sender, EventArgs eventArgs)
		{
			if (_pickerView == null)
				return;

			PickerSource? model = (PickerSource)_pickerView.Model;

			if (model.SelectedIndex != -1 && model.SelectedIndex != _pickerView.SelectedRowInComponent(0))
			{
				_pickerView.Select(model.SelectedIndex, 0, false);
			}

			if (VirtualView != null)
				VirtualView.IsFocused = false;
		}

		void OnEditing(object? sender, EventArgs eventArgs)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			// Reset the TextField's Text so it appears as if typing with a keyboard does not work.
			var selectedIndex = VirtualView.SelectedIndex;

			PlatformView.Text = VirtualView.GetItem(selectedIndex);

			// Also clears the undo stack (undo/redo possible on iPads)
			PlatformView.UndoManager?.RemoveAllActions();
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

		void FinishSelectItem(UIPickerView? pickerView, UITextField textField)
		{
			var pickerSource = pickerView?.Model as PickerSource;
			var count = VirtualView?.GetCount() ?? 0;
			if (pickerSource != null && pickerSource.SelectedIndex == -1 && count > 0)
				UpdatePickerSelectedIndex(pickerView, 0);

			UpdatePickerFromPickerSource(pickerSource);
			textField.ResignFirstResponder();
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