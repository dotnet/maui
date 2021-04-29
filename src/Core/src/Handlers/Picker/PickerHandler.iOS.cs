using System;
using System.Collections.Specialized;
using Microsoft.Extensions.DependencyInjection;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, MauiPicker>
	{
		UIPickerView? _pickerView;

		protected override MauiPicker CreateNativeView()
		{
			_pickerView = new UIPickerView();

			var nativePicker = new MauiPicker(_pickerView) { BorderStyle = UITextBorderStyle.RoundedRect };

			var width = UIScreen.MainScreen.Bounds.Width;
			var toolbar = new UIToolbar(new RectangleF(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);

			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
			{
				var pickerSource = (PickerSource)_pickerView.Model;
				var count = VirtualView?.GetCount() ?? 0;
				if (pickerSource.SelectedIndex == -1 && count > 0)
					UpdatePickerSelectedIndex(0);

				if (VirtualView?.SelectedIndex == -1 && count > 0)
				{
					NativeView?.SetSelectedIndex(VirtualView, 0);
				}

				UpdatePickerFromPickerSource(pickerSource);
				nativePicker.ResignFirstResponder();
			});

			toolbar.SetItems(new[] { spacer, doneButton }, false);

			nativePicker.InputView = _pickerView;
			nativePicker.InputAccessoryView = toolbar;

			nativePicker.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			nativePicker.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
			{
				nativePicker.InputAssistantItem.LeadingBarButtonGroups = null;
				nativePicker.InputAssistantItem.TrailingBarButtonGroups = null;
			}

			nativePicker.AccessibilityTraits = UIAccessibilityTrait.Button;

			_pickerView.Model = new PickerSource(VirtualView);

			return nativePicker;
		}

		protected override void ConnectHandler(MauiPicker nativeView)
		{
			nativeView.EditingDidEnd += OnEnded;
			nativeView.EditingChanged += OnEditing;


			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiPicker nativeView)
		{
			nativeView.EditingDidEnd -= OnEnded;
			nativeView.EditingChanged -= OnEditing;

			if (_pickerView != null)
			{
				if (_pickerView.Model != null)
				{
					_pickerView.Model.Dispose();
					_pickerView.Model = null;
				}

				_pickerView.RemoveFromSuperview();
				_pickerView.Dispose();
				_pickerView = null;
			}

			base.DisconnectHandler(nativeView);
		}
		void Reload()
		{
			if (VirtualView == null || NativeView == null)
				return;

			NativeView.UpdatePicker(VirtualView);
		}

		public static void MapReload(PickerHandler handler, IPicker picker) => handler.Reload();

		public static void MapTitle(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateTitle(picker);
		}

		public static void MapSelectedIndex(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateSelectedIndex(picker);
		}

		public static void MapCharacterSpacing(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateCharacterSpacing(picker);
		}

		public static void MapFont(PickerHandler handler, IPicker picker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(picker, fontManager);
		}

		public static void MapHorizontalTextAlignment(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(picker);
		}

		[MissingMapper]
		public static void MapTextColor(PickerHandler handler, IPicker view) { }

		void OnEnded(object? sender, EventArgs eventArgs)
		{
			if (_pickerView == null)
				return;

			PickerSource? model = (PickerSource)_pickerView.Model;

			if (model.SelectedIndex != -1 && model.SelectedIndex != _pickerView.SelectedRowInComponent(0))
			{
				_pickerView.Select(model.SelectedIndex, 0, false);
			}
		}

		void OnEditing(object? sender, EventArgs eventArgs)
		{
			if (VirtualView == null || NativeView == null)
				return;

			// Reset the TextField's Text so it appears as if typing with a keyboard does not work.
			var selectedIndex = VirtualView.SelectedIndex;

			NativeView.Text = VirtualView.GetItem(selectedIndex);

			// Also clears the undo stack (undo/redo possible on iPads)
			NativeView.UndoManager.RemoveAllActions();
		}

		void UpdatePickerFromPickerSource(PickerSource pickerSource)
		{
			if (VirtualView == null || NativeView == null)
				return;

			NativeView.Text = VirtualView.GetItem(pickerSource.SelectedIndex);
			VirtualView.SelectedIndex = pickerSource.SelectedIndex;
		}

		void UpdatePickerSelectedIndex(int formsIndex)
		{
			if (VirtualView == null || _pickerView == null)
				return;

			var source = (PickerSource)_pickerView.Model;
			source.SelectedIndex = formsIndex;
			_pickerView.Select(Math.Max(formsIndex, 0), 0, true);
		}
	}

	public class PickerSource : UIPickerViewModel
	{
		IPicker? _virtualView;
		bool _disposed;

		public PickerSource(IPicker? virtualView)
		{
			_virtualView = virtualView;
		}

		public int SelectedIndex { get; internal set; }

		public override nint GetComponentCount(UIPickerView picker)
		{
			return 1;
		}

		public override nint GetRowsInComponent(UIPickerView pickerView, nint component) =>
			_virtualView?.GetCount() ?? 0;

		public override string GetTitle(UIPickerView picker, nint row, nint component) =>
			_virtualView?.GetItem((int)row) ?? "";

		public override void Selected(UIPickerView picker, nint row, nint component) =>
			SelectedIndex = (int)row;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
				_virtualView = null;

			base.Dispose(disposing);
		}
	}
}