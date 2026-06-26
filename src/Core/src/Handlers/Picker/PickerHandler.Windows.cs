#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.UI.Xaml.Controls;
using WSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, ComboBox>
	{
		protected override ComboBox CreatePlatformView()
		{
			var platformPicker = new ComboBox();
			return platformPicker;
		}

		protected override void ConnectHandler(ComboBox platformView)
		{
			platformView.SelectionChanged += OnControlSelectionChanged;
			platformView.DropDownOpened += OnMauiComboBoxDropDownOpened;
			platformView.DropDownClosed += OnMauiComboBoxDropDownClosed;
		}

		protected override void DisconnectHandler(ComboBox platformView)
		{
			platformView.SelectionChanged -= OnControlSelectionChanged;
			platformView.DropDownOpened -= OnMauiComboBoxDropDownOpened;
			platformView.DropDownClosed -= OnMauiComboBoxDropDownClosed;
		}

		// Updating ItemSource Resets the SelectedIndex.
		// Which propagates that change to the VirtualView
		// We don't want the virtual views selected index to change
		// when updating the ItemSource.
		// The ItemSource should probably be reworked to just be an OC that's
		// kept in sync
		internal bool UpdatingItemSource { get; set; }

		internal void SetUpdatingItemSource(bool updatingItemSource)
		{
			UpdatingItemSource = updatingItemSource;

			if (!updatingItemSource && VirtualView is not null && (VirtualView.SelectedIndex < VirtualView.GetCount()))
				UpdateValue(nameof(IPicker.SelectedIndex));
		}

		static void Reload(IPickerHandler handler)
		{
			if (handler is PickerHandler ph1)
				ph1.SetUpdatingItemSource(true);

			handler.PlatformView.ItemsSource = new ItemDelegateList<string>(handler.VirtualView);

			if (handler is PickerHandler ph2)
				ph2.SetUpdatingItemSource(false);
		}

		[Obsolete("Use Microsoft.Maui.Handlers.PickerHandler.MapItems instead")]
		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		internal static void MapItems(IPickerHandler handler, IPicker picker) => Reload(handler);

		public static void MapTitle(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTitle(picker);
			handler.UpdateValue(nameof(IView.Semantics));
		}

		public static void MapTitleColor(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTitle(picker);
		}

		public static void MapBackground(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateBackground(picker);
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

		public static void MapTextColor(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView.UpdateTextColor(picker);
		}

		public static void MapHorizontalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(picker);
		}

		public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(picker);
		}

		internal static void MapIsOpen(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateIsOpen(picker);
		}

		void OnControlSelectionChanged(object? sender, WSelectionChangedEventArgs e)
		{
			if (PlatformView == null)
				return;

			if (VirtualView != null && !UpdatingItemSource)
				VirtualView.SelectedIndex = PlatformView.SelectedIndex;

			PlatformView.MinWidth = 0;
		}

		void OnMauiComboBoxDropDownOpened(object? sender, object e)
		{
			ComboBox? comboBox = sender as ComboBox;

			if (comboBox is null)
				return;

			comboBox.MinWidth = comboBox.ActualWidth;

			if (VirtualView is null)
				return;

			VirtualView.IsOpen = true;
		}

		void OnMauiComboBoxDropDownClosed(object? sender, object e)
		{
			if (VirtualView is null)
				return;

			if (sender is ComboBox comboBox && comboBox.MinWidth > 0)
			{
				//Reset the MinWidth to allow ComboBox to resize when the parent's size changes
				comboBox.MinWidth = 0;
			}

			VirtualView.IsOpen = false;
		}
	}
}
