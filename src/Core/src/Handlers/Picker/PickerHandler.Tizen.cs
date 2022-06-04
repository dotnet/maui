using System;
using System.Collections.Generic;
using Tizen.UIExtensions.ElmSharp;
using TEntry = Tizen.UIExtensions.ElmSharp.Entry;
using TTextAlignment = Tizen.UIExtensions.Common.TextAlignment;
using DeviceInfo = Tizen.UIExtensions.Common.DeviceInfo;
using EcoreMainloop = ElmSharp.EcoreMainloop;
using List = ElmSharp.List;
using ListItem = ElmSharp.ListItem;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, TEntry>
	{
		List? _list;
		Dialog? _dialog;
		Dictionary<ListItem, int> _itemToItemNumber = new Dictionary<ListItem, int>();

		protected override TEntry CreatePlatformView()
		{
			return new EditfieldEntry(PlatformParent)
			{
				IsSingleLine = true,
				InputPanelShowByOnDemand = true,
				IsEditable = false,
				HorizontalTextAlignment = TTextAlignment.Center
			};
		}

		protected override void ConnectHandler(TEntry platformView)
		{
			platformView.SetVerticalTextAlignment(0.5);

			platformView.TextBlockFocused += OnTextBlockFocused;
			platformView.EntryLayoutFocused += OnFocused;
			platformView.EntryLayoutUnfocused += OnUnfocused;

			if (DeviceInfo.IsTV)
			{
				platformView.EntryLayoutFocused += OnLayoutFocused;
				platformView.EntryLayoutUnfocused += OnLayoutUnfocused;
			}

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(TEntry platformView)
		{
			platformView.TextBlockFocused -= OnTextBlockFocused;
			platformView.EntryLayoutFocused -= OnFocused;
			platformView.EntryLayoutUnfocused -= OnUnfocused;
			if (DeviceInfo.IsTV)
			{
				platformView.EntryLayoutFocused -= OnLayoutFocused;
				platformView.EntryLayoutUnfocused -= OnLayoutUnfocused;
			}
			CleanView();
			base.DisconnectHandler(platformView);
		}

		static void Reload(IPickerHandler handler)
		{
			if (handler.VirtualView == null || handler.PlatformView == null)
				return;

			handler.PlatformView.UpdatePicker(handler.VirtualView);
		}

		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		public static void MapTitleColor(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTitleColor(picker);
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

		public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(picker);
		}

		public static void MapTextColor(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTextColor(picker);
		}

		public static void MapTitle(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTitle(picker);
		}

		public static void MapSelectedIndex(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateSelectedIndex(picker);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(IPickerHandler handler, IPicker picker) { }

		void OnLayoutFocused(object? sender, EventArgs e)
		{
			if (PlatformView == null)
				return;

			PlatformView.FontSize = PlatformView.FontSize * 1.5;
		}

		void OnLayoutUnfocused(object? sender, EventArgs e)
		{
			if (PlatformView == null)
				return;

			PlatformView.FontSize = PlatformView.FontSize / 1.5;
		}

		void OnTextBlockFocused(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			// For EFL Entry, the event will occur even if it is currently disabled.
			// If the problem is resolved, no conditional statement is required.
			if (VirtualView.IsEnabled)
			{
				int i = 0;
				_dialog = new Dialog(PlatformParent)
				{
					AlignmentX = -1,
					AlignmentY = -1,
					Title = VirtualView.Title,
				};
				_dialog.Dismissed += OnDialogDismissed;
				_dialog.BackButtonPressed += (object? senders, EventArgs es) =>
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

		void OnItemSelected(object? senderObject, EventArgs ev)
		{
			if (VirtualView == null || PlatformView == null || _dialog == null)
				return;

			VirtualView.SelectedIndex = _itemToItemNumber[(senderObject as List)!.SelectedItem];
			_dialog.Dismiss();
		}

		void OnDialogDismissed(object? sender, EventArgs e)
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