#nullable enable
using System;
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

			if (VirtualView != null)
				platformPicker.ItemsSource = new ItemDelegateList<string>(VirtualView);

			platformPicker.DropDownOpened += OnMauiComboBoxDropDownOpened;
			platformPicker.SelectionChanged += OnMauiComboBoxSelectionChanged;

			return platformPicker;
		}

		protected override void ConnectHandler(ComboBox platformView)
		{
			platformView.SelectionChanged += OnControlSelectionChanged;

			if (VirtualView.Items is INotifyCollectionChanged notifyCollection)
				notifyCollection.CollectionChanged += OnRowsCollectionChanged;
		}

		protected override void DisconnectHandler(ComboBox platformView)
		{
			platformView.SelectionChanged -= OnControlSelectionChanged;

			if (VirtualView.Items is INotifyCollectionChanged notifyCollection)
				notifyCollection.CollectionChanged -= OnRowsCollectionChanged;
		}

		static void Reload(IPickerHandler handler)
		{
			if (handler.VirtualView == null || handler.PlatformView == null)
				return;
			handler.PlatformView.ItemsSource = new ItemDelegateList<string>(handler.VirtualView!);
		}

		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		public static void MapTitle(IPickerHandler handler, IPicker picker) 
		{
			handler.PlatformView?.UpdateTitle(picker);
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
			if (handler is PickerHandler platformHandler)
			{
				platformHandler.PlatformView?.UpdateTextColor(picker);
			}
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
			if (VirtualView != null && PlatformView != null)
				VirtualView.SelectedIndex = PlatformView.SelectedIndex;
		}

		void OnRowsCollectionChanged(object? sender, EventArgs e)
		{
			Reload(this);
		}

		static void OnMauiComboBoxDropDownOpened(object? sender, object e)
		{
			ComboBox? comboBox = sender as ComboBox;
			if (comboBox == null)
				return;
			comboBox.MinWidth = comboBox.ActualWidth;
		}

		static void OnMauiComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
		{
			ComboBox? comboBox = sender as ComboBox;
			if (comboBox == null)
				return;
			comboBox.MinWidth = 0;
		}
	}
}
