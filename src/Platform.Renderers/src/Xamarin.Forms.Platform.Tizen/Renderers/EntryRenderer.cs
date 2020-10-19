using System;
using ElmSharp;
using EEntry = ElmSharp.Entry;
using IEntry = Xamarin.Forms.Platform.Tizen.Native.IEntry;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.Entry;

namespace Xamarin.Forms.Platform.Tizen
{
	public class EntryRenderer : ViewRenderer<Entry, EEntry>
	{
		SmartEvent _selectionCleared;
		bool _nativeSelectionIsUpdating;

		public EntryRenderer()
		{
			RegisterPropertyHandler(Entry.IsPasswordProperty, UpdateIsPassword);
			RegisterPropertyHandler(Entry.TextProperty, UpdateText);
			RegisterPropertyHandler(Entry.TextColorProperty, UpdateTextColor);
			RegisterPropertyHandler(Entry.FontSizeProperty, UpdateFontSize);
			RegisterPropertyHandler(Entry.FontFamilyProperty, UpdateFontFamily);
			RegisterPropertyHandler(Entry.FontAttributesProperty, UpdateFontAttributes);
			RegisterPropertyHandler(Entry.HorizontalTextAlignmentProperty, UpdateHorizontalTextAlignment);
			RegisterPropertyHandler(Entry.VerticalTextAlignmentProperty, UpdateVerticalTextAlignment);
			RegisterPropertyHandler(InputView.KeyboardProperty, UpdateKeyboard);
			RegisterPropertyHandler(Entry.PlaceholderProperty, UpdatePlaceholder);
			RegisterPropertyHandler(Entry.PlaceholderColorProperty, UpdatePlaceholderColor);
			RegisterPropertyHandler(InputView.MaxLengthProperty, UpdateMaxLength);
			RegisterPropertyHandler(Entry.ReturnTypeProperty, UpdateReturnType);
			RegisterPropertyHandler(InputView.IsSpellCheckEnabledProperty, UpdateIsSpellCheckEnabled);
			RegisterPropertyHandler(Entry.IsTextPredictionEnabledProperty, UpdateIsSpellCheckEnabled);
			RegisterPropertyHandler(Specific.FontWeightProperty, UpdateFontWeight);
			RegisterPropertyHandler(Entry.SelectionLengthProperty, UpdateSelectionLength);
			RegisterPropertyHandler(InputView.IsReadOnlyProperty, UpdateIsReadOnly);
			RegisterPropertyHandler(Entry.ClearButtonVisibilityProperty, UpdateClearButtonVisibility);
			RegisterPropertyHandler(Entry.CursorPositionProperty, UpdateSelectionLength);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			if (Control == null)
			{
				var entry = CreateNativeControl();
				entry.Activated += OnCompleted;
				entry.CursorChanged += OnCursorChanged;


				// In order to know when the selection is cleared, "selecton,cleared" event has been used.
				// Because CursorChanged event is still invoked with the selected text when an user clears selection. It is an known issue in EFL.
				_selectionCleared = new SmartEvent(entry, entry.RealHandle, ThemeConstants.Entry.Signals.SelectionCleared);
				_selectionCleared.On += OnSelectionCleared;

				if (entry is IEntry ie)
				{
					ie.TextChanged += OnTextChanged;
					ie.EntryLayoutFocused += OnFocused;
					ie.EntryLayoutUnfocused += OnUnfocused;
				}
				entry.PrependMarkUpFilter(MaxLengthFilter);
				SetNativeControl(entry);

				// An initial CursorPosition is set after layouting to avoid timing issue when the EditField entry is initialized.
				Device.BeginInvokeOnMainThread(() =>
				{
					UpdateSelectionLength(false);
				});

			}
			base.OnElementChanged(e);
		}

		protected virtual EEntry CreateNativeControl()
		{
			return new Native.EditfieldEntry(Forms.NativeParent)
			{
				IsSingleLine = true,
			};
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (null != Control)
				{
					Control.Activated -= OnCompleted;
					Control.CursorChanged -= OnCursorChanged;

					if (Control is IEntry ie)
					{
						ie.TextChanged -= OnTextChanged;
						ie.EntryLayoutFocused -= OnFocused;
						ie.EntryLayoutUnfocused -= OnUnfocused;
					}
				}
			}

			base.Dispose(disposing);
		}

		protected override Size MinimumSize()
		{
			Size measured;
			if (Control is Native.IMeasurable im)
			{
				measured = im.Measure(Control.MinimumWidth, Control.MinimumHeight).ToDP();
			}
			else
			{
				measured = base.MinimumSize();
			}

			return measured;
		}

		void OnTextChanged(object sender, EventArgs e)
		{
			Element.SetValueFromRenderer(Entry.TextProperty, Control.Text);
		}

		void OnCompleted(object sender, EventArgs e)
		{
			if (Element.ReturnType == ReturnType.Next)
			{
				FocusSearch(true)?.SetFocus(true);
			}
			else
			{
				Control.SetFocus(false);
			}
			((IEntryController)Element).SendCompleted();
		}

		void UpdateIsPassword()
		{
			Control.IsPassword = Element.IsPassword;
		}

		void UpdateText()
		{
			Control.Text = Element.Text ?? "";
			if (!Control.IsFocused)
			{
				Control.MoveCursorEnd();
			}
		}

		protected virtual void UpdateTextColor()
		{
			if (Control is IEntry ie)
			{
				ie.TextColor = Element.TextColor.ToNative();
			}
		}

		void UpdateFontSize()
		{
			if (Control is IEntry ie)
			{
				ie.FontSize = Element.FontSize;
			}
		}

		void UpdateFontFamily()
		{
			if (Control is IEntry ie)
			{
				ie.FontFamily = Element.FontFamily.ToNativeFontFamily();
			}
		}

		void UpdateFontAttributes()
		{
			if (Control is IEntry ie)
			{
				ie.FontAttributes = Element.FontAttributes;
			}
		}

		void UpdateHorizontalTextAlignment()
		{
			if (Control is IEntry ie)
			{
				ie.HorizontalTextAlignment = Element.HorizontalTextAlignment.ToNative();
			}
		}

		void UpdateVerticalTextAlignment()
		{
			Control.SetVerticalTextAlignment(Element.VerticalTextAlignment.ToNativeDouble());
			Control.SetVerticalPlaceHolderTextAlignment(Element.VerticalTextAlignment.ToNativeDouble());
		}

		void UpdateKeyboard(bool initialize)
		{
			if (initialize && Element.Keyboard == Keyboard.Default)
				return;

			(Control as IEntry)?.UpdateKeyboard(Element.Keyboard, Element.IsSpellCheckEnabled, Element.IsTextPredictionEnabled);
		}

		void UpdateIsSpellCheckEnabled()
		{
			Control.InputHint = Element.Keyboard.ToInputHints(Element.IsSpellCheckEnabled, Element.IsTextPredictionEnabled);
		}

		void UpdatePlaceholder()
		{
			if (Control is IEntry ie)
			{
				ie.Placeholder = Element.Placeholder;
			}
		}

		void UpdatePlaceholderColor()
		{
			if (Control is IEntry ie)
			{
				ie.PlaceholderColor = Element.PlaceholderColor.ToNative();
			}
		}

		void UpdateFontWeight()
		{
			if (Control is IEntry ie)
			{
				ie.FontWeight = Specific.GetFontWeight(Element);
			}
		}

		void UpdateMaxLength()
		{
			if (Control.Text.Length > Element.MaxLength)
				Control.Text = Control.Text.Substring(0, Element.MaxLength);
		}

		string MaxLengthFilter(ElmSharp.Entry entry, string s)
		{
			if (entry.Text.Length < Element.MaxLength)
				return s;

			return null;
		}

		void UpdateReturnType()
		{
			Control.SetInputPanelReturnKeyType(Element.ReturnType.ToInputPanelReturnKeyType());
		}

		void UpdateSelectionLength(bool initialize)
		{
			if (initialize || _nativeSelectionIsUpdating)
				return;

			var start = GetSelectionStart();
			var end = GetSelectionEnd(start);
			var selectionLength = end - start;

			if (selectionLength != Element.SelectionLength)
				SetSelectionLengthFromRenderer(selectionLength);

			if (selectionLength > 0)
			{
				Control.SetSelectionRegion(start, end);
			}
			else
			{
				Control.CursorPosition = Element.CursorPosition;
			}
		}

		int GetSelectionEnd(int start)
		{
			var end = start;

			if (Element.IsSet(Entry.SelectionLengthProperty))
				end = Math.Min((start + Element.SelectionLength), Element.Text?.Length ?? 0);

			return end;
		}

		int GetSelectionStart()
		{
			var start = Element.Text?.Length ?? 0;
			var cursorPosition = Element.CursorPosition;

			if (Element.IsSet(Entry.CursorPositionProperty))
				start = Math.Min(start, cursorPosition);

			if (start != cursorPosition)
				SetCursorPositionFromRenderer(start);

			return start;
		}

		void OnSelectionCleared(object sender, EventArgs e)
		{
			if (Control.IsFocused)
			{
				SetSelectionLengthFromRenderer(0);
				SetCursorPositionFromRenderer(Control.CursorPosition);
			}
		}

		void OnCursorChanged(object sender, EventArgs e)
		{
			var position = Control.CursorPosition;

			Control.GetSelectRegion(out int start, out int end);

			if (start > -1)
			{
				position = (start < end) ? start : end;
				var selectionLength = Math.Abs(end - start);
				SetSelectionLengthFromRenderer(selectionLength);
			}

			SetCursorPositionFromRenderer(position);
		}

		void SetCursorPositionFromRenderer(int position)
		{
			try
			{
				_nativeSelectionIsUpdating = true;
				Element?.SetValueFromRenderer(Entry.CursorPositionProperty, position);
			}
			catch (Exception ex)
			{
				Log.Error($"Failed to set CursorPosition from renderer: {ex}");
			}
			finally
			{
				_nativeSelectionIsUpdating = false;
			}
		}

		void SetSelectionLengthFromRenderer(int selectionLength)
		{
			try
			{
				_nativeSelectionIsUpdating = true;
				Element?.SetValueFromRenderer(Entry.SelectionLengthProperty, selectionLength);
			}
			catch (Exception ex)
			{
				Log.Error($"Failed to set SelectionLength from renderer: {ex}");
			}
			finally
			{
				_nativeSelectionIsUpdating = false;
			}
		}

		void UpdateIsReadOnly()
		{
			Control.IsEditable = !Element.IsReadOnly;
		}

		void UpdateClearButtonVisibility(bool init)
		{
			if (Element.ClearButtonVisibility == ClearButtonVisibility.WhileEditing)
			{
				if (Control is Native.EditfieldEntry editfieldEntry)
				{
					editfieldEntry.EnableClearButton = true;
				}
			}
			else if (!init)
			{
				if (Control is Native.EditfieldEntry editfieldEntry)
				{
					editfieldEntry.EnableClearButton = false;
				}
			}
		}
	}
}
