using System;
using System.ComponentModel;
using System.Collections.Generic;
using ElmSharp;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class PickerRenderer : ViewRenderer<Picker, Native.Button>
	{
		internal List _list;
		internal Native.Dialog _dialog;
		Dictionary<ListItem, int> _itemToItemNumber = new Dictionary<ListItem, int>();

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.Clicked -= OnClicked;
					CleanView();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			if (Control == null)
			{
				SetNativeControl (new Native.Button(Forms.NativeParent));
				Control.Clicked += OnClicked;
			}

			UpdateSelectedIndex();
			UpdateTextColor();
			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Picker.SelectedIndexProperty.PropertyName)
			{
				UpdateSelectedIndex();
			}
			else if (e.PropertyName == Picker.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
			}
		}

		void UpdateSelectedIndex()
		{
			Control.Text = (Element.SelectedIndex == -1 || Element.Items == null ?
				"" : Element.Items[Element.SelectedIndex]);
		}

		void UpdateTextColor()
		{
			Control.TextColor = Element.TextColor.ToNative();
		}

		void OnClicked(object sender, EventArgs e)
		{
			int i = 0;
			_dialog = new Native.Dialog(Forms.NativeParent);
			_list = new List(_dialog);
			_dialog.AlignmentX = -1;
			_dialog.AlignmentY = -1;

			_dialog.Title = Element.Title;
			_dialog.Dismissed += OnDialogDismissed;
			_dialog.BackButtonPressed += (object senders, EventArgs es) =>
			{
				_dialog.Dismiss();
			};

			foreach (var s in Element.Items)
			{
				ListItem item = _list.Append(s);
				_itemToItemNumber[item] = i;
				i++;
			}
			_list.ItemSelected += OnItemSelected;
			_dialog.Content = _list;

			_dialog.Show();
			_list.Show();
		}

		void OnItemSelected(object senderObject, EventArgs ev)
		{
			Element.SelectedIndex = _itemToItemNumber[(senderObject as List).SelectedItem];
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
