using System;
using System.Collections.Generic;
using Xamarin.Forms.Platform.Tizen.Native;
using Xamarin.Forms.Platform.Tizen.Native.Watch;
using ElmSharp;
using EEntry = ElmSharp.Entry;

namespace Xamarin.Forms.Platform.Tizen
{
	public class PickerRenderer : ViewRenderer<Picker, EEntry>
	{
		List _list;
		Dialog _dialog;
		Dictionary<ListItem, int> _itemToItemNumber = new Dictionary<ListItem, int>();

		public PickerRenderer()
		{
			RegisterPropertyHandler(Picker.SelectedIndexProperty, UpdateSelectedIndex);
			RegisterPropertyHandler(Picker.TextColorProperty, UpdateTextColor);
			RegisterPropertyHandler(Picker.FontSizeProperty, UpdateFontSize);
			RegisterPropertyHandler(Picker.FontFamilyProperty, UpdateFontFamily);
			RegisterPropertyHandler(Picker.FontAttributesProperty, UpdateFontAttributes);
			RegisterPropertyHandler(Picker.TitleProperty, UpdateTitle);
			RegisterPropertyHandler(Picker.TitleColorProperty, UpdateTitleColor);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					if (Control is IEntry ie)
					{
						ie.TextBlockFocused -= OnTextBlockFocused;
						if (Device.Idiom == TargetIdiom.TV)
						{
							ie.EntryLayoutFocused -= OnLayoutFocused;
							ie.EntryLayoutUnfocused -= OnLayoutUnfocused;
						}
					}
					CleanView();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			if (Control == null)
			{
				var entry = CreateNativeControl();
				entry.SetVerticalTextAlignment("elm.text", 0.5);
				if (entry is IEntry ie)
				{
					ie.TextBlockFocused += OnTextBlockFocused;

					if (Device.Idiom == TargetIdiom.TV)
					{
						ie.EntryLayoutFocused += OnLayoutFocused;
						ie.EntryLayoutUnfocused += OnLayoutUnfocused;
					}
				}
				SetNativeControl(entry);
			}
			base.OnElementChanged(e);
		}

		protected virtual EEntry CreateNativeControl()
		{
			return new EditfieldEntry(Forms.NativeParent)
			{
				IsSingleLine = true,
				InputPanelShowByOnDemand = true,
				IsEditable = false,
				HorizontalTextAlignment = Native.TextAlignment.Center
			};
		}

		protected virtual void UpdateSelectedIndex()
		{
			Control.Text = (Element.SelectedIndex == -1 || Element.Items == null ?
				"" : Element.Items[Element.SelectedIndex]);
		}

		protected virtual void UpdateTitleColor()
		{
			if (Control is IEntry ie)
			{
				ie.PlaceholderColor = Element.TitleColor.ToNative();
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
				ie.FontFamily = Element.FontFamily;
			}
		}

		void UpdateFontAttributes()
		{
			if (Control is IEntry ie)
			{
				ie.FontAttributes = Element.FontAttributes;
			}
		}

		void UpdateTitle()
		{
			if (Control is IEntry ie)
			{
				ie.Placeholder = Element.Title;
			}
		}

		void OnLayoutFocused(object sender, EventArgs e)
		{
			if (Control is IEntry ie)
			{
				ie.FontSize = ie.FontSize * 1.5;
			}
		}

		void OnLayoutUnfocused(object sender, EventArgs e)
		{
			if (Control is IEntry ie)
			{
				ie.FontSize = ie.FontSize / 1.5;
			}
		}

		void OnTextBlockFocused(object sender, EventArgs e)
		{
			// For EFL Entry, the event will occur even if it is currently disabled.
			// If the problem is resolved, no conditional statement is required.
			if (Element.IsEnabled)
			{
				int i = 0;
				if (Device.Idiom == TargetIdiom.Watch)
				{
					_dialog = new WatchDialog(Forms.NativeParent, false);
				}
				else
				{
					_dialog = new Dialog(Forms.NativeParent);
				}
				_dialog.AlignmentX = -1;
				_dialog.AlignmentY = -1;
				_dialog.Title = Element.Title;
				_dialog.TitleColor = Element.TitleColor.ToNative();
				_dialog.Dismissed += OnDialogDismissed;
				_dialog.BackButtonPressed += (object senders, EventArgs es) =>
				{
					_dialog.Dismiss();
				};

				_list = new List(_dialog);
				foreach (var s in Element.Items)
				{
					ListItem item = _list.Append(s);
					_itemToItemNumber[item] = i;
					i++;
				}
				_list.ItemSelected += OnItemSelected;
				_dialog.Content = _list;

				// You need to call Show() after ui thread occupation because of EFL problem.
				// Otherwise, the content of the popup will not receive focus.
				Device.BeginInvokeOnMainThread(() =>
				{
					_dialog.Show();
					_list.Show();
				});
			}
		}

		void OnItemSelected(object senderObject, EventArgs ev)
		{
			Element.SetValueFromRenderer(Picker.SelectedIndexProperty, _itemToItemNumber[(senderObject as List).SelectedItem]);
			_dialog.Dismiss();
		}

		void OnDialogDismissed(object sender, EventArgs e)
		{
			CleanView();
		}

		void CleanView()
		{
			if (null != _list)
			{
				_list.Unrealize();
				_itemToItemNumber.Clear();
				_list = null;
			}
			if (null != _dialog)
			{
				_dialog.Unrealize();
				_dialog = null;
			}
		}
	}
}
