#nullable enable
using System;
using System.Collections.Specialized;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, ComboBox>
	{
		MauiComboBox? GetMauiPicker() => PlatformView as MauiComboBox;
		static MauiComboBox? GetMauiPicker(IPickerHandler handler) => handler.PlatformView as MauiComboBox;

		protected override ComboBox CreatePlatformView()
		{
			var platformPicker = new MauiComboBox();

			if (VirtualView != null)
				platformPicker.ItemsSource = new ItemDelegateList<string>(VirtualView);

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
			handler.PlatformView.ItemsSource = new ItemDelegateList<string>(handler.VirtualView);
		}

		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		public static void MapTitle(IPickerHandler handler, IPicker picker) 
		{
			GetMauiPicker(handler)?.UpdateTitle(picker);
		}

		public static void MapTitleColor(IPickerHandler handler, IPicker picker)
		{
			GetMauiPicker(handler)?.UpdateTitle(picker);
		}

		public static void MapSelectedIndex(IPickerHandler handler, IPicker picker)
		{
			GetMauiPicker(handler)?.UpdateSelectedIndex(picker);
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
			GetMauiPicker(handler)?.UpdateHorizontalTextAlignment(picker);
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
	}
}
