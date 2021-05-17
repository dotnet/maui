using System;
using System.Collections.Generic;
using Tizen.UIExtensions.ElmSharp;
using TEntry = Tizen.UIExtensions.ElmSharp.Entry;
using TTextAlignment = Tizen.UIExtensions.Common.TextAlignment;
using EcoreMainloop = ElmSharp.EcoreMainloop;
using List = ElmSharp.List;
using ListItem = ElmSharp.ListItem;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : EViewHandler<IPicker, TEntry>
	{
		List? _list;
		Dialog? _dialog;
		Dictionary<ListItem, int> _itemToItemNumber = new Dictionary<ListItem, int>();

		protected override TEntry CreateNativeView() => new EditfieldEntry(NativeParent)
		{
			IsSingleLine = true,
			InputPanelShowByOnDemand = true,
			IsEditable = false,
			HorizontalTextAlignment = TTextAlignment.Center
		};

		protected override void ConnectHandler(TEntry nativeView)
		{
			nativeView.SetVerticalTextAlignment(0.5);

			nativeView.TextBlockFocused += OnTextBlockFocused;
			nativeView.EntryLayoutFocused += OnFocused;
			nativeView.EntryLayoutUnfocused += OnUnfocused;

			if (DeviceInfo.IsTV)
			{
				nativeView.EntryLayoutFocused += OnLayoutFocused;
				nativeView.EntryLayoutUnfocused += OnLayoutUnfocused;
			}

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(TEntry nativeView)
		{
			nativeView.TextBlockFocused -= OnTextBlockFocused;
			nativeView.EntryLayoutFocused -= OnFocused;
			nativeView.EntryLayoutUnfocused -= OnUnfocused;
			if (DeviceInfo.IsTV)
			{
				nativeView.EntryLayoutFocused -= OnLayoutFocused;
				nativeView.EntryLayoutUnfocused -= OnLayoutUnfocused;
			}
			CleanView();
			base.DisconnectHandler(nativeView);
		}

		void Reload()
		{
			if (VirtualView == null || NativeView == null)
				return;

			NativeView.UpdatePicker(VirtualView);
		}

		public static void MapReload(PickerHandler handler, IPicker picker) => handler.Reload();

		public static void MapFont(PickerHandler handler, IPicker picker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.NativeView?.UpdateFont(picker, fontManager);
		}

		public static void MapHorizontalTextAlignment(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(picker);
		}

		public static void MapTextColor(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateTextColor(picker);
		}

		public static void MapTitle(PickerHandler handler, IPicker picker) 
		{
			handler.NativeView?.UpdateTitle(picker);
		}

		public static void MapSelectedIndex(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateSelectedIndex(picker);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(PickerHandler handler, IPicker picker) { }

		void OnLayoutFocused(object sender, EventArgs e)
		{
			if (NativeView == null)
				return;

			NativeView.FontSize = NativeView.FontSize * 1.5;
		}

		void OnLayoutUnfocused(object sender, EventArgs e)
		{
			if (NativeView == null)
				return;

			NativeView.FontSize = NativeView.FontSize / 1.5;
		}

		void OnTextBlockFocused(object sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			// For EFL Entry, the event will occur even if it is currently disabled.
			// If the problem is resolved, no conditional statement is required.
			if (VirtualView.IsEnabled)
			{
				int i = 0;
				_dialog = new Dialog(NativeParent)
				{
					AlignmentX = -1,
					AlignmentY = -1,
					Title = VirtualView.Title,
				};
				_dialog.Dismissed += OnDialogDismissed;
				_dialog.BackButtonPressed += (object senders, EventArgs es) =>
				{
					_dialog.Dismiss();
				};

				_list = new List(_dialog);
				foreach (var s in VirtualView.GetItemsAsArray())
				{
					ListItem item = _list.Append(s);
					_itemToItemNumber[item] = i;
					i++;
				}
				_list.ItemSelected += OnItemSelected;
				_dialog.Content = _list;

				// You need to call Show() after ui thread occupation because of EFL problem.
				// Otherwise, the content of the popup will not receive focus.
				EcoreMainloop.Post(() =>
				{
					_dialog.Show();
					_list.Show();
				});
			}
		}

		void OnItemSelected(object senderObject, EventArgs ev)
		{
			if (VirtualView == null || NativeView == null || _dialog == null)
				return;

			VirtualView.SelectedIndex = _itemToItemNumber[(senderObject as List)!.SelectedItem];
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