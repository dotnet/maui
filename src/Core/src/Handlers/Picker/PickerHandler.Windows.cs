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
		}

		protected override void DisconnectHandler(ComboBox platformView)
		{
			platformView.SelectionChanged -= OnControlSelectionChanged;
			platformView.DropDownOpened -= OnMauiComboBoxDropDownOpened;
		}

		// Updating ItemSource Resets the SelectedIndex.
		// Which propagates that change to the virtualview
		// We don't want the virtual views selected index to change
		// when updating the itmmsource.
		// The ItemSource should probably be reworked to just be an OC that's
		// kept in sync
		internal bool UpdatingItemSource { get; set; }

		internal void SetUpdatingItemSource(bool updatingItemSource)
		{
			UpdatingItemSource = updatingItemSource;

			if (!updatingItemSource)
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

		// TODO: Uncomment me on NET8 [Obsolete]
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

		void OnControlSelectionChanged(object? sender, WSelectionChangedEventArgs e)
		{
			if (PlatformView == null)
				return;

			if (VirtualView != null && !UpdatingItemSource)
				VirtualView.SelectedIndex = PlatformView.SelectedIndex;

			PlatformView.MinWidth = 0;
		}

		static void OnMauiComboBoxDropDownOpened(object? sender, object e)
		{
			ComboBox? comboBox = sender as ComboBox;
			if (comboBox == null)
				return;
			comboBox.MinWidth = comboBox.ActualWidth;
		}
	}
}
